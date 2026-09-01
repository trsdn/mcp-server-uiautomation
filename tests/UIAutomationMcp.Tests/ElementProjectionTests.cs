using UIAutomationMcp.ComInterop;

namespace UIAutomationMcp.Tests;

/// <summary>
/// Covers the element metadata projection, including the relationship properties
/// and the IUIAutomationElement2..9 reads.
/// </summary>
/// <remarks>
/// Reads from the shared desktop sample rather than walking the tree per test.
/// See <see cref="DesktopSampleFixture"/> for why.
/// </remarks>
[Collection(DesktopSampleGroup.Name)]
public sealed class ElementProjectionTests(DesktopSampleFixture desktop)
{
    private IReadOnlyList<UiAutomationElementInfo> Sample => desktop.Elements;

    [DesktopFact]
    public void CoreMetadataIsPopulated()
    {
        Assert.NotNull(desktop.Root);
        var root = desktop.Root!;

        Assert.False(string.IsNullOrEmpty(root.ClassName));
        Assert.True(root.ControlType > 0);
        Assert.True(root.ProcessId > 0);
        Assert.NotEmpty(root.RuntimeId);
        Assert.NotEmpty(root.SupportedPatterns);
    }

    [DesktopFact]
    public void SupportedPatternsAreNamedRatherThanNumbered()
    {
        // Every id in UIA_PatternIds is mapped, so a bare "Pattern:12345" would
        // mean the map has fallen behind the interop assembly. Id 0 is filtered
        // upstream because providers occasionally report it and it is not a pattern.
        var unnamed = Sample
            .SelectMany(e => e.SupportedPatterns)
            .Where(p => p.ProgrammaticName.StartsWith("Pattern:", StringComparison.Ordinal))
            .Select(p => p.ProgrammaticName)
            .Distinct()
            .ToList();

        Assert.True(unnamed.Count == 0, $"Unmapped pattern ids: {string.Join(", ", unnamed)}");
    }

    [DesktopFact]
    public void PatternIdsAreAlwaysPositive()
    {
        Assert.All(Sample.SelectMany(e => e.SupportedPatterns), p => Assert.True(p.Id > 0));
    }

    [DesktopFact]
    public void RelationshipPropertiesAreProjectedRatherThanOmitted()
    {
        Assert.NotNull(desktop.Root);
        var root = desktop.Root!;

        // Collections are never null, so a caller can enumerate without a guard.
        Assert.NotNull(root.ControllerFor);
        Assert.NotNull(root.DescribedBy);
        Assert.NotNull(root.FlowsTo);
        Assert.NotNull(root.FlowsFrom);
    }

    [DesktopFact]
    public void RelationshipReferencesAreFlatAndDoNotRecurse()
    {
        // A referenced element is projected as a reference rather than full element
        // info. Full info would not terminate: a column header is its own header.
        var reference = Sample
            .Select(e => e.LabeledBy ?? (e.ControllerFor.Count > 0 ? e.ControllerFor[0] : null))
            .FirstOrDefault(r => r is not null);

        if (reference is null)
        {
            return; // No provider in this sample exposes one; nothing to assert.
        }

        Assert.NotNull(reference.RuntimeId);
        Assert.NotNull(reference.Name);
        Assert.NotNull(reference.ClassName);
    }

    [DesktopFact]
    public void NameSourceIsAlwaysOneOfTheThreeKnownTiers()
    {
        Assert.All(
            Sample.Select(e => e.NameSource).Distinct(),
            s => Assert.Contains(s, new[] { "uia", "legacy", "labeledBy" }));
    }

    [DesktopFact]
    public void NameSourceReportsLabeledByOnlyWhenTheNameCameFromALabel()
    {
        foreach (var element in Sample.Where(e => e.NameSource == "labeledBy"))
        {
            Assert.NotNull(element.LabeledBy);
            Assert.Equal(element.LabeledBy!.Name, element.Name);
        }
    }

    [DesktopFact]
    public void HeadingLevelIsNormalizedRatherThanRaw()
    {
        // UIA reports HeadingLevel_None as 80050 and headings as 80051..80059.
        // Passing that through would stamp a meaningless five-digit constant on
        // every element in the tree, so it is projected to 1..9 or null.
        foreach (var element in Sample)
        {
            if (element.HeadingLevel is { } level)
            {
                Assert.InRange(level, 1, 9);
            }
        }
    }

    [DesktopFact]
    public void ExtendedInterfacePropertiesAreReadableOnAModernWindows()
    {
        // Each interface level is cast independently, so this also proves the
        // Element2/4/5/6/8/9 casts all succeed here rather than silently degrading.
        Assert.NotNull(desktop.Root);
        var root = desktop.Root!;

        Assert.NotNull(root.FullDescription);
        Assert.NotNull(root.PositionInSet);
        Assert.NotNull(root.SizeOfSet);
        Assert.NotNull(root.Level);
        Assert.NotNull(root.LandmarkType);
        Assert.NotNull(root.IsDialog);
        Assert.NotNull(root.IsPeripheral);
        Assert.NotNull(root.LiveSetting);
    }

    [DesktopFact]
    public void PositionInSetAndSizeOfSetAgreeWhereBothArePresent()
    {
        foreach (var element in Sample.Where(e => e.PositionInSet > 0 && e.SizeOfSet > 0))
        {
            Assert.True(
                element.PositionInSet <= element.SizeOfSet,
                $"'{element.Name}' reports position {element.PositionInSet} of {element.SizeOfSet}.");
        }
    }

    [DesktopFact]
    public void TransformPatternStateAppearsWhenThePatternIsSupported()
    {
        foreach (var element in Sample.Where(e =>
                     e.SupportedPatterns.Any(p => p.ProgrammaticName == "Transform")))
        {
            Assert.NotNull(element.TransformPattern);
        }
    }

    [DesktopFact]
    public void VirtualizationBlockIsOmittedWhenNeitherPatternIsSupported()
    {
        foreach (var element in Sample)
        {
            var names = element.SupportedPatterns.Select(p => p.ProgrammaticName).ToList();
            if (!names.Contains("ItemContainer") && !names.Contains("VirtualizedItem"))
            {
                Assert.Null(element.Virtualization);
            }
        }
    }

    [DesktopFact]
    public void BoundingRectangleIsAlwaysProjected()
    {
        // Asserts the projection, not the provider. Whether a given window reports
        // a sensible rectangle is its own business; what this repository controls
        // is that the value is always present, so a caller can read it without a
        // guard even when the element is mid-transition or already gone.
        Assert.All(Sample, e => Assert.NotNull(e.BoundingRectangle));
    }

    [DesktopFact]
    public void OnscreenElementsMostlyReportUsableRectangles()
    {
        // Providers occasionally report a degenerate or inverted rectangle for an
        // element being created or destroyed, so this asserts the shape of the
        // population rather than every element - the latter is a guarantee no UI
        // Automation client can make.
        var onscreen = Sample.Where(e => !e.IsOffscreen).ToList();
        if (onscreen.Count == 0)
        {
            return;
        }

        var sane = onscreen.Count(e =>
            e.BoundingRectangle.Right >= e.BoundingRectangle.Left
            && e.BoundingRectangle.Bottom >= e.BoundingRectangle.Top);

        Assert.True(
            sane > onscreen.Count / 2,
            $"Only {sane} of {onscreen.Count} on-screen elements reported a non-inverted rectangle.");
    }
}
