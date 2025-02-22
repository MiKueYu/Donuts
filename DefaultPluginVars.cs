using Donuts.Models;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Donuts;

internal static class DefaultPluginVars
{
	// Main Settings
	internal static Setting<bool> PluginEnabled;
	internal static Setting<bool> DespawnEnabledPMC;
	internal static Setting<bool> DespawnEnabledSCAV;
	internal static Setting<bool> HardCapEnabled;
	internal static Setting<float> coolDownTimer;
	internal static Setting<string> pmcGroupChance;
	internal static Setting<string> scavGroupChance;
	internal static Setting<string> botDifficultiesPMC;
	internal static Setting<string> botDifficultiesSCAV;
	internal static Setting<string> botDifficultiesOther;
	internal static Setting<bool> ShowRandomFolderChoice;
	internal static Setting<string> pmcFaction;
	internal static Setting<string> forceAllBotType;

	internal static Setting<bool> useTimeBasedHardStop;
	internal static Setting<bool> hardStopOptionPMC;
	internal static Setting<int> hardStopTimePMC;
	internal static Setting<int> hardStopPercentPMC;
	internal static Setting<bool> hardStopOptionSCAV;
	internal static Setting<int> hardStopTimeSCAV;
	internal static Setting<int> hardStopPercentSCAV;

	internal static Setting<bool> hotspotBoostPMC;
	internal static Setting<bool> hotspotBoostSCAV;
	internal static Setting<bool> hotspotIgnoreHardCapPMC;
	internal static Setting<bool> hotspotIgnoreHardCapSCAV;
	internal static Setting<int> pmcFactionRatio;
	internal static Setting<float> battleStateCoolDown;
	internal static Setting<int> maxRespawnsPMC;
	internal static Setting<int> maxRespawnsSCAV;

	// Global Minimum Spawn Distance From Player
	internal static Setting<bool> globalMinSpawnDistanceFromPlayerBool;
	internal static Setting<float> globalMinSpawnDistanceFromPlayerFactory;
	internal static Setting<float> globalMinSpawnDistanceFromPlayerCustoms;
	internal static Setting<float> globalMinSpawnDistanceFromPlayerReserve;
	internal static Setting<float> globalMinSpawnDistanceFromPlayerStreets;
	internal static Setting<float> globalMinSpawnDistanceFromPlayerWoods;
	internal static Setting<float> globalMinSpawnDistanceFromPlayerLaboratory;
	internal static Setting<float> globalMinSpawnDistanceFromPlayerShoreline;
	internal static Setting<float> globalMinSpawnDistanceFromPlayerGroundZero;
	internal static Setting<float> globalMinSpawnDistanceFromPlayerInterchange;
	internal static Setting<float> globalMinSpawnDistanceFromPlayerLighthouse;

	// Global Minimum Spawn Distance From Other Bots
	internal static Setting<bool> globalMinSpawnDistanceFromOtherBotsBool;
	internal static Setting<float> globalMinSpawnDistanceFromOtherBotsFactory;
	internal static Setting<float> globalMinSpawnDistanceFromOtherBotsCustoms;
	internal static Setting<float> globalMinSpawnDistanceFromOtherBotsReserve;
	internal static Setting<float> globalMinSpawnDistanceFromOtherBotsStreets;
	internal static Setting<float> globalMinSpawnDistanceFromOtherBotsWoods;
	internal static Setting<float> globalMinSpawnDistanceFromOtherBotsLaboratory;
	internal static Setting<float> globalMinSpawnDistanceFromOtherBotsShoreline;
	internal static Setting<float> globalMinSpawnDistanceFromOtherBotsGroundZero;
	internal static Setting<float> globalMinSpawnDistanceFromOtherBotsInterchange;
	internal static Setting<float> globalMinSpawnDistanceFromOtherBotsLighthouse;

	// Advanced Settings
	internal static Setting<float> replenishInterval;
	internal static Setting<int> maxSpawnTriesPerBot;
	internal static Setting<float> despawnInterval;
	internal static Setting<string> groupWeightDistroLow;
	internal static Setting<string> groupWeightDistroDefault;
	internal static Setting<string> groupWeightDistroHigh;
	internal static Setting<float> maxRaidDelay;

	// Debugging
	internal static Setting<bool> debugLogging;
	internal static Setting<bool> DebugGizmos;
	internal static Setting<bool> gizmoRealSize;

