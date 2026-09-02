using UIAutomationMcp.ComInterop;
using UIAutomationMcp.Service;

namespace UIAutomationMcp.Tests;

/// <summary>
/// Covers text reads and the offset-addressed range operations.
/// </summary>
/// <remarks>
/// These tests use whatever text providers the running desktop happens to expose
/// rather than launching an application, so they assert invariants that must hold
/// for any provider instead of exact strings. Launching an app would make the
/// suite dependent on a specific Windows build's Notepad.
/// </remarks>
[Collection(DesktopSampleGroup.Name)]
public sealed class TextTests(DesktopSampleFixture desktop)
{
    private readonly UiAutomationService service = new();

    /// <summary>Finds any element exposing the Text pattern, or null.</summary>
    /// <summary>The shared sample's Text provider, if the desktop has one.</summary>
    private UiAutomationElementInfo? FindTextProvider() => desktop.TextProvider;

    private static UiAutomationLocateRequest LocatorFor(UiAutomationElementInfo element) => new()
    {
        DesktopRoot = true,
        Scope = "subtree",
        ClassName = element.ClassName,
        ControlType = element.ControlType
    };

    [DesktopFact]
    public void ReadText_ReturnsNullForAnElementThatIsNeitherTextNorInsideText()
    {
        // The desktop root is not a text provider and is not an inline child of
        // one, so there is nothing for TextChild to report either.
        var text = service.ReadText(new UiAutomationLocateRequest { DesktopRoot = true });

        Assert.Null(text);
    }

    [DesktopFact]
    public void ReadText_WithoutASearchLeavesFindUnset()
    {
        var provider = FindTextProvider();
        if (provider is null)
        {
            return;
        }

        var text = service.ReadText(LocatorFor(provider));
        if (text is null)
        {
            return;
        }

        // Null Find means "no search was requested", which is deliberately
        // distinct from a search that found nothing.
        Assert.Null(text.Find);
    }

    [DesktopFact]
    public void ReadText_ReportsAMissAsFoundFalseRatherThanNull()
    {
        var provider = FindTextProvider();
        if (provider is null)
        {
            return;
        }

        var text = service.ReadText(LocatorFor(provider), "NoSuchText_UIAutomationMcpTests_ZZZ");
        if (text is null)
        {
            return;
        }

        Assert.NotNull(text.Find);
        Assert.False(text.Find!.Found);
        Assert.Null(text.Find.StartOffset);
        Assert.Equal("NoSuchText_UIAutomationMcpTests_ZZZ", text.Find.Needle);
    }

    [DesktopFact]
    public void ReadText_FindingItsOwnContentReportsAConsistentOffsetAndLength()
    {
        var provider = FindTextProvider();
        if (provider is null)
        {
            return;
        }

        var locator = LocatorFor(provider);
        var text = service.ReadText(locator);
        if (text is null || string.IsNullOrWhiteSpace(text.Text) || text.Text.Length < 4)
        {
            return;
        }

        // Search for a run taken from the document itself, so a miss would mean
        // the search is broken rather than that the text is absent.
        var needle = text.Text.Trim().Split('\r', '\n').FirstOrDefault(s => s.Length >= 3);
        if (string.IsNullOrEmpty(needle))
        {
            return;
        }

        var found = service.ReadText(locator, needle)?.Find;
        if (found is null || !found.Found)
        {
            // Supporting the Text pattern does not oblige a provider to implement
            // FindText; that path is covered by the miss test, which asserts the
            // failure is reported rather than thrown.
            return;
        }

        Assert.NotNull(found.StartOffset);
        Assert.True(found.StartOffset >= 0, "A match must report a non-negative offset.");
        Assert.True(found.Length > 0, "A match must report a positive length.");
    }

