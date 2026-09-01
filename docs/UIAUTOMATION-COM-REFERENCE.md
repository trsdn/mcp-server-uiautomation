# UI Automation COM reference

This document summarizes the COM model and practical interop choices used by UIAutomationMcp.

## Core COM objects

Windows UI Automation is COM-based. The desktop is exposed as a tree of `IUIAutomationElement` instances, and clients reach that tree through `IUIAutomation`.

Important entry points:
- `CUIAutomation`
- `CUIAutomation8`
- `IUIAutomation`
- `IUIAutomationElement`
- `IUIAutomationCondition`
- `IUIAutomationTreeWalker`

The desktop root element is obtained via `IUIAutomation.GetRootElement()`.

## Activation notes

On this machine, direct COM activation by CLSID works reliably:
- `CUIAutomation` -> `{FF48DBA4-60EF-4201-AA87-54103EEF594E}`
- `CUIAutomation8` -> `{E22AD333-B25F-460C-83D0-0581107395C9}`

The UI Automation type library GUID is:
- `{944DE083-8FB8-45CF-BCB7-C477ACB2F897}`

For SDK-style .NET builds, the practical typed path in this repository is the `Interop.UIAutomationClient` package:

```csharp
using Interop.UIAutomationClient;

IUIAutomation automation = new CUIAutomation8Class();
```

## Search model

UI Automation works over an element tree rather than a document model.

Typical starting points:
- `GetRootElement()`
- `ElementFromHandle(hwnd)`
- `ElementFromPoint(x, y)`
- `GetFocusedElement()`

Typical condition helpers:
- `CreatePropertyCondition(...)`
- `CreateAndCondition(...)`
- `CreateOrCondition(...)`
- `CreateTrueCondition()`
- `CreateTreeWalker(...)`

## Views and traversal

UI Automation exposes three common views:
- raw view
- control view
- content view

In this repo, user-facing inspection defaults should stay conservative and understandable. Control view is the safest default for discovery, while raw view is more appropriate for diagnostics.

## Patterns

Element capability is often discovered through control patterns rather than element type alone.

`supportedPatterns` reports every pattern a provider advertises, but advertising a
pattern is not the same as this toolkit being able to use it. The table below is the
authoritative status per pattern:

| Pattern | Status | Surface |
| --- | --- | --- |
| Invoke | actionable | `action invoke` |
| Value | readable + actionable | `valuePattern`, `action set-value` |
| RangeValue | readable + actionable | `rangeValuePattern`, `action set-range-value` |
| Toggle | readable + actionable | `togglePattern`, `action toggle` |
| ExpandCollapse | readable + actionable | `expandCollapsePattern`, `action expand`/`collapse` |
| Window | readable + actionable | `windowPattern`, `action maximize`/`minimize`/`restore`/`close` |
| Scroll | readable + actionable | `scrollPattern`, `action scroll`/`scroll-percent` |
| SelectionItem | readable + actionable | `selectionItemPattern`, `action select`/`add-to-selection`/`remove-from-selection` |
| Transform | readable + actionable | `transformPattern`, `action move`/`resize`/`rotate` |
| MultipleView | readable + actionable | `multipleViewPattern`, `action set-view` |
| Dock | readable + actionable | `dockPattern`, `action dock` |
| Text / Text2 | readable | `text` command: `caret`, `annotations`, `hasTextPattern2` |
| Selection / Selection2 | readable | `selection` command |
| Grid, GridItem, Table, TableItem | readable | `gridPattern`, `gridItemPattern`, `tablePattern`, `tableItemPattern`, `table` command |
| LegacyIAccessible | readable + actionable | `legacyAccessiblePattern`, `nameSource`/`localizedControlTypeSource`, `action default-action`, `set-value` fallback |
| ItemContainer, VirtualizedItem | readable + actionable | `virtualization`, virtualized-item lookup fallback, `action realize` |
| ScrollItem | actionable | `action scroll-into-view` |
| TextChild | readable | `text` command: `textChild` (container + offset for inline elements) |
| TextEdit | readable + observable | `text` command: `textEdit`, `wait-event --event-kind text-edit` |
| Drag | readable | `dragPattern` (`isGrabbed`, `dropEffect`, `grabbedItems`) |
| DropTarget | readable | `dropTargetPattern` (`dropTargetEffect`, `dropTargetEffects`) |
| Annotation, Styles, Spreadsheet, SpreadsheetItem, CustomNavigation, ObjectModel, SynchronizedInput, Transform2 | detect-only | no consumer planned |

