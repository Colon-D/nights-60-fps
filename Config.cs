using System.ComponentModel;
using nights.test.sixtyfps.Template.Configuration;

namespace nights.test.sixtyfps.Configuration;

public class Config : Configurable<Config>
{
	/*
        User Properties:
            - Please put all of your configurable properties here.
    
        By default, configuration saves as "Config.json" in mod user config folder.    
        Need more config files/classes? See Configuration.cs
    
        Available Attributes:
        - Category
        - DisplayName
        - Description
        - DefaultValue

        // Technically Supported but not Useful
        - Browsable
        - Localizable

        The `DefaultValue` attribute is used as part of the `Reset` button in Reloaded-Launcher.
    */

	[Category("Experimental")]
	[DisplayName("Render Framerate")]
    [Description("Framerate that the game will try to render itself to.\n" +
        "Values greater than Update Framerate will use linear interpolation between update frames.\n" +
        "Values lower than Update Framerate won't do anything will limit the Update Framerate." +
        "This is a new experimental feature, and not everything will interpolate.")]
    [DefaultValue(60)]
    public int RenderFramerate { get; set; } = 60;
	[Category("Debugging")]
	[DisplayName("Update Framerate")]
	[Description("Framerate that the game will try to update itself to.\n" +
		"This WILL make the game run either too fast or too slow.\n" +
		"This should be kept at 60 FPS unless you want the game to run at the wrong speed.\n" +
		"The game's logic was already in 60 FPS, which is why this mod exists.")]
	[DefaultValue(60)]
    public int UpdateFramerate { get; set; } = 60;
}

/// <summary>
/// Allows you to override certain aspects of the configuration creation process (e.g. create multiple configurations).
/// Override elements in <see cref="ConfiguratorMixinBase"/> for finer control.
/// </summary>
public class ConfiguratorMixin : ConfiguratorMixinBase
{
    // 
}