    [DesktopFact]
    public void SelectText_RequiresARangeToActOn()
    {
        var provider = FindTextProvider();
        if (provider is null)
        {
            return;
        }

        // Neither a search string nor an offset was supplied, so the verb must
        // say which input was missing rather than failing at the COM boundary.
        var error = Assert.ThrowsAny<Exception>(() => service.PerformAction(new UiAutomationActionRequest
        {
            Action = "select-text",
            Locator = LocatorFor(provider)
        }));

        Assert.Contains("text range", error.Message, StringComparison.OrdinalIgnoreCase);
    }

    [DesktopFact]
    public void SelectText_ReportsTextThatIsNotPresent()
    {
        var provider = FindTextProvider();
        if (provider is null)
        {
            return;
        }

        var error = Assert.ThrowsAny<Exception>(() => service.PerformAction(new UiAutomationActionRequest
        {
            Action = "select-text",
            Locator = LocatorFor(provider),
            StringValue = "NoSuchText_UIAutomationMcpTests_ZZZ"
        }));

        // Either the text genuinely was not found, or the provider does not
        // implement FindText at all. Both must be reported as an actionable
        // message rather than as a raw "Specified method is not supported".
        Assert.True(
            error.Message.Contains("not found", StringComparison.OrdinalIgnoreCase)
            || error.Message.Contains("not text search", StringComparison.OrdinalIgnoreCase),
            $"Unhelpful failure message: {error.Message}");
    }

    [DesktopFact]
    public void TextVerbsRejectAnElementWithNoTextPattern()
    {
        foreach (var action in new[] { "select-text", "move-caret", "scroll-text-into-view" })
        {
            var error = Assert.ThrowsAny<Exception>(() => service.PerformAction(new UiAutomationActionRequest
            {
                Action = action,
                Locator = new UiAutomationLocateRequest { DesktopRoot = true },
                IntValue = 0
            }));

            Assert.Contains("Text pattern", error.Message, StringComparison.OrdinalIgnoreCase);
        }
    }

    [DesktopTheory]
    [InlineData(100000, 5)]      // offset far beyond the document
    [InlineData(2, 100000)]      // length far beyond the document
    [InlineData(0, 0)]           // degenerate range
    [InlineData(-5, 3)]          // negative offset, clamped
    [InlineData(3, -1)]          // negative length, clamped
    public void SelectText_ClampsOutOfRangeOffsetsOrExplainsItself(int startOffset, int length)
    {
        // Offsets arrive from a caller who cannot see the document, so out-of-range
        // values are ordinary input rather than programmer error. Verified against a
        // live provider: an offset past the end yields an empty selection, a length
        // past it stops at the document end, and a negative value is treated as zero.
        //
        // The one legitimate failure is a provider that exposes text but not range
        // manipulation. That must be explained, not surfaced as the unactionable
        // "Specified method is not supported" that this test originally caught.
        var provider = FindTextProvider();
        if (provider is null)
        {
            return;
        }

        var exception = Record.Exception(() => service.PerformAction(new UiAutomationActionRequest
        {
            Action = "select-text",
            Locator = LocatorFor(provider),
            IntValue = startOffset,
            NumberValue = length
        }));

        if (exception is not null)
        {
            Assert.Contains("does not support manipulating text ranges", exception.Message, StringComparison.OrdinalIgnoreCase);
        }
    }

    [DesktopFact]
    public void MoveCaret_AcceptsAnOffsetBeyondTheDocumentOrExplainsItself()
    {
        var provider = FindTextProvider();
        if (provider is null)
        {
            return;
        }

        var exception = Record.Exception(() => service.PerformAction(new UiAutomationActionRequest
        {
            Action = "move-caret",
            Locator = LocatorFor(provider),
            IntValue = 100000
        }));

        if (exception is not null)
        {
            Assert.Contains("does not support manipulating text ranges", exception.Message, StringComparison.OrdinalIgnoreCase);
        }
    }

