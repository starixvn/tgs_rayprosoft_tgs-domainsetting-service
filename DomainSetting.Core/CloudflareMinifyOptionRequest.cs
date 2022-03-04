namespace Tgs.DomainSettings;

internal class CloudflareMinifyOptionRequest
{
    public string Css { get; set; } = "on";

    public string Html { get; set; } = "on";

    public string Js { get; set; } = "on";
}