	// Spawn Point Maker
	internal static Setting<string> spawnName;
	internal static Setting<int> groupNum;
	internal static Setting<string> wildSpawns;
	internal static Setting<float> minSpawnDist;
	internal static Setting<float> maxSpawnDist;
	internal static Setting<float> botTriggerDistance;
	internal static Setting<float> botTimerTrigger;
	internal static Setting<int> maxRandNumBots;
	internal static Setting<int> spawnChance;
	internal static Setting<int> maxSpawnsBeforeCooldown;
	internal static Setting<bool> ignoreTimerFirstSpawn;
	internal static Setting<float> minSpawnDistanceFromPlayer;
	internal static Setting<KeyCode> CreateSpawnMarkerKey;
	internal static Setting<KeyCode> DeleteSpawnMarkerKey;

	//private static readonly string[] _wildSpawnTypes = Enum.GetNames(typeof(WildSpawnType));

	private static readonly string[] _wildSpawnTypes =
	[
		"arenafighterevent",
		"assault",
		"assaultgroup",
		"bossboar",
		"bossboarsniper",
		"bossbully",
		"bossgluhar",
		"bosskilla",
		"bossknight",
		"bosskojaniy",
		"bosssanitar",
		"bosstagilla",
		"bosszryachiy",
		"crazyassaultevent",
		"cursedassault",
		"exusec-rogues",
		"raiders",
		"followerbigpipe",
		"followerbirdeye",
		"followerboar",
		"followerbully",
		"followergluharassault",
		"followergluharscout",
		"followergluharsecurity",
		"followergluharsnipe",
		"followerkojaniy",
		"followersanitar",
		"followertagilla",
		"followerzryachiy",
		"gifter",
		"marksman",
		"pmc",
		"pmcBEAR",
		"pmcUSEC",
		"sectantpriest",
		"sectantwarrior",
	];

	// Save Settings
	internal static Setting<bool> saveNewFileOnly;
	internal static Setting<KeyCode> WriteToFileKey;

	private static readonly Dictionary<string, int[]> _groupChanceWeights = new()
	{
		{ "Low", [400, 90, 9, 0, 0] },
		{ "Default", [210, 210, 45, 25, 10] },
		{ "High", [0, 75, 175, 175, 75] }
	};
	private static readonly string _defaultWeightsString = ConvertIntArrayToString(_groupChanceWeights["Default"]);
	private static readonly string _lowWeightsString = ConvertIntArrayToString(_groupChanceWeights["Low"]);
	private static readonly string _highWeightsString = ConvertIntArrayToString(_groupChanceWeights["High"]);

	private static readonly string[] _pmcGroupChanceList = ["None", "Default", "Low", "High", "Max", "Random"];
	private static readonly string[] _scavGroupChanceList = ["None", "Default", "Low", "High", "Max", "Random"];
	private static readonly string[] _pmcFactionList = ["Default", "USEC", "BEAR"];
	private static readonly string[] _forceAllBotTypeList = ["Disabled", "SCAV", "PMC"];

	public static Dictionary<string, int[]> GroupChanceWeights => _groupChanceWeights;
	
	public static BotDifficulty[] BotDifficulties { get; } =
		[BotDifficulty.easy, BotDifficulty.normal, BotDifficulty.hard, BotDifficulty.impossible];

	private static string ConvertIntArrayToString(int[] array) => string.Join(",", array);

	// IMGUI Vars
	private static readonly string[] _botDifficultyOptions = ["AsOnline", "Easy", "Normal", "Hard", "Impossible"];

	internal static bool ShowGUI { get; set; }
	internal static Rect WindowRect { get; set; } = new(20, 20, 1664, 936);  // Default position and size

	// Scenario Selection
	internal static List<Folder> PmcScenarios { get; set; } = [];
	internal static List<Folder> PmcRandomScenarios { get; set; } = [];
	internal static List<Folder> ScavScenarios { get; set; } = [];
	internal static List<Folder> ScavRandomScenarios { get; set; } = [];
	
	internal static Setting<string> pmcScenarioSelection;
	internal static Setting<string> scavScenarioSelection;
	internal static string[] pmcScenarioCombinedArray;
	internal static string[] scavScenarioCombinedArray;

	// Temporarily store the scenario selections to initialize them later
	internal static string PmcScenarioSelectionValue { get; set; }
	internal static string ScavScenarioSelectionValue { get; set; }

