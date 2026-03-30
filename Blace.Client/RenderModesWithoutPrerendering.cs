using Microsoft.AspNetCore.Components.Web;

namespace Blace.Client;

public static class RenderModesWithoutPrerendering
{
    public static InteractiveServerRenderMode InteractiveServerWithoutPrerendering { get; } = new(false);
    public static InteractiveWebAssemblyRenderMode InteractiveWebAssemblyWithoutPrerendering { get; } = new(false);
}
