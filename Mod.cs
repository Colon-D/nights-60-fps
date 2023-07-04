using Reloaded.Mod.Interfaces;
using nights.test.sixtyfps.Template;
using nights.test.sixtyfps.Configuration;
using Reloaded.Hooks.Definitions;
using CallingConventions = Reloaded.Hooks.Definitions.X86.CallingConventions;
using Reloaded.Hooks.Definitions.X86;
using System.Diagnostics;
using Reloaded.Memory;
using Reloaded.Memory.Interfaces;

namespace nights.test.sixtyfps;

/// <summary>
/// Your mod logic goes here.
/// </summary>
public class Mod : ModBase // <= Do not Remove.
{
	/// <summary>
	/// Provides access to the mod loader API.
	/// </summary>
	private readonly IModLoader _modLoader;

	/// <summary>
	/// Provides access to the Reloaded.Hooks API.
	/// </summary>
	/// <remarks>This is null if you remove dependency on Reloaded.SharedLib.Hooks in your mod.</remarks>
	private readonly IReloadedHooks _hooks;

	/// <summary>
	/// Provides access to the Reloaded logger.
	/// </summary>
	private readonly ILogger _logger;

	/// <summary>
	/// Entry point into the mod, instance that created this class.
	/// </summary>
	private readonly IMod _owner;

	/// <summary>
	/// Provides access to this mod's configuration.
	/// </summary>
	private Config _configuration;

	/// <summary>
	/// The configuration of the currently executing mod.
	/// </summary>
	private readonly IModConfig _modConfig;

	public Mod(ModContext context) {
		_modLoader = context.ModLoader;
		_hooks = context.Hooks;
		_logger = context.Logger;
		_owner = context.Owner;
		_configuration = context.Configuration;
		_modConfig = context.ModConfig;

		unsafe {
			// hook a new function to limit the game's framerate
			LimitFramerateHook = _hooks.CreateHook<LimitFramerate>(
				LimitFramerateImpl, 0x40B0F0
			).Activate();

			// NOP the call to update the game for the 2nd time in one frame
			const byte NOP = 0x90;
			Memory.Instance.SafeWrite(
				0x40A559, new[] { NOP, NOP, NOP, NOP, NOP }
			);

			// write jz to replace the jnz

			// this makes the soft museum update on the first loop, rather
			// than the NOPed out second loop.
			// this also allows the SS Dreams ground to render.
			// it might do other things too, but I haven't noticed anything.
			Span<byte> SHORT_JZ = new byte[] { 0x74 };
			Memory.Instance.SafeWrite(
				0x569DDC, SHORT_JZ
			);
			
			Span<byte> NEAR_JZ = new byte[] { 0x0F, 0x84 };
			// this makes the cheats enterable on the title screen
			Memory.Instance.SafeWrite(
				0x509E67, NEAR_JZ
			);
		}
	}

	// The game's built-in framerate limiter is not very accurate.
	[Function(CallingConventions.Cdecl)]
	public unsafe delegate void LimitFramerate();
	public IHook<LimitFramerate> LimitFramerateHook;
	public Stopwatch LimitFramerateStopwatch = new Stopwatch();
	public unsafe void LimitFramerateImpl() {
		var TargetFrameMilliseconds = 1000.0 / _configuration.TargetFramerate;
		var elapsedMilliseconds =
			LimitFramerateStopwatch.Elapsed.TotalMilliseconds;
		var remainingMilliseconds =
			TargetFrameMilliseconds - elapsedMilliseconds;
		if (remainingMilliseconds >= 1.0) {
			Thread.Sleep((int)remainingMilliseconds);
		}
		LimitFramerateStopwatch.Restart();
	}

	#region Standard Overrides
	public override void ConfigurationUpdated(Config configuration)
	{
		// Apply settings from configuration.
		// ... your code here.
		_configuration = configuration;
		_logger.WriteLine($"[{_modConfig.ModId}] Config Updated: Applying");
	}
	#endregion

	#region For Exports, Serialization etc.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
	public Mod() { }
#pragma warning restore CS8618
	#endregion
}