	static DefaultPluginVars()
	{
		InitMainSettings();

		InitGlobalMinSpawnDistFromPlayer();
		InitGlobalMinSpawnDistFromOtherBots();

		InitAdvancedSettings();
		InitDebuggingSettings();
		InitSpawnPointMakerSettings();
		InitSaveSettings();
	}

	private static void InitMainSettings()
	{
		PluginEnabled = new Setting<bool>("Donuts 开启", "启用/禁用从Donuts点生成",
			true, true);

		PluginEnabled.OnSettingChanged += _ =>
		{
			if (MonoBehaviourSingleton<DonutsRaidManager>.Instantiated)
			{
				MonoBehaviourSingleton<DonutsRaidManager>.Instance.enabled = PluginEnabled.Value;
			}
		};

		DespawnEnabledPMC = new Setting<bool>("移除PMC",
            "启用后,当动态生成的机器人超过您在Donuts机器人上限(ScenarioConfig.json)时,移除离玩家最远的PMC机器人.",
			true, true);

		DespawnEnabledSCAV = new Setting<bool>("移除SCAV",
            "启用后,当动态生成的机器人超过您在Donuts机器人上限(ScenarioConfig.json)时,移除离玩家最远的SCAV机器人.",
			true, true);

		HardCapEnabled = new Setting<bool>("机器人硬上限选项",
            "启用后,所有机器人生成将受到您预设上限的严格限制.换句话说,如果您的机器人数量已达到Donuts上限,则在有机器人死亡之前不会再生成新的机器人(类似于原版SPT行为).",
			false, false);

		coolDownTimer = new Setting<float>("冷却计时器",
            "成功生成机器人后，生成标记的冷却计时器(MaxSpawnsBeforeCooldown)",
			300f, 300f, 0f, 1000f);

		pmcGroupChance = new Setting<string>("Donuts PMC组生成几率",
            "用于确定PMC组生成几率及组大小的设置.所有几率均可配置,请查看上方的高级设置.更多详情请参阅模组页面.",
			"Default", "Default", null, null, _pmcGroupChanceList);

		scavGroupChance = new Setting<string>("Donuts SCAV组生成几率",
            "用于确定SCAV组生成几率及组大小的设置.所有几率均可配置,请查看上方的高级设置.更多详情请参阅模组页面.",
			"Default", "Default", null, null, _scavGroupChanceList);

		botDifficultiesPMC = new Setting<string>("Donuts PMC生成难度",
            "所有PMC Donuts相关生成的难度设置",
			"Normal", "Normal", null, null, _botDifficultyOptions);

		botDifficultiesSCAV = new Setting<string>("Donuts SCAV生成难度",
            "所有SCAV Donuts相关生成的难度设置",
			"Normal", "Normal", null, null, _botDifficultyOptions);

		botDifficultiesOther = new Setting<string>("其他机器人类型生成难度",
            "使用Donuts生成的所有其他机器人类型(如Boss,Rogue,Raider等)的难度设置.",
			"Normal", "Normal", null, null, _botDifficultyOptions);

		ShowRandomFolderChoice = new Setting<bool>("显示生成预设选择",
            "在突袭开始时,在右下角显示所选的生成预设", true, true);

		pmcFaction = new Setting<string>("强制 PMC 阵营",
            "强制所有 PMC 生成点使用特定阵营,或使用 Donuts 生成文件中指定的默认阵营.默认为随机阵营.",
			"Default", "Default", null, null, _pmcFactionList);

		forceAllBotType = new Setting<string>("强制所有生成的机器人类型",
            "为所有生成强制指定机器人类型——此选项将所有定义的初始生成和波次转换为指定的机器人类型.默认情况下为禁用.",
			"Disabled", "Disabled", null, null, _forceAllBotTypeList);

		useTimeBasedHardStop = new Setting<bool>("使用基于时间的硬停止",
            "如果启用,硬停止设置将基于突袭剩余时间(以秒为单位,可在下方配置).如果禁用,硬停止设置将基于突袭剩余时间的百分比(可在下方配置).",
			true, true);

		hardStopOptionPMC = new Setting<bool>("PMC生成硬停止",
            "如果启用,当撤离剩余时间达到指定的时间或百分比时,所有 PMC 的生成点将完全停止.这可以使用秒数或百分比进行配置(见下方).",
			false, false);

		hardStopPercentPMC = new Setting<int>("PMC 生成点硬停止:撤离剩余时间百分比",
            "当撤离剩余时间达到此百分比时,将停止任何进一步的 PMC 生成(如果该选项已启用).默认为完整撤离时间的 50%.",
			50, 50, 0, 100);

		hardStopTimePMC = new Setting<int>("PMC 生成点硬停止:撤离剩余时间",
            "当撤离剩余时间达到此时间(以秒为单位)时,将停止任何进一步的 PMC 生成(如果该选项已启用).默认为 300 秒(5 分钟).",
			300, 300);

		hardStopOptionSCAV = new Setting<bool>("SCAV 生成点硬停止",
            "如果启用,当撤离剩余时间达到指定的时间或百分比时,所有 SCAV 的生成点将完全停止.这可以使用秒数或百分比进行配置(见下方).",
			false, false);

		hardStopTimeSCAV = new Setting<int>("SCAV 生成点硬停止:撤离剩余时间",
            "当撤离剩余时间达到此时间(以秒为单位)时,将停止任何进一步的 SCAV 生成(如果该选项已启用).默认为 300 秒(5 分钟).",
			300, 300);

		hardStopPercentSCAV = new Setting<int>("SCAV 生成点硬停止:撤离剩余时间百分比",
            "当撤离剩余时间达到此百分比时.将停止任何进一步的 SCAV 生成(如果该选项已启用).默认为完整撤离时间的 10%.",
			10, 10, 0, 100);

		hotspotBoostPMC = new Setting<bool>("PMC 热点刷新提升",
            "如果启用,所有热点区域将有更高的几率刷新更多的 PMC.",
			false, false);

		hotspotBoostSCAV = new Setting<bool>("SCAV 热点刷新提升",
            "如果启用,所有热点区域将有更高的几率刷新更多的 SCAV.",
			false, false);

		hotspotIgnoreHardCapPMC = new Setting<bool>("PMC 热点忽略硬上限",
            "如果启用,所有热点生成点都将忽略硬上限(如果已启用).这适用于任何标记为'热点'的生成点.我建议将此选项与 Despawn(清除) + 硬上限 + Boost(加速)结合使用,以获得最佳体验,并在热点区域获得更多行动.",
			false, false);

		hotspotIgnoreHardCapSCAV = new Setting<bool>("SCAV 热点忽略硬上限",
            "如果启用,所有热点生成点都将忽略硬上限(如果已启用).这适用于任何标记为'热点'的生成点.我建议将此选项与 Despawn(清除) + 硬上限 + Boost(加速)结合使用,以获得最佳体验,并在热点区域获得更多行动.",
			false, false);

		pmcFactionRatio = new Setting<int>("PMC 阵营比例",
            "USEC/Bear 默认比例.默认为 50%.数值越低 = USEC 的几率越低,因此:20 表示 20% USEC,80% Bear,以此类推.",
			50, 50);

		battleStateCoolDown = new Setting<float>("战斗中生成冷却",
            "在你未受到攻击达到X秒后,才会停止刷新AI,因为你仍然被认为处于战斗状态.",
			20f, 20f);

		maxRespawnsPMC = new Setting<int>("每次战局中PMC的最大重生次数",
            "一旦 Donuts 在一个战局中生成了这么多次PMC,它就会跳过所有后续触发的PMC生成.默认为0（无限制).",
			0, 0);

		maxRespawnsSCAV = new Setting<int>("每次战局中SCAV的最大重生次数",
            "一旦 Donuts 在一个战局中生成了这么多次PMC,它就会跳过所有后续触发的PMC生成.默认为0（无限制).",
			0, 0);
	}

