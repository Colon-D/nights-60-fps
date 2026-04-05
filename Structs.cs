using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.X86;
using System.Runtime.InteropServices;

namespace nights.test.sixtyfps.structs;

// stolen from my own: https://github.com/Colon-D/nights-a-life-editor/blob/main/Structs.cs

// there is a lot of unused structs here from other mods I have made

[StructLayout(LayoutKind.Sequential, Size = 0xC)]
public struct Vec3 {
	public float X;
	public float Y;
	public float Z;
}


[StructLayout(LayoutKind.Sequential, Size = 0x40)]
public struct Mat4x4 {
	public float M00;
	public float M01;
	public float M02;
	public float M03;
	public float M10;
	public float M11;
	public float M12;
	public float M13;
	public float M20;
	public float M21;
	public float M22;
	public float M23;
	public float M30;
	public float M31;
	public float M32;
	public float M33;
}

[StructLayout(LayoutKind.Sequential)]
public struct Rot3 {
	public short X { get; set; }
	public short Y { get; set; }
	public short Z { get; set; }
}

[StructLayout(LayoutKind.Explicit)]
public unsafe struct Character {
	[FieldOffset(0x88)]
	public Animation* Animation;

	[FieldOffset(0x94)]
	public Vec3 Pos;
}

[StructLayout(LayoutKind.Explicit)]
public unsafe struct PlayerSub { // this should inherit Character?
	[StructLayout(LayoutKind.Explicit)]
	private unsafe struct PlayerSubVFTable {
		[FieldOffset(0x0)]
		public long Dtor;
		[FieldOffset(0x8)]
		public long Update;
		[FieldOffset(0x58)]
		public long InitMaybe;
	}

	[FieldOffset(0x0)]
	private PlayerSubVFTable* _vftable;

	[Function(CallingConventions.MicrosoftThiscall)]
	private unsafe delegate void DtorT(PlayerSub* self, char a2);
	[Function(CallingConventions.MicrosoftThiscall)]
	private unsafe delegate int UpdateT(PlayerSub* self);
	[Function(CallingConventions.MicrosoftThiscall)]
	private unsafe delegate int InitMaybeT(PlayerSub* self, int a2);

	public void Dtor() {
		Console.WriteLine("Dtor: 0x" + ((int)_vftable->Dtor).ToString("X"));
		var fn = Globals.Hooks.CreateWrapper<DtorT>(_vftable->Dtor, out _);
		fixed (PlayerSub* self = &this) {
			fn(self, (char)1);
		}
	}

	public int Update() {
		var fn = Globals.Hooks.CreateWrapper<UpdateT>(_vftable->Update, out _);
		fixed (PlayerSub* self = &this) {
			return fn(self);
		}
	}

	public int InitMaybe(int a2) {
		var fn = Globals.Hooks.CreateWrapper<InitMaybeT>(_vftable->InitMaybe, out _);
		fixed (PlayerSub* self = &this) {
			return fn(self, a2);
		}
	}

	[FieldOffset(0x84)]
	public int Unknown84;

	[FieldOffset(0x94)]
	public Vec3 Pos;
	[FieldOffset(0xA8)]
	public Vec3 RenderPos;

	[FieldOffset(0xBC)]
	public Vec3 Velocity;

	[FieldOffset(0xC8)]
	public short Pitch;

	[FieldOffset(0xCA)]
	public short Yaw;

	[FieldOffset(0xCC)]
	public short Roll;

	[FieldOffset(0xEC)]
	public Player* Player;

	[FieldOffset(0xF0)]
	public PlayerSubType Type;

	[FieldOffset(0xF4)]
	public int State;

	[FieldOffset(0x112)]
	public short SpinSpeed;

	[FieldOffset(0x114)]
	public float Speed;

	[FieldOffset(0x178)]
	public Vec3 BubbleSomething; // not velocity, idk what it actually is

	[FieldOffset(0x88)]
	public Animation* Animation;
}

[StructLayout(LayoutKind.Explicit)]
public unsafe struct Player {
	[FieldOffset(0x58)]
	public byte Unknown58;

	[FieldOffset(0x59)]
	public byte Unknown59;

	[FieldOffset(0x5A)]
	public byte WantsMovement;

	[FieldOffset(0x5C)]
	public short AngleTarget;

	[FieldOffset(0x60)]
	public PlayerSub* PlayerSub;