### MultipleView

`multipleViewPattern` reports `currentView`, `currentViewName`, and the full
`supportedViews` list as id/name pairs. View names are provider-supplied and
therefore **localized** — on a German Windows the Explorer item view reports
`Details`, `Kacheln`, `Liste` and so on.

`action set-view` accepts either an id or a name. A numeric argument is only
treated as an id when the control actually advertises that id, so controls whose
view *names* are numeric stay addressable by name. Unknown views fail with the
list of available id/name pairs rather than a generic error.

### Dock

`dockPattern` reports `dockPosition` and `dockPositionName`. `action dock` accepts
`top`, `left`, `bottom`, `right`, `fill`, and `none`.

Dock is rare in practice. Most Win32 and WinForms controls are MSAA-bridged and
expose only `LegacyIAccessible`; a real `IDockProvider` generally comes from WPF or
a custom UIA provider.

### Grid and Table

`gridPattern` reports `rowCount`/`columnCount`, `gridItemPattern` reports a cell's
`row`, `column`, `rowSpan`, `columnSpan`, and its containing grid, `tablePattern`
reports `rowOrColumnMajor` plus row and column headers, and `tableItemPattern`
reports the headers a single cell belongs to.

The `table` command (`uia_table` over MCP) reads a whole control as a rectangular
matrix so callers do not have to walk descendants and reconstruct coordinates:

```text
uiamcp table --root --class UIItemsView --max-rows 20 --max-columns 4
```

Limits default to 50 rows and 25 columns and the response reports `returnedRowCount`,
`returnedColumnCount`, and `truncated`, because grids can be arbitrarily large.

Two provider realities shape the payload:

- **Cell text lives in different places.** Explorer's details view puts the *column
  title* in the cell's `name` and the actual content in the Value pattern, while many
  WPF and web grids do the opposite. Each cell therefore exposes `name`, `value`, and
  a resolved `text` that prefers `value` and falls back to `name`.
