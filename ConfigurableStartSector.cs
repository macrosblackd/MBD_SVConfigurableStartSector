using System;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

namespace MBD_SVConfigurableStartSector
{
    internal enum SectorType
    {
        Normal = 0,
        Nebula = 1,
        PitchBlack = 2,
        AsteroidRush = 3
    }

    internal enum SectorSize
    {
        VerySmall = 1,
        Small = 2,
        Medium = 3,
        Large = 4
    }

    internal enum SectorBigAsteroidDensity
    {
        None = -1, // No big asteroids
        Random = 0, // Random density
        VeryDense = 1 // Very dense big asteroids
    }

    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    public class ConfigurableStartSector : BaseUnityPlugin
    {
        private const string pluginGuid = "macrosblackd.starvalormods.configurablestartsector";
        private const string pluginName = "Configurable Start Sector";
        private const string pluginVersion = "1.1.0";

        private static readonly ManualLogSource logger = BepInEx.Logging.Logger.CreateLogSource(pluginName);

        private static ConfigEntry<SectorSize> startSectorSize;
        private static ConfigEntry<float> startSectorRichness;
        private static ConfigEntry<SectorBigAsteroidDensity> startSectorBigAsteroidDensity;
        private static ConfigEntry<SectorType> startSectorType;
        private static ConfigEntry<bool> pluginEnabled;
        private static ConfigEntry<bool> spawnMaxBigAsteroids;
        private static ConfigEntry<bool> spawnTinheadWorkshop;
        // TODO: Add a configurable option to remove all AI stations from the start sector.
        // TODO: Add a configurable option to remove all jumpgates from the start sector.
        // TODO: Add a configurable option to spawn a Rebel base in the start sector.
        // TODO: add a configurable option to spawn a Venghi base in the start sector.

        // TODO: Add a configurable option to spawn more debris fields in the start sector.
        // TODO: Add a configurable option to spawn more asteroid fields in the start sector.
        // TODO: Add a configurable option to enable more or periodic SOS events in the start sector.
        // TODO: Add a configurable option to spawn hidden debris fields in the start sector.
        // TODO: Add a configurable option to spawn extra big asteroids (above the max) in the start sector.
        // TODO: Add a configurable option to force all hideouts to be on a big asteroid in the start sector.

        public void Awake()
        {
            // This method is called when the plugin is loaded
            LoadConfig();
            Harmony.CreateAndPatchAll(typeof(ConfigurableStartSector));
            logger.LogInfo("ConfigurableStartSector plugin loaded successfully!");
        }

        private void LoadConfig()
        {
            startSectorSize = Config.Bind("Settings", "StartSectorSize", SectorSize.VerySmall, new ConfigDescription(
                "Size of the start sector (VerySmall (default), Small, Medium, Large)"));
            startSectorRichness = Config.Bind("Settings", "StartSectorRichness", 0.2f, new ConfigDescription(
                "Richness of the start sector (0.0 to 1.0, where 1.0 is very rich. Default value is 0.2f)", new AcceptableValueRange<float>(0.0f, 1.0f)));
            startSectorBigAsteroidDensity = Config.Bind("Settings", "StartSectorBigAsteroidDensity", SectorBigAsteroidDensity.Random,
                "Density of big asteroids in the start sector (None, Random (default), VeryDense)");
            startSectorType = Config.Bind("Settings", "StartSectorType", SectorType.Normal,
                "Type of the start sector (Normal, Nebula, AsteroidRush, PitchBlack)");
            pluginEnabled = Config.Bind("Settings", "PluginEnabled", true,
                                "Enable or disable the Configurable Start Sector plugin");

            spawnMaxBigAsteroids = Config.Bind("Settings.Spawns", "SpawnMaxBigAsteroids", false,
                                "When enabled, forces maximum big asteroid spawns in the start sector");
            spawnTinheadWorkshop = Config.Bind("Settings.Spawns", "SpawnTinheadWorkshop", false, new ConfigDescription(
                "When enabled, will force the start sector to spawn with a Tinhead Workshop, otherwise it will be left to RNG."
                ));
        }

        [HarmonyPatch(typeof(TSector), nameof(TSector.GenerateSectorData))]
        [HarmonyPrefix]
        public static void PreFix_TSector_GenerateSectorData(TSector __instance, int mode)
        {
            if (!pluginEnabled.Value)
            {
                return; // Skip if the plugin is disabled
            }

            if (mode == 1) // Assuming mode 1 is the start sector
            {
                logger.LogInfo($"Start sector set to {__instance.Index}");
                logger.LogInfo($"Initial start sector size set to {__instance.size}");
                logger.LogInfo($"Initial start sector richness set to {__instance.mineralLevel}");
                logger.LogInfo($"Initial start sector big asteroid {__instance.bigAsteroidDensity}");
                logger.LogInfo($"Initial start sector type {__instance.type.ToString()}");

                __instance.SetSectorSize((int)startSectorSize.Value);
                logger.LogInfo($"Start sector size changed to {__instance.size}");
                __instance.mineralLevel = startSectorRichness.Value;
                logger.Equals($"Start sector richness changed to {__instance.mineralLevel}");
                __instance.bigAsteroidDensity = (int)startSectorBigAsteroidDensity.Value;
                logger.LogInfo($"Start sector big asteroid density changed to {__instance.bigAsteroidDensity}");
                __instance.type = (global::SectorType)startSectorType.Value;
                logger.LogInfo($"Start sector type changed to {__instance.type.ToString()}");
            }
        }

        [HarmonyPatch(typeof(TSector), MethodType.Constructor)]
        [HarmonyPatch(new Type[] { typeof(int), typeof(int), typeof(int), typeof(int), typeof(int) })]
        [HarmonyPostfix]
        public static void PostFix_TSector_Ctor(TSector __instance, int mode)
        {
            if(!pluginEnabled.Value || !spawnTinheadWorkshop.Value)
            {
                return;
            }

            logger.LogDebug($"PostFix_TSector_Ctor called with mode: {mode}");
            logger.LogDebug($"Sector has workshop? {__instance.HasStationOfFaction(6)}");
            if (!__instance.HasStationOfFaction(6) && mode == 1)
            {
                var coord = __instance.GetMainCoords(true);
                new Workshop(__instance.level, coord, 6, __instance.Index, 0, -1);
                logger.LogInfo($"Creating workshop at {coord.AsVector2.ToString()} in sector {__instance.Index}");
            }
        }

        [HarmonyPatch(typeof(TSector), nameof(TSector.GetObjectsQuantity))]
        [HarmonyPostfix]
        public static void PostFix_TSector_GetObjectsQuantity(TSector __instance, int objType, ref int __result)
        {
            if (!pluginEnabled.Value)
            {
                return; // Skip if the plugin is disabled
            }

            // Check if this is the starting sector, objType is 7, and if SpawnMaxBigAsteroids is enabled
            if (__instance.isStarterSector && objType == 7 && spawnMaxBigAsteroids.Value)
            {
                // Log the original result for debugging
                logger.LogInfo($"TSector.GetObjectsQuantity() called for sector {__instance.Index} with objType {objType}, original result: {__result}");
                // Force maximum big asteroid spawns when objType is 7 for the starting sector
                __result = __instance.sizeSqr + 3;
                logger.LogInfo($"SpawnMaxBigAsteroids enabled - forcing maximum big asteroid spawns for starting sector (index 1) with objType 7. New result: {__result} (based on sector size: {__instance.size})");
                logger.LogInfo($"TSector.GetObjectsQuantity() final result: {__result}");
            }

        }
    }
}
