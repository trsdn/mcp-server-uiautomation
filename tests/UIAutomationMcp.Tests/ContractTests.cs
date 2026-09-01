using System.Text.Json;
using UIAutomationMcp.ComInterop;

namespace UIAutomationMcp.Tests;

/// <summary>
/// Covers the DTO contract and the JSON shape the CLI emits. These need no
/// desktop: they are about the boundary the toolkit publishes, which is what
/// downstream callers actually depend on.
/// </summary>
public sealed class ContractTests
{
    // Matches the CLI, so a change in casing or null handling shows up here.
    private static readonly JsonSerializerOptions Options =
        new(JsonSerializerDefaults.Web) { WriteIndented = false };

    [Fact]
    public void CollectionPropertiesDefaultToEmptyRatherThanNull()
    {
        // A caller enumerating these should never need a null guard.
        var element = new UiAutomationElementInfo();

        Assert.NotNull(element.RuntimeId);
        Assert.NotNull(element.SupportedPatterns);
        Assert.NotNull(element.ControllerFor);
        Assert.NotNull(element.DescribedBy);
        Assert.NotNull(element.FlowsTo);
        Assert.NotNull(element.FlowsFrom);
    }

    [Fact]
    public void OptionalExtendedPropertiesDefaultToNull()
    {
        // Null means "this OS or provider could not answer", which must stay
        // distinguishable from a real zero or false.
        var element = new UiAutomationElementInfo();

        Assert.Null(element.FullDescription);
        Assert.Null(element.HeadingLevel);
        Assert.Null(element.IsDialog);
        Assert.Null(element.PositionInSet);
        Assert.Null(element.LiveSetting);
        Assert.Null(element.TransformPattern);
    }

    [Fact]
    public void NameSourceDefaultsToUia()
    {
        Assert.Equal("uia", new UiAutomationElementInfo().NameSource);
        Assert.Equal("uia", new UiAutomationElementInfo().LocalizedControlTypeSource);
    }

    [Fact]
    public void ElementInfoSerializesWithCamelCaseNames()
    {
        var json = JsonSerializer.Serialize(
            new UiAutomationElementInfo { Name = "Save", ClassName = "Button" },
            Options);

        Assert.Contains("\"name\":\"Save\"", json, StringComparison.Ordinal);
        Assert.Contains("\"className\":\"Button\"", json, StringComparison.Ordinal);
        Assert.Contains("\"nameSource\":\"uia\"", json, StringComparison.Ordinal);
    }

    [Fact]
    public void NewPropertiesAppearInSerializedOutput()
    {
        // These are the fields added for the element-relationship and extended
        // interface work; a rename would silently break downstream consumers.
        var json = JsonSerializer.Serialize(new UiAutomationElementInfo(), Options);

        foreach (var expected in new[]
                 {
                     "labeledBy", "controllerFor", "describedBy", "flowsTo", "flowsFrom",
                     "fullDescription", "positionInSet", "sizeOfSet", "level",
                     "landmarkType", "headingLevel", "isDialog", "isPeripheral", "liveSetting"
                 })
        {
            Assert.Contains($"\"{expected}\":", json, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void EventResultSerializesEveryEventKindPayload()
    {
        var json = JsonSerializer.Serialize(new UiAutomationEventResult(), Options);

        foreach (var expected in new[]
                 {
                     "eventKind", "timedOut",
                     "notificationKind", "displayString", "activityId",
                     "changeId", "changeCount",
                     "textRangeText", "textRangeOffset"
                 })
        {
            Assert.Contains($"\"{expected}\":", json, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void TextFindResultDistinguishesAMissFromAnAbsentSearch()
    {
        var miss = new UiAutomationTextFindResult { Found = false, Needle = "x" };

        Assert.False(miss.Found);
        Assert.Null(miss.StartOffset);
        Assert.NotNull(miss.BoundingRectangles);
        Assert.Empty(miss.BoundingRectangles);
    }

    [Fact]
    public void LocateRequestDefaultsToVirtualizedFallbackEnabled()
    {
        // The ItemContainer fallback only runs on the path that used to throw, so
        // it is on by default; --no-virtualized is the opt-out for absence checks.
        Assert.True(new UiAutomationLocateRequest().RealizeVirtualized);
        Assert.Equal("subtree", new UiAutomationLocateRequest().Scope);
    }

    [Fact]
    public void SearchRequestDefaultsAreConservative()
    {
        var request = new UiAutomationSearchRequest();

        Assert.Equal(50, request.MaxResults);
        Assert.Equal("subtree", request.Scope);
        Assert.Null(request.NotName);
        Assert.Null(request.NotControlType);
    }

    [Fact]
    public void EventWaitRequestDefaultsToAFiveSecondFocusWait()
    {
        var request = new UiAutomationEventWaitRequest();

        Assert.Equal("focus", request.EventKind);
        Assert.Equal(5000, request.TimeoutMs);
        Assert.Null(request.ChangeId);
    }
}
