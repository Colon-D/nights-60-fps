using nights.test.sixtyfps.Configuration;
using nights.test.sixtyfps.structs;
using nights.test.sixtyfps.Template;
using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.X86;
using Reloaded.Memory;
using Reloaded.Memory.Interfaces;
using Reloaded.Mod.Interfaces;
using System.Diagnostics;
using System.Runtime.InteropServices;
using static Reloaded.Hooks.Definitions.X86.FunctionAttribute;
using CallingConventions = Reloaded.Hooks.Definitions.X86.CallingConventions;

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

	public struct InterpolatedPosition {
		public UInt32 LastUpdated = 0;
		public Vec3 PrevPos = new Vec3 { X = 0f, Y = 0f, Z = 0f };
		public Vec3 NextPos = new Vec3 { X = 0f, Y = 0f, Z = 0f };
		public InterpolatedPosition() {
		}
	}

	public Mod(ModContext context) {
		_modLoader = context.ModLoader;
		_hooks = context.Hooks;
		_logger = context.Logger;
		_owner = context.Owner;
		_configuration = context.Configuration;
		_modConfig = context.ModConfig;

		Globals.Hooks = _hooks;

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

			// 60+ FPS

			if (_configuration.RenderFramerate > _configuration.UpdateFramerate) {
				// update the game's logic LESS often than you render it
				render_delta = 1f / _configuration.RenderFramerate;
				update_delta = 1f / _configuration.UpdateFramerate;
				UpdateGameHook = _hooks.CreateHook<UpdateGame>(
					UpdateGameImpl, 0x40B000
				).Activate();

				// set position to interpolate between
				CharacterSetPositionHook = _hooks.CreateHook<CharacterSetPosition>(
					CharacterSetPositionImpl, 0x485D20
				).Activate();

				// interpolate between positions
				UseAnimationHook = _hooks.CreateHook<UseAnimation>(
					UseAnimationImpl, 0x47D5A0
				).Activate();
				sub_40D1D0Hook = _hooks.CreateHook<sub_40D1D0>(
					sub_40D1D0Impl, 0x40D1D0
				).Activate();

				// clear map so no memory leak
				CameraBase_dtorHook = _hooks.CreateHook<CameraBase_dtor>(
					CameraBase_dtorImpl, 0x42C2A0
				).Activate();
			}
		}
	}

	// The game's built-in framerate limiter is not very accurate.
	[Function(CallingConventions.Cdecl)]
	public unsafe delegate void LimitFramerate();
	public IHook<LimitFramerate> LimitFramerateHook;
	public Stopwatch LimitFramerateStopwatch = new Stopwatch();
	public unsafe void LimitFramerateImpl() {
		var TargetFrameMilliseconds = 1000.0 / _configuration.RenderFramerate;
		var elapsedMilliseconds =
			LimitFramerateStopwatch.Elapsed.TotalMilliseconds;
		var remainingMilliseconds =
			TargetFrameMilliseconds - elapsedMilliseconds;
		if (remainingMilliseconds >= 1.0) {
			Thread.Sleep((int)remainingMilliseconds);
		}
		LimitFramerateStopwatch.Restart();
	}

	// 60+ FPS
	// update the game's logic LESS often than you render it
	[Function(new[]{ Register.ecx, Register.edi }, Register.eax, StackCleanup.Caller)]
	public unsafe delegate int UpdateGame(int a1, int a2);
	public IHook<UpdateGame> UpdateGameHook;
	public static UInt32 logic_frame = 0;
	public static bool wait_one_frame_because_otherwise_the_csharp_is_prone_to_crashing_for_no_fucking_reason = true;
	public static float update_timer = 0f; // will increase by render_delta every "render" until it is >= update_delta
	public static float update_delta = 1f / 60f; // will be set to (1 / LogicFramerate)
	public static float render_delta = 1f / 60f; // will be set to (1 / RenderFramerate)
	public unsafe int UpdateGameImpl(int a1, int a2) {
		update_timer = update_timer + render_delta;
		bool update_logic = update_timer >= update_delta;
		if (update_logic) {
			update_timer = update_timer % update_delta;
		}

		const float extrapolation = 1f;
		Globals.LerpAlpha = extrapolation + update_timer / update_delta;

		var camera = (*Globals.WorldManager)->Camera;

		int return_this = 0;
		if (update_logic) {
			// LOGIC UPDATE BEYOND THIS POINT
			++logic_frame;
			return_this = UpdateGameHook.OriginalFunction(a1, a2); // ORIG UPDATE
			if (camera != null) {
					if ((*camera) != null) {
						if (wait_one_frame_because_otherwise_the_csharp_is_prone_to_crashing_for_no_fucking_reason) {
						wait_one_frame_because_otherwise_the_csharp_is_prone_to_crashing_for_no_fucking_reason = false;
					}
					else {
							CameraPrevPos = CameraNextPos;
							CameraPrevLookAt = CameraNextLookAt;
							var camera_dereferenced = (*camera);
							CameraNextPos = (*camera)->pos; // how is this crashing?!
							CameraNextLookAt = (*camera)->look_at;
					}
				}
			}
		}

		if (camera != null) {
			var lerpedPos = Globals.lerp(CameraPrevPos, CameraNextPos, Globals.LerpAlpha);
			var lerpedLookAt = Globals.lerp(CameraPrevLookAt, CameraNextLookAt, Globals.LerpAlpha);

			var CamUp = new Vec3 {
				X = 0f,
				Y = 1f,
				Z = 0f
			};
			var LookAt = new Vec3 {
				X = lerpedPos.X + lerpedLookAt.X,
				Y = lerpedPos.Y + lerpedLookAt.Y,
				Z = lerpedPos.Z + lerpedLookAt.Z
			};
			var IDK = new Vec3 {
				X = lerpedPos.X,
				Y = lerpedPos.Y,
				Z = lerpedPos.Z
			};

			var unknown = (UInt32*)0x24C4FCC;
			if (camera != null) {
				var unknown2 = (*unknown) + 0x10;
				if (unknown2 != 0) {
					Mat4x4* mat = *((Mat4x4**)unknown2);
					if (mat != null) {
						D3DXMatrixLookAtRH(mat, &lerpedPos, &LookAt, &CamUp);
					}
				}
			}
		}

		return return_this;
	}

	[DllImport("d3dx9_41.dll")]
	public unsafe static extern void* D3DXMatrixLookAtRH(void* pOut, Vec3* pEye, Vec3* pAt, Vec3* pUp);

	// map of Animation's to Positions
	public unsafe static Dictionary<UInt32, InterpolatedPosition> animation_positions = new Dictionary<UInt32, InterpolatedPosition>();

	[Function(CallingConventions.MicrosoftThiscall)]
	public unsafe delegate Animation* CharacterSetPosition(Character* character, Vec3* to);
	public IHook<CharacterSetPosition> CharacterSetPositionHook;
	public unsafe Animation* CharacterSetPositionImpl(Character* character, Vec3* to) {
		character->Pos = *to;
		var anim = character->Animation;
		if (anim != null) {
			anim->Pos = *to;

			if (!animation_positions.ContainsKey((UInt32)anim)) {
				animation_positions.Add((UInt32)anim, new InterpolatedPosition()); // MEMORY LEAK! Todo: clear after each Dream?
			}
			var interpolated_pos = animation_positions[(UInt32)anim];
			// don't "teleport" by accidently updating prev position
			//   if pos set more than once per frame
			if (interpolated_pos.LastUpdated != logic_frame) {
				interpolated_pos.LastUpdated = logic_frame;
				interpolated_pos.PrevPos = interpolated_pos.NextPos;
			}
			interpolated_pos.NextPos = anim->Pos;
			animation_positions[(UInt32)anim] = interpolated_pos;
		}
		return anim;
	}

	[Function(Register.eax, Register.eax, StackCleanup.Caller)]
	public unsafe delegate int UseAnimation(Animation* anim);
	public IHook<UseAnimation> UseAnimationHook;
	public static Vec3 LerpedPos;
	public static bool LerpNextPos = false;
	public unsafe int UseAnimationImpl(Animation* anim) {
		if (animation_positions.ContainsKey((UInt32)anim)) {
			var interpolated_pos = animation_positions[(UInt32)anim];
			if (interpolated_pos.LastUpdated == logic_frame) {
				LerpNextPos = true;
				LerpedPos = Globals.lerp(interpolated_pos.PrevPos, interpolated_pos.NextPos, Globals.LerpAlpha);
			}
		}

		return UseAnimationHook.OriginalFunction(anim);
	}

	public static Vec3 CameraPrevPos;
	public static Vec3 CameraNextPos;
	public static Vec3 CameraPrevLookAt;
	public static Vec3 CameraNextLookAt;

	[Function(CallingConventions.Stdcall)]
	public unsafe delegate int sub_40D1D0(int a1, Vec3* pos);
	public static IHook<sub_40D1D0> sub_40D1D0Hook;
	public static unsafe int sub_40D1D0Impl(int a1, Vec3* pos) {
		if (LerpNextPos) {
			LerpNextPos = false;
			*pos = LerpedPos;
		}
		return sub_40D1D0Hook.OriginalFunction(a1, pos);
	}

	// going to use camera dtor to reset dictionary and variable,
	// there might be better places but idk, whatever works tbh
	[Function(CallingConventions.MicrosoftThiscall)]
	public unsafe delegate int* CameraBase_dtor(int* self, char a2);
	public static IHook<CameraBase_dtor> CameraBase_dtorHook;
	public static unsafe int* CameraBase_dtorImpl(int* self, char a2) {
		wait_one_frame_because_otherwise_the_csharp_is_prone_to_crashing_for_no_fucking_reason = true;
		animation_positions.Clear();
		return CameraBase_dtorHook.OriginalFunction(self, a2);
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