	// world's ugliest code, in C++/Rust this would be simple and good looking
	[FieldOffset(0x64)]
	private PlayerSub* _playerSubsBegin;
	public PlayerSub* GetPlayerSub(PlayerSubType type) {
		fixed (PlayerSub** _playerSubs = &_playerSubsBegin) {
			return _playerSubs[(int)type];
		}
	}

	[FieldOffset(0x80)]
	public int Dream;

	[FieldOffset(0x84)]
	public int Unknown84;

	[FieldOffset(0x88)]
	public int Unknown88;

	[FieldOffset(0x8C)]
	public int Unknown8C;

	[FieldOffset(0x90)]
	public int Unknown90;
}

public enum PlayerSubType {
	Nights,
	Elliot,
	Claris,
	ElliotTwinSeeds,
	ClarisTwinSeeds,
	OtherNightsWizemanFight
}

[StructLayout(LayoutKind.Explicit)]
public unsafe struct GameInput {
	[StructLayout(LayoutKind.Explicit)]
	public struct Action {
		[FieldOffset(0x0)]
		public int State;
		[FieldOffset(0x4)]
		public int Down;
		[FieldOffset(0x8)]
		public sbyte AnalogCopy;
		[FieldOffset(0x9)]
		public sbyte Analog;
		[FieldOffset(0xC)]
		public int FramesDown;
	};
	[FieldOffset(0x4)]
	private Action _actionsBegin;
	public Action* GetAction(int action) {
		fixed (Action* actions = &_actionsBegin) {
			return &actions[action];
		}
		//fixed (Action* actions = &_actionsBegin) {
		//	return (Action*)((Byte*)actions + 0x20 * action);
		//}
	}

	[FieldOffset(0x3D4)]
	public sbyte select;
	[FieldOffset(0x3D7)]
	public sbyte start;
	[FieldOffset(0x3D8)]
	public sbyte dpad_up;
	[FieldOffset(0x3D9)]
	public sbyte dpad_right;
	[FieldOffset(0x3DA)]
	public sbyte dpad_down;
	[FieldOffset(0x3DB)]
	public sbyte dpad_left;
	[FieldOffset(0x3DC)]
	public sbyte left_trigger;
	[FieldOffset(0x3DD)]
	public sbyte right_trigger;
	[FieldOffset(0x3DE)]
	public sbyte left_bumper;
	[FieldOffset(0x3DF)]
	public sbyte right_bumper;
	[FieldOffset(0x3E0)]
	public sbyte y;
	[FieldOffset(0x3E1)]
	public sbyte a;
	[FieldOffset(0x3E2)]
	public sbyte b;
	[FieldOffset(0x3E3)]
	public sbyte x;
	[FieldOffset(0x3E4)]
	public sbyte right_x;
	[FieldOffset(0x3E5)]
	public sbyte right_y;
	[FieldOffset(0x3E6)]
	public sbyte left_x;
	[FieldOffset(0x3E7)]
	public sbyte left_y;
}

[StructLayout(LayoutKind.Explicit)]
public unsafe struct WorldManager {
	[FieldOffset(0x24)]
	public CameraBoyGirl** Camera;
	[FieldOffset(0x34)]
	public EnemyManager* EnemyManager;
	[FieldOffset(0x50)]
	public Player* Player;
}

public unsafe struct Globals {
	public static unsafe WorldManager** WorldManager = (WorldManager**)0x24C4EC4;
	public static unsafe GameStateManager** GameStateManager = (GameStateManager**)0x24C4E94;
	public static unsafe GameInput** GameInput = (GameInput**)0x24C4E88;
	public static unsafe uint** FramesElapsed = (uint**)0x24C4E88;
	public static unsafe IntPtr* HWND = (IntPtr*)0x24C44B0;
	public static unsafe SaveFile* SaveFile = (SaveFile*)(0x24C4E94 + 0x38C);
	public static unsafe GameModeManager** GameModeManager = (GameModeManager**)(0x24C4E90);

	public static IReloadedHooks Hooks;

	public static float LerpAlpha = 0f;

	public static float lerp(float v0, float v1, float t) {
		return v0 + t * (v1 - v0);
	}

	public static Vec3 lerp(Vec3 v0, Vec3 v1, float t) {
		return new Vec3 {
			X = lerp(v0.X, v1.X, t),
			Y = lerp(v0.Y, v1.Y, t),
			Z = lerp(v0.Z, v1.Z, t)
		};
	}
}