- **Virtualized rows may not exist yet.** Cells the provider cannot realize are
  returned with `isUnavailable: true` instead of aborting the read, so a partial table
  is still usable. See [ItemContainer and VirtualizedItem](#itemcontainer-and-virtualizeditem)
  for how those rows are reached.

### ItemContainer and VirtualizedItem

A virtualizing container only materializes the items it is currently showing.
Explorer backs a 300-file folder with roughly a dozen live `UIItem` elements, so a
plain tree search for the 250th file fails even though the item plainly exists.

`ItemContainer` is the provider-side escape hatch: `FindItemByProperty` asks the
container itself, which can answer for items that have no UIA element yet.
`VirtualizedItem.Realize()` then turns the returned placeholder into a live element.

**Element lookup now falls back to the container automatically.** When a locator
finds nothing, the resolver walks the search origin and every `ItemContainer`
descendant, asks each one for the item, realizes the result, and verifies the
remaining locator criteria on it. Because this only runs on the path that used to
throw, searches that already succeeded are unchanged; searches that previously
failed may now succeed and take slightly longer. Pass `--no-virtualized` (CLI) or
`realizeVirtualized: false` (MCP) to restore the strict tree-only behaviour — useful
when you are asserting that something is genuinely absent.

`FindItemByProperty` accepts exactly one property, so the fallback picks the most
selective criterion available (automation id → name → class name → control type) and
re-checks the rest on the returned element.

Elements that participate in virtualization report a `virtualization` block:

```json
"virtualization": { "isItemContainer": true, "isVirtualizedItem": false }
```

The block is omitted (`null`) for elements that support neither pattern.
`VirtualizedItem` exposes no readable state at all — `Realize()` is its only member —
so a boolean hint plus `action realize` is the complete surface.

Verified against Explorer: a 400-item folder resolves to 12 live rows, `inspect
--name "Report 0350"` fails with `--no-virtualized` and succeeds without it, and
`action select` drives an off-screen item through the same fallback.

### LegacyIAccessible

Win32 controls, MFC dialogs, and most installers never got a native UI Automation
provider. UIA bridges their MSAA `IAccessible` implementation instead, and without a
consumer for that bridge such windows look near-empty: generic control types, missing
names, no usable actions. They are not unautomatable - the data is just on the other
side of the bridge.

`legacyAccessiblePattern` surfaces it: `childId`, `name`, `value`, `description`,
`role` / `roleName`, `state` / `stateNames`, `help`, `keyboardShortcut`, and
`defaultAction`. `role` is a raw `ROLE_SYSTEM_*` value resolved to a name, and `state`
is a `STATE_SYSTEM_*` bit field decoded into flag names, so a Character Map button
reads as:

```json
"legacyAccessiblePattern": {
  "roleName": "CheckButton",
  "stateNames": ["Focusable"],
  "keyboardShortcut": "Alt+a",
  "defaultAction": "Druecken"
}
```

Note that MSAA roles and UIA control types disagree by design. Character Map's push
buttons report `ROLE_SYSTEM_CHECKBUTTON` while UIA reports control type `Button`; a
WinUI title bar reports `ROLE_SYSTEM_CLIENT` where UIA has no localized control type
at all. Treat `roleName` as provider-reported legacy metadata, not as a second opinion
on the UIA control type.

**Fallbacks are marked, never silent.** When the native UIA name is empty and the
bridge has one, `name` is filled from the bridge and `nameSource` becomes `legacy`
instead of `uia`; `localizedControlType` behaves the same way through
`localizedControlTypeSource`. Consumers can therefore always tell bridged data from
native UIA data.

**Actions.** `action default-action` calls `DoDefaultAction()`, which is the only way
to drive controls that expose no modern actionable pattern. `action set-value` falls
back to the legacy `SetValue()` when the Value pattern is absent. `action invoke` stays
strict, but when Invoke is missing on an MSAA-bridged element the error points at
`default-action` rather than just failing.

Verified against Character Map: `default-action` on the select button reports
`Performed the default action 'Druecken'.` and the character actually lands in the
selection field.

### Text patterns: caret, annotations, TextChild, TextEdit

The base Text pattern answers "what does this control contain". The three companion
patterns answer the questions that actually come up when driving an editor: where is
the caret, what did the app mark up, where does an inline element sit in its document,
and did the app rewrite what was typed.

`text` returns them together:

```json
{
  "hasTextPattern2": true,
  "hasTextEditPattern": true,
  "caret": { "isActive": true, "offset": 30, "lineText": "Second line here.\r" },
  "annotations": [
    { "typeId": 60001, "typeName": "SpellingError", "startOffset": 54, "length": 7, "text": "recieve" }
  ],
  "textChild": null,
  "textEdit": { "activeComposition": "", "conversionTarget": "" }
}
```

**Offsets are computed, not read.** `IUIAutomationTextRange` exposes no offset
property. Every offset here is derived the same way: clone the document range, move
its end to the target range's start with `MoveEndpointByRange`, and take the length of
the resulting text. That is one extra cross-process call per offset, which is why
offsets are only computed where they carry information.

**Annotations are walked, not enumerated.** UI Automation has no "list the
annotations" API, so the document is walked one *format run* at a time
(`ExpandToEnclosingUnit(TextUnit_Format)`, then advance) and each run's
`AnnotationTypes` attribute is read. `AnnotationType_Unknown` (60000) is dropped
because ordinary unannotated runs report it. The walk is capped at 400 runs: each step
is a cross-process COM call and documents are unbounded, so a hard cap is preferable to
an open-ended scan.

**`text` no longer returns null for non-text elements.** A hyperlink or an image is not
a text control, but it lives inside one, and TextChild is the answer to "where". For
such an element `text` returns a payload whose only populated field is `textChild`,
giving the containing document plus the element's range and offset within it — verified
against Edge, where a link 750 characters into a page reports exactly that offset.

**TextEdit is both readable and observable.** `textEdit` reports the active IME
composition and conversion target. The interesting part is the event:
`wait-event --event-kind text-edit` reports auto-correct, IME composition,
composition-finalized and auto-complete changes, with the substituted text in
`eventStrings`. Typing `udn` into Notepad with autocorrect on yields:

```json
{ "textEditChangeTypeName": "TextEditChangeType_AutoCorrect", "eventStrings": ["und"] }
```

UI Automation subscribes text-edit handlers *per change type* and offers no "any"
value, so `wait-event` registers the same handler against all four concrete types.
Passing `TextEditChangeType_None` — the obvious-looking choice — silently subscribes to
nothing and always times out.

### Drag and DropTarget

These two patterns are **observational, and deliberately stay that way.** UI Automation
lets a provider report that a drag is happening and what a drop would do; it offers no
method to *start* one. There is no `IUIAutomationDragPattern.BeginDrag`, and there never
was.

That leaves one alternative — synthesizing mouse input with `SendInput` — and this
project does not do that. Synthetic input steals focus, depends on the pointer actually
being over the right pixels, races with anything else on the desktop, and fails silently
under UIPI when the target runs elevated. Every other verb here drives a provider
directly; a `drag` verb that moved the physical mouse would be a different kind of tool
wearing the same name. If a caller genuinely needs pixel-level dragging, that belongs in
a separate input-injection tool, not behind a UI Automation verb.

What *is* exposed is the read half, which is useful on its own:

```json
"dragPattern": {
  "isGrabbed": false,
  "dropEffect": "",
  "dropEffects": [],
  "grabbedItems": [ { "name": "Explorer", "automationId": "Appid: Microsoft.Windows.Explorer", "...": "..." } ]
},
"dropTargetPattern": { "dropTargetEffect": "", "dropTargetEffects": [] }
```

`dropEffect` and `dropTargetEffect` are free-form provider strings ("move", "copy",
"link", or whatever the app chose) — UI Automation does not constrain them, so do not
switch on them without checking the specific app first. `grabbedItems` matters for
multi-item drags; a provider dragging only itself typically reports just that element.

**Drag progress is observable through ordinary automation events.** There is no separate
event kind: use `wait-event --event-kind automation` with one of

| Event id | Name |
| --- | --- |
| 20026 | `Drag_DragStart` |
| 20027 | `Drag_DragCancel` |
| 20028 | `Drag_DragComplete` |
| 20029 | `DropTarget_DragEnter` |
| 20030 | `DropTarget_DragLeave` |
| 20031 | `DropTarget_Dropped` |

Automation event results now also carry `eventName`, so an observed event identifies
itself instead of arriving as a bare number.

Verified against the Windows 11 taskbar, whose task-list buttons expose the Drag pattern
with populated `grabbedItems`, and against File Explorer's list view, which exposes
DropTarget.


### Condition model and negation

Locator criteria are AND-composed property conditions. Negated criteria wrap a
property condition in `CreateNotCondition`, so exclusions are evaluated by the
provider rather than by fetching everything and filtering client-side.

That distinction is not only about speed, though every excluded element does
cost a cross-process read. It is about correctness under `--max-results`: a cap
consumed entirely by elements the caller meant to skip returns a truncated list
that looks complete.

`--not-name`, `--not-class`, `--not-automation-id` and `--not-control-type`
compose with the positive criteria and with `--scope`. A request carrying only
negative criteria is still a request; it resolves against the search origin
rather than being treated as "no locator given".

`CreateFalseCondition` is deliberately unused - it has no caller that a true
condition plus ordinary filtering does not already serve.

### Absence as a result

`Inspect` throws when nothing matches; `TryInspect` returns null. Both are now
reachable: `inspect --try` (CLI) and `tryInspect: true` (MCP) select the second.

This matters for assertions about things that should be *gone* - a dialog that
has closed, a spinner that has stopped. Without it, proving absence meant
catching a thrown error and inferring intent from its message. It pairs with
`--no-virtualized`, which exists so a caller can assert an item is genuinely
absent rather than merely unmaterialized.
### Transform and ScrollItem

`transformPattern` reports `canMove`, `canResize` and `canRotate`. Advertising
the pattern is not the same as permitting every operation — a fixed-size dialog
exposes Transform and reports `canResize: false` — so `move`, `resize` and
`rotate` check the specific capability first and fail naming it, rather than
letting the call reach COM and return an opaque provider error.

`TransformPattern2` (`Zoom`, `ZoomByUnit`, `CanZoom`, `ZoomLevel`) stays
detect-only. Zoom would be genuinely useful against document and map surfaces,
but no consumer needs it yet and adding it now would be speculative.

`ScrollItem` has exactly one member, `ScrollIntoView()`, and no readable state —
so, as with VirtualizedItem, the verb plus the `supportedPatterns` entry is the
complete surface.

It composes with virtualization: `realize` makes an off-screen item exist,
`scroll-into-view` then makes it visible. The alternative, `scroll-percent`,
requires the caller to compute a percentage from row counts they usually do not
have; `ScrollIntoView()` asks the provider to work it out instead. When the
pattern is absent the error points at `realize`, because a virtualized item that
has not been materialized is the common reason for it to be missing.

### Element relationships and extended interface levels

Two groups of element properties sit outside the flat metadata block.

**Relationships** come from the base `IUIAutomationElement` and point at other
elements: `labeledBy`, `controllerFor`, `describedBy`, `flowsTo`, plus
`flowsFrom` from `IUIAutomationElement2`. They use the flat element *reference*
shape described below rather than full element info, for the same
non-termination reason.

`labeledBy` is the one that changes what is addressable. Win32 and WinForms
inputs frequently carry no name of their own, and the label beside them is a
separate element. Name resolution therefore has three tiers, reported through
`nameSource`:

| `nameSource` | Meaning |
| --- | --- |
| `uia` | the provider supplied a native name |
| `legacy` | the native name was empty; the MSAA bridge supplied one |
| `labeledBy` | both were empty; the labelling element supplied one |

Verified against the live desktop: of 400 scanned elements, 4 reported
`labeledBy` and 16 reported `controllerFor`.

**Extended properties** live on `IUIAutomationElement2` through
`IUIAutomationElement9`. `CreateAutomation()` activates `CUIAutomation8Class`,
so these are reachable, but each level is cast independently and failures
degrade to `null` — the same soft-cast approach `WaitForEvent` already uses for
`IUIAutomation3`. A build that exposes Element4 but not Element9 still yields
everything it has, and `null` means "this OS cannot answer" rather than "the
provider said zero".

| Property | Interface |
| --- | --- |
| `liveSetting` / `liveSettingName`, `optimizeForVisualContent`, `flowsFrom` | Element2 |
| `isPeripheral` | Element3 |
| `positionInSet`, `sizeOfSet`, `level`, `annotationTypes` | Element4 |
| `landmarkType`, `localizedLandmarkType` | Element5 |
| `fullDescription` | Element6 |
| `headingLevel` | Element8 |
| `isDialog` | Element9 |

Two of these need care:

- **`fullDescription` is often where the meaning is.** WinUI, UWP and Edge
  frequently leave `name` terse and put the real accessible description here.
  An element that looks anonymous is worth re-checking against this field.
- **`headingLevel` is normalized.** UIA reports `HeadingLevel_None` as 80050 and
  headings as 80051–80059, so a raw passthrough would stamp a meaningless
  five-digit constant on every element in a tree. It is projected to the
  ordinary 1–9, and `null` for "not a heading".

`annotationTypes` here is *element-level* and is distinct from the format-run
annotation walk performed by the `text` command, which describes runs of text
rather than the element itself.

### Element references in pattern state
Pattern state that points at other elements — a cell's containing grid, a table's
headers — uses a flat element *reference* (name, class, automation id, control type,
runtime id, bounds) rather than full element info. This is deliberate: a column
header is its own column header, so reading full element info would recurse forever,
and header lists would otherwise dominate the payload.

Future service-layer operations should consider both element identity and supported patterns.

## Threading guidance

UI Automation is COM and must be treated carefully.

Practical rules:
- keep client creation inside one interop layer
- avoid scattering COM activation through the codebase
- keep STA-sensitive work inside audited helpers
- isolate retry or wait logic when it is introduced

## Lifetime guidance

Even though UIA is query-heavy, it still uses COM proxies.

Guidelines:
- release temporary COM proxies created in hot loops
- prefer DTOs at the public boundary
- keep raw COM interfaces inside the interop layer where possible

## Current repository focus

The current repository surface centers on:
1. typed client bootstrap
2. desktop and focused-element inspection
3. handle-based lookup
4. search by name, class name, and automation id
5. CLI and MCP exposure for those queries

## References
- Microsoft Learn: UI Automation overview
- Microsoft Learn: `IUIAutomation`
- Microsoft Learn: `CUIAutomation`
- NuGet: `Interop.UIAutomationClient`

