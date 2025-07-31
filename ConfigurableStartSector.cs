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
        private const string pluginVersion = "1.0.0";

        private static ManualLogSource logger = BepInEx.Logging.Logger.CreateLogSource(pluginName);

        private static ConfigEntry<SectorSize> startSectorSize;
        private static ConfigEntry<float> startSectorRichness;
        private static ConfigEntry<SectorBigAsteroidDensity> startSectorBigAsteroidDensity;
        private static ConfigEntry<SectorType> startSectorType;
        private static ConfigEntry<bool> pluginEnabled;

        private static AcceptableValueList<int> sectorTypeOptions;

        public void Awake()
        {
            // This method is called when the plugin is loaded
            LoadConfig();
            Harmony.CreateAndPatchAll(typeof(ConfigurableStartSector));
            logger.LogInfo("ConfigurableStartSector plugin loaded successfully!");
            startSectorType.SettingChanged += (sender, args) =>
            {
                if (!sectorTypeOptions.IsValid((int)startSectorType.Value))
                {
                    startSectorType.Value = SectorType.Normal; // Default to Normal if invalid
                }
            };
        }

        private void LoadConfig()
        {
            startSectorSize = Config.Bind("Settings", "StartSectorSize", SectorSize.VerySmall,
                "Size of the start sector (VerySmall (default), Small, Medium, Large)");
            startSectorRichness = Config.Bind("Settings", "StartSectorRichness", 0.2f,
                "Richness of the start sector (0.0 to 1.0, where 1.0 is very rich. Default value is 0.2f)");
            startSectorBigAsteroidDensity = Config.Bind("Settings", "StartSectorBigAsteroidDensity", SectorBigAsteroidDensity.Random,
                "Density of big asteroids in the start sector (None, Random (default), VeryDense)");
            startSectorType = Config.Bind("Settings", "StartSectorType", SectorType.Normal,
                "Type of the start sector (Normal, Nebula, AsteroidRush, PitchBlack)");
            pluginEnabled = Config.Bind("Settings", "PluginEnabled", true,
                                "Enable or disable the Configurable Start Sector plugin");

            AcceptableValueList<int> sectorSizeOptions = new AcceptableValueList<int>(-1, 0, 1);


            AcceptableValueList<int> sectorBigAsteroidDensityOptions = new AcceptableValueList<int>(-1, 0, 1);
            if (!sectorBigAsteroidDensityOptions.IsValid(startSectorBigAsteroidDensity.Value))
            {
                startSectorBigAsteroidDensity.Value = 0; // Default to random if invalid
            }

            // Define acceptable values for sector types
            sectorTypeOptions = new AcceptableValueList<int>(
                (int)SectorType.Normal,
                (int)SectorType.Nebula,
                (int)SectorType.PitchBlack,
                (int)SectorType.AsteroidRush
            );

            if (!sectorTypeOptions.IsValid((int)startSectorType.Value))
            {
                startSectorType.Value = SectorType.Normal; // Default to Normal if invalid
            }
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
    }
}