    [DesktopFact]
    public void CaseSensitiveSearchIsNarrowerThanTheDefault()
    {
        // The default is case-insensitive because a caller is usually locating text
        // read off a screen. --match-case must genuinely narrow the search rather
        // than being accepted and ignored, so this asserts that a search for text
        // whose case has been inverted stops matching.
        var provider = FindTextProvider();
        if (provider is null)
        {
            return;
        }

        var locator = LocatorFor(provider);
        var document = service.ReadText(locator);
        if (document is null || string.IsNullOrWhiteSpace(document.Text))
        {
            return;
        }

        // Take a run with at least one letter, then invert its case.
        var needle = document.Text
            .Split(' ', '\r', '\n')
            .FirstOrDefault(w => w.Length >= 4 && w.Any(char.IsLetter) && w.Any(char.IsUpper) != w.Any(char.IsLower));
        if (string.IsNullOrEmpty(needle))
        {
            return;
        }

        var inverted = new string(needle.Select(c => char.IsUpper(c) ? char.ToLowerInvariant(c) : char.ToUpperInvariant(c)).ToArray());
        if (string.Equals(inverted, needle, StringComparison.Ordinal))
        {
            return;
        }

        var insensitive = service.ReadText(locator, inverted)?.Find;
        var sensitive = service.ReadText(locator, inverted, matchCase: true)?.Find;
        if (insensitive is null || sensitive is null || !insensitive.Found)
        {
            // The provider does not implement FindText; covered elsewhere.
            return;
        }

        Assert.False(
            sensitive.Found,
            $"Case-sensitive search for \"{inverted}\" should miss where the insensitive search hit \"{insensitive.Text}\".");
    }

    [DesktopFact]
    public void BackwardSearchIsAcceptedAndDoesNotPrecedeTheForwardMatch()
    {
        var provider = FindTextProvider();
        if (provider is null)
        {
            return;
        }

        var locator = LocatorFor(provider);
        var document = service.ReadText(locator);
        var needle = document?.Text?.Split(' ', '\r', '\n').FirstOrDefault(w => w.Length >= 3);
        if (string.IsNullOrEmpty(needle))
        {
            return;
        }

        var forward = service.ReadText(locator, needle)?.Find;
        var backward = service.ReadText(locator, needle, searchBackward: true)?.Find;
        if (forward?.Found != true || backward?.Found != true)
        {
            return;
        }

        // Backward finds the last occurrence, so it can never sit before the first.
        Assert.True(
            backward.StartOffset >= forward.StartOffset,
            $"Backward match at {backward.StartOffset} precedes the forward match at {forward.StartOffset}.");
    }

    [DesktopFact]
    public void ACaseSensitiveMissSaysThatItWasCaseSensitive()
    {
        var provider = FindTextProvider();
        if (provider is null)
        {
            return;
        }

        var exception = Record.Exception(() => service.PerformAction(new UiAutomationActionRequest
        {
            Action = "select-text",
            Locator = LocatorFor(provider),
            StringValue = "nOsUcHtExT_UIAutomationMcpTests",
            MatchCase = true
        }));

        if (exception is not null && exception.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
        {
            // A caller who forgot they asked for case sensitivity should be able to
            // tell that from the message alone.
            Assert.Contains("case-sensitive", exception.Message, StringComparison.OrdinalIgnoreCase);
        }
    }

    [DesktopFact]
    public void TextRangeFailuresNeverLeakTheRawComMessage()
    {
        // "Specified method is not supported" tells a caller nothing about what to
        // do next. Every text-range verb must translate it.
        var provider = FindTextProvider();
        if (provider is null)
        {
            return;
        }

        foreach (var action in new[] { "select-text", "move-caret", "scroll-text-into-view" })
        {
            var exception = Record.Exception(() => service.PerformAction(new UiAutomationActionRequest
            {
                Action = action,
                Locator = LocatorFor(provider),
                IntValue = 0,
                NumberValue = 1
            }));

            if (exception is not null)
            {
                Assert.DoesNotContain("Specified method is not supported", exception.Message, StringComparison.OrdinalIgnoreCase);
            }
        }
    }
}