	private static void InitGlobalMinSpawnDistFromPlayer()
	{
		globalMinSpawnDistanceFromPlayerBool = new Setting<bool>("使用全局最小玩家距离",
            "如果启用,所有预设中的所有生成点都将使用下面定义的每个地图的全局最小玩家生成距离.",
			true, true);
		
		const string tooltipText = "机器人应该离玩家(你)的距离(以米为单位).";

		globalMinSpawnDistanceFromPlayerFactory = new Setting<float>("工厂", tooltipText, 35f, 35f);
		globalMinSpawnDistanceFromPlayerCustoms = new Setting<float>("海关", tooltipText, 60f, 60f);
		globalMinSpawnDistanceFromPlayerReserve = new Setting<float>("储备站", tooltipText, 80f, 80f);
		globalMinSpawnDistanceFromPlayerStreets = new Setting<float>("街区", tooltipText, 80f, 80f);
		globalMinSpawnDistanceFromPlayerWoods = new Setting<float>("森林", tooltipText, 110f, 110f);
		globalMinSpawnDistanceFromPlayerLaboratory = new Setting<float>("实验室", tooltipText, 40f, 40f);
		globalMinSpawnDistanceFromPlayerShoreline = new Setting<float>("海岸线", tooltipText, 100f, 100f);
		globalMinSpawnDistanceFromPlayerGroundZero = new Setting<float>("中心区", tooltipText, 50f, 50f);
		globalMinSpawnDistanceFromPlayerInterchange = new Setting<float>("立交桥", tooltipText, 85f, 85f);
		globalMinSpawnDistanceFromPlayerLighthouse = new Setting<float>("灯塔", tooltipText, 70f, 70f);
	}

