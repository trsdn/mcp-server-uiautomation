using UIAutomationMcp.ComInterop;
using UIAutomationMcp.Service;

namespace UIAutomationMcp.Tests;

/// <summary>
/// Covers locator composition, including the negated criteria that are evaluated
/// by the provider rather than by filtering results afterwards.
/// </summary>
[Collection(DesktopSampleGroup.Name)]
public sealed class LocatorTests
{
    private readonly UiAutomationService service = new();

    [DesktopFact]
    public void NotControlType_ExcludesThatControlType()
    {
        const int Button = 50000;

        var withButtons = service.FindAll(new UiAutomationSearchRequest
        {
            DesktopRoot = true,
            Scope = "subtree",
            MaxResults = 150
        });

        var withoutButtons = service.FindAll(new UiAutomationSearchRequest
        {
            DesktopRoot = true,
            Scope = "subtree",
            MaxResults = 150,
            NotControlType = Button
        });

        // Only meaningful if the unfiltered search actually saw buttons.
        if (withButtons.Any(e => e.ControlType == Button))
        {
            Assert.DoesNotContain(withoutButtons, e => e.ControlType == Button);
        }
    }

    [DesktopFact]
    public void NotName_ExcludesThatName()
    {
        var all = service.FindAll(new UiAutomationSearchRequest
        {
            DesktopRoot = true,
            Scope = "subtree",
            MaxResults = 100
        });

        var named = all.FirstOrDefault(e => !string.IsNullOrWhiteSpace(e.Name));
        if (named is null)
        {
            return;
        }

        var filtered = service.FindAll(new UiAutomationSearchRequest
        {
            DesktopRoot = true,
            Scope = "subtree",
            MaxResults = 100,
            NotName = named.Name
        });

        Assert.DoesNotContain(filtered, e => e.Name == named.Name);
    }

    [DesktopFact]
    public void NegativeCriteriaComposeWithPositiveOnes()
    {
        const int Button = 50000;

        var buttons = service.FindAll(new UiAutomationSearchRequest
        {
            DesktopRoot = true,
            Scope = "subtree",
            ControlType = Button,
            MaxResults = 60
        });

        var named = buttons.FirstOrDefault(e => !string.IsNullOrWhiteSpace(e.Name));
        if (named is null)
        {
            return;
        }

        var filtered = service.FindAll(new UiAutomationSearchRequest
        {
            DesktopRoot = true,
            Scope = "subtree",
            ControlType = Button,
            NotName = named.Name,
            MaxResults = 60
        });

        Assert.All(filtered, e => Assert.Equal(Button, e.ControlType));
        Assert.DoesNotContain(filtered, e => e.Name == named.Name);
    }

    [DesktopFact]
    public void ANegativeOnlyRequestIsStillAValidLocator()
    {
        // A request carrying only negative criteria must not be mistaken for
        // "no locator given" and silently resolved to the search origin.
        var results = service.FindAll(new UiAutomationSearchRequest
        {
            DesktopRoot = true,
            Scope = "subtree",
            NotClassName = "NoSuchClass_UIAutomationMcpTests",
            MaxResults = 25
        });

        Assert.NotEmpty(results);
    }

    [DesktopTheory]
    [InlineData("element")]
    [InlineData("children")]
    [InlineData("descendants")]
    [InlineData("subtree")]
    public void EveryDocumentedScopeIsAccepted(string scope)
    {
        var results = service.FindAll(new UiAutomationSearchRequest
        {
            DesktopRoot = true,
            Scope = scope,
            MaxResults = 5
        });

        Assert.NotNull(results);
    }

    [DesktopFact]
    public void ChildrenScopeReturnsNoMoreThanSubtreeScope()
    {
        var children = service.FindAll(new UiAutomationSearchRequest
        {
            DesktopRoot = true,
            Scope = "children",
            MaxResults = 200
        });

        var subtree = service.FindAll(new UiAutomationSearchRequest
        {
            DesktopRoot = true,
            Scope = "subtree",
            MaxResults = 200
        });

        Assert.True(children.Count <= subtree.Count);
    }

    [DesktopFact]
    public void CacheRequestDoesNotChangeTheResolvedIdentity()
    {
        var plain = service.Inspect(new UiAutomationLocateRequest { DesktopRoot = true });
        var cached = service.Inspect(new UiAutomationLocateRequest
        {
            DesktopRoot = true,
            CacheRequest = new UiAutomationCacheRequestInfo { UseCache = true, Scope = "element", View = "control" }
        });

        Assert.Equal(plain.ClassName, cached.ClassName);
        Assert.Equal(plain.RuntimeId, cached.RuntimeId);
    }
}