[StructLayout(LayoutKind.Explicit)]
public unsafe struct Animation {
	[FieldOffset(0x18)]
	public byte Frozen;
	[FieldOffset(0x20)]
	public Motion* Motion;
	[FieldOffset(0x2C)]
	public Motion* MotionCopy;
	[FieldOffset(0x30)]
	public Vec3 Pos;
	[FieldOffset(0x3C)]
	public Rot3 Rot;
	[FieldOffset(0x58)]
	public Vec3 Scale;
}

[StructLayout(LayoutKind.Explicit)]
public unsafe struct Motion {
	[FieldOffset(0x14)]
	public int Animation;
	[FieldOffset(0x18)]
	public int Frame;
	[FieldOffset(0x116C)]
	public JointArray* JointArray;
	[FieldOffset(0x1290)]
	public int FrameAlt;
	[FieldOffset(0x1294)]
	public int Unknown1294;
	[FieldOffset(0x1298)]
	public int Unknown1298;
	[FieldOffset(0x12A8)]
	public int ThisNeedsToBe2OrAnimationsAreBrokenIDKWhy;
	[FieldOffset(0x12AC)]
	public int SpeedSometimes;
}

[StructLayout(LayoutKind.Explicit)]
public unsafe struct JointArray {
	[StructLayout(LayoutKind.Explicit)]
	private unsafe struct VFTable {
		[FieldOffset(0x0)]
		public long Dtor;
	}

	[FieldOffset(0x0)]
	private VFTable* _vftable;

	[Function(CallingConventions.MicrosoftThiscall)]
	private unsafe delegate void DtorT(JointArray* self, int a1);

	public void Dtor(int a1) {
		var fn = Globals.Hooks.CreateWrapper<DtorT>(_vftable->Dtor, out _);
		fixed (JointArray* self = &this) {
			fn(self, a1);
		}
	}
}

[StructLayout(LayoutKind.Explicit)]
public unsafe struct GameStateManager {
	[StructLayout(LayoutKind.Explicit)]
	public unsafe struct GameStateSpecial {
		[FieldOffset(0x4)]
		public byte TopDownOrSledOrRear; // depending on dream
		[FieldOffset(0x5)]
		public byte RearOrInMuseum;
	}

	[FieldOffset(0x3B8C)]
	public GameStateSpecial* Special;

	[FieldOffset(0xB50)]
	public SaveData SaveData;

	[FieldOffset(0x5D8)]
	public ALifeStageData StageDataCopy;

	[FieldOffset(0x44)]
	public Dream dream;
}

[StructLayout(LayoutKind.Explicit)]
public unsafe struct GameModeManager {
	[FieldOffset(0x8)]
	public int gameMode;
}

public enum Dream {
	SpringValley = 0x1,
	Gillwing = 0x2,
	SplashGarden = 0x3,
	Puffy = 0x4,
	MysticForest = 0x5,
	Gulpo = 0x6,
	FrozenBell = 0x7,
	Clawz = 0x8,
	SoftMuseum = 0x9,
	Jackle = 0xA,
	StickCanyon = 0xB,
	Reala = 0xC,
	TwinSeedsClaris = 0xD,
	WizemanClaris = 0xE,
	TwinSeedsElliot = 0xF,
	WizemanElliot = 0x10,
	SpringValleyXmasClaris = 0x11,
	GillwingXmasClaris = 0x12,
	SpringValleyXmasElliot = 0x13,
	GillwingXmasElliot = 0x14,
};

[StructLayout(LayoutKind.Explicit, Size = 0x3018)]
public unsafe struct SaveData {
	[FieldOffset(0xA3C)]
	private ALifeStageData _aLifeStageDataBegin;
	public ALifeStageData* GetALifeStageData(ALifeStage stage) {
		fixed (ALifeStageData* aLifeStageData = &_aLifeStageDataBegin) {
			return &aLifeStageData[(int)stage];
		}
	}
}

[StructLayout(LayoutKind.Explicit, Size = 0xF0B0)]
public unsafe struct SaveFile {

	// then is the main save data slot
	[FieldOffset(0x0)]
	public SaveData saveData;

	// next is four unused save data slots

	// and some footer data?
}