	private static void InitGlobalMinSpawnDistFromOtherBots()
	{
		globalMinSpawnDistanceFromOtherBotsBool = new Setting<bool>("使用与其他机器人之间的全局最小距离",
            "如果启用,所有预设中的所有刷新点都将使用下面定义的每个地图的与玩家之间的全局最小刷新距离.",
			true, true);

		const string tooltipText = "机器人与其他存活机器人之间应保持的刷新距离(以米为单位).";

		globalMinSpawnDistanceFromOtherBotsFactory = new Setting<float>("工厂", tooltipText, 15f, 15f);
		globalMinSpawnDistanceFromOtherBotsCustoms = new Setting<float>("海关", tooltipText, 40f, 50f);
		globalMinSpawnDistanceFromOtherBotsReserve = new Setting<float>("储备站", tooltipText, 50f, 50f);
		globalMinSpawnDistanceFromOtherBotsStreets = new Setting<float>("街区", tooltipText, 50f, 50f);
		globalMinSpawnDistanceFromOtherBotsWoods = new Setting<float>("森林", tooltipText, 80f, 80f);
		globalMinSpawnDistanceFromOtherBotsLaboratory = new Setting<float>("实验室", tooltipText, 20f, 20f);
		globalMinSpawnDistanceFromOtherBotsShoreline = new Setting<float>("海岸线", tooltipText, 60f, 60f);
		globalMinSpawnDistanceFromOtherBotsGroundZero = new Setting<float>("中心区", tooltipText, 30f, 30f);
		globalMinSpawnDistanceFromOtherBotsInterchange = new Setting<float>("立交桥", tooltipText, 65f, 65f);
		globalMinSpawnDistanceFromOtherBotsLighthouse = new Setting<float>("灯塔", tooltipText, 60f, 60f);
	}

	private static void InitAdvancedSettings()
	{
		maxRaidDelay = new Setting<float>("游戏准备时间延迟",
            "Donuts强制延迟游戏准备的最长时间(以秒为单位),以便有时间为所有起始点生成机器人数据.这是为了避免在游戏开始时可能出现的机器人刷新延迟.默认值为 60 秒.",
			60f, 60f, 0f, 120f);

		replenishInterval = new Setting<float>("机器人缓存补充间隔",
            "Donuts 重新填充其机器人数据缓存的时间间隔,以秒为单位.除非您清楚自己在做什么,否则请保留默认值.",
			10f, 10f, 0f, 300f);

		maxSpawnTriesPerBot = new Setting<int>("每个机器人的最大生成尝试次数",
            "在尝试这么多次寻找合适的生成点后,它将停止尝试生成其中一个机器人.数值越低越好.",
			1, 1, 0, 10);

		despawnInterval = new Setting<float>("移除机器人间隔",
            "此数值表示 Donuts 应该在多少秒后移除机器人.默认值为 15 秒.注意:降低此数值可能会影响您的性能.",
			15f, 15f, 5f, 600f);

		groupWeightDistroLow = new Setting<string>("成群几率权重:低",
            "'低'成群几率的权重分配.分别使用相对权重来设置群组大小 1/2/3/4/5 的权重.使用此公式:群组权重 / 总权重 = % 几率.",
			_lowWeightsString, _lowWeightsString);

		groupWeightDistroLow.OnSettingChanged +=
			_ => GroupChanceWeights["Low"] = ParseGroupWeightDistro(groupWeightDistroLow.Value);

		groupWeightDistroDefault = new Setting<string>("成群几率权重：默认",
            "'默认'成群几率的权重分配.分别使用相对权重来设置群组大小 1/2/3/4/5 的权重.使用此公式:群组权重 / 总权重 = % 几率.",
			_defaultWeightsString, _defaultWeightsString);
		
		groupWeightDistroDefault.OnSettingChanged +=
			_ => GroupChanceWeights["Default"] = ParseGroupWeightDistro(groupWeightDistroDefault.Value);

		groupWeightDistroHigh = new Setting<string>("成群几率权重:高",
            "'高'成群几率的权重分配.分别使用相对权重来设置群组大小 1/2/3/4/5 的权重.使用此公式:群组权重 / 总权重 = % 几率.",
			_highWeightsString, _highWeightsString);
		
		groupWeightDistroHigh.OnSettingChanged +=
			_ => GroupChanceWeights["High"] = ParseGroupWeightDistro(groupWeightDistroHigh.Value);
	}

