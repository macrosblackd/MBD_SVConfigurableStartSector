using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Assertions.Must;

namespace MBD_SVConfigurableStartSector
{
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    public class ConfigurableStartSector : BaseUnityPlugin
    {
        private const string pluginGuid = "macrosblackd.starvalormods.configurablestartsector";
        private const string pluginName = "Configurable Start Sector";
        private const string pluginVersion = "1.0.0";

        private static ManualLogSource logger = BepInEx.Logging.Logger.CreateLogSource(pluginName);

        private static ConfigEntry<int> startSectorSize;
        private static ConfigEntry<float> startSectorRichness;
        private static ConfigEntry<int> startSectorBigAsteroidDensity;
        private static ConfigEntry<SectorType> startSectorType;

        public void Awake()
        {
            // This method is called when the plugin is loaded
            LoadConfig();
            Harmony.CreateAndPatchAll(typeof(ConfigurableStartSector));
            logger.LogInfo("ConfigurableStartSector plugin loaded successfully!");
        }

        private void LoadConfig()
        {
            startSectorSize = Config.Bind("Settings", "StartSectorSize", 1,
                "Size of the start sector (1: Very Small, 2: Small, 3: Medium, 4: Large)");
            startSectorRichness = Config.Bind("Settings", "StartSectorRichness", 0.2f,
                "Richness of the start sector (0.0 to 1.0, where 1.0 is very rich)");
            startSectorBigAsteroidDensity = Config.Bind("Settings", "StartSectorBigAsteroidDensity", 1,
                "Density of big asteroids in the start sector (-1 is none, 0 is random (default), and 1 is very dense)");
            startSectorType = Config.Bind("Settings", "StartSectorType", SectorType.Normal,
                "Type of the start sector (Normal, Nebula, PitchBlack,)");

            AcceptableValueList<int> sectorSizeOptions = new AcceptableValueList<int>(-1, 0, 1);
            if (!sectorSizeOptions.IsValid(startSectorBigAsteroidDensity.Value))
            {
                startSectorBigAsteroidDensity.Value = 0; // Default to random if invalid
            }

            AcceptableValueList<int> sectorTypeOptions = new AcceptableValueList<int>(
                (int)SectorType.Normal, 
                (int)SectorType.Nebula, 
                (int)SectorType.PitchBlack
            );
            if (!sectorTypeOptions.IsValid((int)startSectorType.Value))
            {
                startSectorType.Value = SectorType.Normal; // Default to PitchBlack if invalid
            }
        }

        [HarmonyPatch(typeof(TSector), nameof(TSector.GenerateSectorData))]
        [HarmonyPrefix]
        public static void PreFix_TSector_GenerateSectorData(TSector __instance, int mode)
        {
            if (mode == 1) // Assuming mode 1 is the start sector
            {
                logger.LogInfo($"Start sector set to {__instance.Index}");
                logger.LogInfo($"Initial start sector size set to {__instance.size}");
                logger.LogInfo($"Initial start sector richness set to {__instance.mineralLevel}");
                logger.LogInfo($"Initial start sector big asteroid {__instance.bigAsteroidDensity}");
                logger.LogInfo($"Initial start sector type {__instance.type.ToString()}");

                __instance.SetSectorSize(startSectorSize.Value);
                logger.LogInfo($"Start sector size changed to {__instance.size}");
                __instance.mineralLevel = startSectorRichness.Value;
                logger.Equals($"Start sector richness changed to {__instance.mineralLevel}");
                __instance.bigAsteroidDensity = startSectorBigAsteroidDensity.Value;
                logger.LogInfo($"Start sector big asteroid density changed to {__instance.bigAsteroidDensity}");
                __instance.type = startSectorType.Value;
                logger.LogInfo($"Start sector type changed to {__instance.type.ToString()}");
                // Remove the default stations and planets
                //for (int i = __instance.stationIDs.Count - 1; i >= 0; i--)
                //{
                //    var station = GameData.GetStation(__instance.stationIDs[i]);
                //    if (station != null)
                //    {
                //        if (station.hasContacts)
                //        {
                //            (station.GetModule(65, false) as SM_Contacts)?.RemoveContacts(0, station.id);
                //        }
                //        PChar.Char.codex.RemoveStationRecord(station.id);
                //        station.ForceRemoveFromData();
                //    }
                //}
                //__instance.planets.Clear();
                //ExploreWholeSector(__instance);
            }
        }

        //[HarmonyPatch(typeof(TSector), MethodType.Constructor)]
        //[HarmonyPatch(new Type[] { typeof(int), typeof(int), typeof(int), typeof(int), typeof(int) })]
        //[HarmonyPrefix]
        //public static bool Prefix_TSector_Ctor(TSector __instance, int mode, int cX, int cY, int newLevel, int newFactionControl)
        //{
        //    if(mode != 1) // Assuming mode 1 is the start sector
        //    {
        //        return true; // Continue with the original constructor
        //    }

        //    if (mode == 1) // Assuming mode 1 is the start sector
        //    {

        //    }

        //    return false; // Skip the original constructor
        //}

        //[HarmonyPatch(typeof(TSector), MethodType.Constructor)]
        //[HarmonyPatch(new Type[] { typeof(int), typeof(int), typeof(int), typeof(int), typeof(int) })]
        //[HarmonyPostfix]
        //public static void PostFix_TSector_Ctor(TSector __instance, int mode)
        //{
        //    if (mode == 1) // Assuming mode 1 is the start sector
        //    {
        //        logger.LogInfo($"Start sector set to {__instance.Index}");
        //        logger.LogInfo($"Initial start sector size set to {__instance.size}");
        //        __instance.SetSectorSize(4); // Assuming size 4 is for Large
        //        logger.LogInfo($"Start sector size changed to {__instance.size}");
        //        //__instance.bg = new Background(__instance.x, __instance.y, SectorType.Normal, false);

        //        // Remove the default stations and planets
        //        for(int i = __instance.stationIDs.Count - 1; i >= 0; i--)
        //        {
        //            var station = GameData.GetStation(__instance.stationIDs[i]);
        //            if (station != null)
        //            {
        //                if (station.hasContacts)
        //                {
        //                    (station.GetModule(65, false)as SM_Contacts)?.RemoveContacts(0, station.id);
        //                }
        //                PChar.Char.codex.RemoveStationRecord(station.id);
        //                station.ForceRemoveFromData();
        //            }
        //        }
        //        __instance.planets.Clear();

        //    }
        //}

        /// <summary>
        /// Reveals the entire sector by destroying all Fog of War (FOW) tiles.
        /// This method iterates through all FOW tiles in the provided <paramref name="instance"/> of <see cref="TSector"/>
        /// and calls <see cref="TSector.DestroyFOWTile"/> on each tile, making the whole sector visible to the player.
        /// </summary>
        /// <param name="instance">
        /// The <see cref="TSector"/> instance whose FOW tiles will be destroyed.
        /// </param>
        private static void ExploreWholeSector(TSector instance)
        {
            foreach(var fowTile in instance.fow.ToList())
            {
                instance.DestroyFOWTile(fowTile);
            }
        }
    }
}