[StructLayout(LayoutKind.Explicit)]
public unsafe struct CameraBoyGirl {
	[StructLayout(LayoutKind.Explicit)]
	private unsafe struct CameraCommonVFTable {
		[FieldOffset(0x0)]
		public UInt32 Dtor;
		[FieldOffset(0x1C)]
		public UInt32 Update;
		[FieldOffset(0x24)]
		public UInt32 Unknown;
	}

	[FieldOffset(0x0)]
	private CameraCommonVFTable* _vftable;

	[Function(CallingConventions.MicrosoftThiscall)]
	private unsafe delegate void* UpdateT(CameraBoyGirl* self, int a2);

	[Function(CallingConventions.MicrosoftThiscall)]
	private unsafe delegate int UnknownT(CameraBoyGirl* self, Vec3* a2, Vec3* a3);

	public void Update(int a2) {
		var fn = Globals.Hooks.CreateWrapper<UpdateT>(_vftable->Update, out _);
		fixed (CameraBoyGirl* self = &this) {
			fn(self, a2);
		}
	}

	public void CommonUpdate(int a2) {
		var fn = Globals.Hooks.CreateWrapper<UpdateT>(0x428810, out _);
		fixed (CameraBoyGirl* self = &this) {
			fn(self, a2);
		}
	}

	public void PlayerUpdate(int a2) {
		var fn = Globals.Hooks.CreateWrapper<UpdateT>(0x429720, out _);
		fixed (CameraBoyGirl* self = &this) {
			fn(self, a2);
		}
	}

	public int Unknown(Vec3* a2, Vec3* a3) {
		var fn = Globals.Hooks.CreateWrapper<UnknownT>(_vftable->Unknown, out _);
		fixed (CameraBoyGirl* self = &this) {
			return fn(self, a2, a3);
		}
	}

	[FieldOffset(0x10)]
	public Mat4x4 matrix;

	[FieldOffset(0x90)]
	public Vec3 pos;

	[FieldOffset(0x9C)]
	public Vec3 look_at;

	// Causing Crashing?!
	//[FieldOffset(0xA8)]
	//public Rot3 rot;
	//[FieldOffset(0xBC)]
	//public Rot3 rot2;

	//[FieldOffset(0xAC)]
	//public float dist;

	//[FieldOffset(0xC8)]
	//public Player* player;

	//[FieldOffset(0xB0)]
	//public Vec3 pos2; // maybe?
};

public enum ALifeStage {
	SpringValley,
	SplashGarden,
	MysticForest,
	FrozenBell,
	SoftMuseum,
	StickCanyon,
	SpringValleyChristmas,
}

// A-LIFE stage data stored in save file
[StructLayout(LayoutKind.Explicit, Size = 0x54C)]
public unsafe struct ALifeStageData {
	[FieldOffset(0x0)]
	public byte nightopianCountCollection;
	[FieldOffset(0x1)]
	public byte mepianCountCollection;
	[FieldOffset(0x2)]
	public byte aLifeCount; // in level, including "hidden"/"decoration"
							// NiGHTS Nice Night Tours refers to it as "ピ アン タワー" (Pian Tower), so I will use that instead of Castle
	[FieldOffset(0x3)]
	public byte pianTowerProgress; // [0, 8] => [nothing, fully built]
	[FieldOffset(0x8)]
	public float happiness; // mean happiness of all nightopians [0, 3]
	[FieldOffset(0xC)]
	private ALifeData _aLifeDataBegin;
	public ALifeData* GetALifeData(int index) {
		fixed (ALifeData* aLifeData = &_aLifeDataBegin) {
			return &aLifeData[index];
		}
	}
	// then like 0x40 bytes of footer data? that is just blank? go off I guess.
}