	private static void InitDebuggingSettings()
	{
		debugLogging = new Setting<bool>("启用调试日志记录",
            "启用后,会将调试日志输出到 BepInEx 控制台和 LogOutput.log 文件.", false, false);
		
		DebugGizmos = new Setting<bool>("启用调试标记",
            "启用后,会在从 JSON 文件设置的生成点上绘制调试用的球体.", false, false);

		gizmoRealSize = new Setting<bool>("调试球体的实际大小",
            "启用后,调试球体的大小将与生成半径的实际大小一致.", false, false);
	}

	private static void InitSpawnPointMakerSettings()
	{
		spawnName = new Setting<string>("名称", "用于识别生成标记的名称",
            "在此处输入生成名称", "在此处输入生成名称");

		groupNum = new Setting<int>("组号", "用于识别生成标记的组号",
			1, 1, 1, 100);

		wildSpawns = new Setting<string>("野生生成类型", "选择一个选项.", "pmc", "pmc",
			null, null, _wildSpawnTypes);

		minSpawnDist = new Setting<float>("最小生成距离",
            "AI从你设置的标记点生成的最小距离.", 1f, 1f, 0f, 500f);

		maxSpawnDist = new Setting<float>("最大生成距离",
            "AI从你设置的标记点生成的最大距离.", 20f, 20f, 1f, 1000f);

		botTriggerDistance = new Setting<float>("触发AI生成的距离",
            "玩家距离战斗地点达到触发AI生成的距离",
			100f, 100f, 0.1f, 1000f);

		botTimerTrigger = new Setting<float>("AI生成计时器触发",
            "当玩家在战斗区域内时,生成下一波AI的间隔时间(秒).",
			180f, 180f, 0f, 10000f);

		maxRandNumBots = new Setting<int>("最大随机AI数量",
            "在此标记点可生成的'野生生成类型'AI的最大数量",
			2, 2, 1, 5);

		spawnChance = new Setting<int>("标记点生成几率",
            "计时器到达后,在此处生成AI的几率", 50, 50, 0, 100);

		maxSpawnsBeforeCooldown = new Setting<int>("冷却之前的最大生成次数",
            "此标记点进入冷却前的成功生成次数", 5, 5, 1, 30);

		ignoreTimerFirstSpawn = new Setting<bool>("忽略首次生成的计时器",
            "启用后,即使计时器未准备好,此标记点仍然会生成,但仅限于首次生成.",
			false, false);

		minSpawnDistanceFromPlayer = new Setting<float>("与玩家的最小生成距离",
            "在生成标记点附近随机选择的生成位置与玩家的最小距离",
			40f, 40f, 0f, 500f);

		CreateSpawnMarkerKey = new Setting<KeyCode>("创建生成标记点按键",
            "按下此键可以在您当前位置创建一个生成标记点.",
			KeyCode.None, KeyCode.None);

		DeleteSpawnMarkerKey = new Setting<KeyCode>("删除生成标记点按键",
            "按下此键可以删除距离您当前位置5米范围内最近的生成标记点.",
			KeyCode.None, KeyCode.None);
	}

	private static void InitSaveSettings()
	{
		saveNewFileOnly = new Setting<bool>("仅保存新位置",
            "如果启用,则将战局会话的更改保存到一个新文件.如果禁用,则会将你能看到的所有位置保存到一个新文件.",
			false, false);

		WriteToFileKey = new Setting<KeyCode>("创建临时Json文件",
            "按下此键可将包含目前所有条目的json文件写入.",
			KeyCode.KeypadMinus, KeyCode.KeypadMinus);
	}
	
	private static int[] ParseGroupWeightDistro(string weightsString)
	{
		// Use the Split(char[]) method and manually remove empty entries
		return weightsString
			.Split(',')
			.Where(s => !string.IsNullOrWhiteSpace(s))
			.Select(int.Parse)
			.ToArray();
	}
}