// A-LIFE data stored in save file
[StructLayout(LayoutKind.Explicit, Size = 0x20)]
public unsafe struct ALifeData {
	[FieldOffset(0x1)]
	private byte _unknownAndHappiness;
	// [0, 3] => [sad, happy]
	public byte Happiness {
		get => (byte)(_unknownAndHappiness & 0x0F);
		set => _unknownAndHappiness = (byte)((_unknownAndHappiness & 0xF0) | value);
	}
	[FieldOffset(0x2)]
	private byte _speedAndReaction;
	// [0, 15] => [stationary, fast]
	public byte Speed {
		get => (byte)((_speedAndReaction & 0xF0) >> 4);
		set => _speedAndReaction = (byte)((value << 4) | (_speedAndReaction & 0x0F));
	}
	// [0, 15] => [unreactive, reactive]
	// (maybe reaction distance if I'm interpreting a discord post correctly?)
	public byte Reaction {
		get => (byte)(_speedAndReaction & 0x0F);
		set => _speedAndReaction = (byte)((_speedAndReaction & 0xF0) | value);
	}
	// counts down during gameplay. death on 0. nightopians lower during mating
	[FieldOffset(0x4)]
	public byte remaining_life;
	// unknown where changes, normally 30/0x1E
	[FieldOffset(0x5)]
	public byte offspring_initial_remaining_life;
	// action performed in collection and maybe gameplay (but different?) [0, 23]
	[FieldOffset(0x6)]
	public byte special_behaviour;
	// always different action? [0, 23]
	// patent states that a mepian might take on two actions at once, like
	// activity 1 being fishing and activity 2 being whistling, so they will
	// whistle whilst fishing, so perhaps this is that?
	[FieldOffset(0x7)]
	public byte special_behaviour_2;

	[FieldOffset(0x8)]
	private byte _flagsAndHeight;
	// behaviour, regardless of parts
	public bool Mepian {
		get => (_flagsAndHeight & 0b1000_0000) != 0;
		set => _flagsAndHeight = (byte)(value ? _flagsAndHeight | 0b1000_0000 : _flagsAndHeight & 0b0111_1111);
	}
	// Inverted bit | incompatible with mepian flag
	public bool King {
		get => (_flagsAndHeight & 0b0100_0000) == 0;
		set => _flagsAndHeight = (byte)(value ? _flagsAndHeight & 0b1011_1111 : _flagsAndHeight | 0b0100_0000);
	}
	// whether their AI is active, otherwise they are egg/decoration
	public bool Active {
		get => (_flagsAndHeight & 0b0010_0000) != 0;
		set => _flagsAndHeight = (byte)(value ? _flagsAndHeight | 0b0010_0000 : _flagsAndHeight & 0b1101_1111);
	}
	// if egg then AI must be inactive
	public bool Egg {
		get => (_flagsAndHeight & 0b0001_0000) != 0;
		set => _flagsAndHeight = (byte)(value ? _flagsAndHeight | 0b0001_0000 : _flagsAndHeight & 0b1110_1111);
	}
	// if not egg and not active
	public bool Decorative {
		get => !Egg && !Active;
	}
	// [0, 15] => [low, high]
	public byte Height {
		get => (byte)(_flagsAndHeight & 0x0F);
		set => _flagsAndHeight = (byte)((_flagsAndHeight & 0xF0) | value);
	}

	// ?? first nibble is ??, second is maybe [0, 3]?
	[FieldOffset(0x9)]
	public byte position;

	// visual only? [0, 3] => [nightopian, ???, ???, ???] (level specific)
	[FieldOffset(0xA)]
	public byte body;
	[FieldOffset(0xB)]
	public byte head;
	[FieldOffset(0xC)]
	public byte arms;
	[FieldOffset(0xD)]
	public byte legs;

	// generally less than 10 apparently?
	[FieldOffset(0xE)]
	public byte position_2;

	// [0, 0xFF], if >= 0x43 then they will attempt to mate, increases during
	// gameplay, decreases for:
	//   nightopians when mating with a nightopian or mepian
	//   mepians when mating with a mepian
	// called "estrous cycle" in patent
	[FieldOffset(0xF)]
	public byte desireToMate;

	// todo: finish
}

[StructLayout(LayoutKind.Explicit)]
public unsafe struct EnemyManager {
	[FieldOffset(0x4)]
	public EnemyList* list;
	[FieldOffset(0x8)]
	public int size;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EnemyList {
	public EnemyNode* begin;
	public EnemyNode* end;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EnemyNode {
	public EnemyNode* next; // goes back to List if last node
	public EnemyNode* prev; // goes back to List if first node
	public Enemy* enemy;
}

// inherits from Renderable3D, which probably has a Position variable, but I
// haven't reverse engineered Renderable3D's layout yet, but I at least know
// Character's position - which is probably a violation of DRY. I should use
// inheritance here, but C# won't let me for some reason?
[StructLayout(LayoutKind.Explicit)]
public unsafe struct Enemy {
	[FieldOffset(0x0)]
	public int vftable;
	[FieldOffset(0x94)]
	public Vec3 pos;
};