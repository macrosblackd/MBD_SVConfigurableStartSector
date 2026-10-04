using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using UnityEngine;
using BepInEx.Configuration;
using HarmonyLib;

namespace MBD_SVConfigurableStartSector
{
    public enum SectorType
    {
        Normal = 0,
        Nebula = 1,
        PitchBlack = 3,
        AsteroidRush = 2
    }

    public enum SectorSize
    {
        VerySmall = 1,
        Medium = 3,
        Small = 2,
        Large = 4
    }

    public enum MineralRichness
    {
        VeryPoor = 0,
        Poor = 1,
        Medium = 2,
        Rich = 3,
        VeryRich = 4
    }

    public enum BigAsteroidMode
    {
        None = 0,
        VanillaRandom = 1,
        VeryDense = 2,
        Maximum = 3
    }

    public enum SunColorPreset
    {
        Vanilla = 0,
        White = 1,
        WarmYellow = 2,
        Orange = 3,
        Red = 4,
        BlueWhite = 5,
        Blue = 6,
        Purple = 7,
        Green = 8
    }

    public enum SolarOutputPreset
    {
        None = 0,
        VeryLow = 30,
        Low = 45,
        Medium = 60,
        High = 75,
        VeryHigh = 90,
        ExtremelyHigh = 100
    }

    public enum StartingSectorPopulation
    {
        Empty = 0,
        Sparse = 25,
        Normal = 50,
        Busy = 75,
        Crowded = 100
    }

    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    public class ConfigurableStartSector : BaseUnityPlugin
    {
        public const string pluginGuid = "starvalormods.configurablestartsector";
        public const string pluginName = "Configurable Start Sector";
        public const string pluginVersion = "2.9.0";

        internal static ConfigEntry<SectorSize> startSectorSize;
        internal static ConfigEntry<MineralRichness> mineralRichness;
        internal static ConfigEntry<BigAsteroidMode> bigAsteroidMode;
        internal static ConfigEntry<SectorType> startSectorType;
        internal static ConfigEntry<bool> pluginEnabled;
        internal static ConfigEntry<bool> spawnTinheadWorkshop;

        internal static ConfigEntry<bool> overrideSolarOutput;
        internal static ConfigEntry<SolarOutputPreset> startSectorSolarOutput;

        internal static ConfigEntry<bool> overrideSunSize;
        internal static ConfigEntry<float> startSectorSunSize;
        internal static ConfigEntry<SunColorPreset> sunColorPreset;
        internal static ConfigEntry<StartingSectorPopulation> startingSectorPopulation;

        internal static ManualLogSource Log;
        private static Dictionary<int, float> originalSunSizeBySector = new Dictionary<int, float>();
        private static bool warnedSunColor;
        private static bool warnedSunSize;
        private static bool snappingSunSize;

        private void Awake()
        {
            Log = Logger;
            LoadConfig();

            startSectorSunSize.SettingChanged += OnSunSizeSettingChanged;
            SnapSunSizeConfigValue();

            // Match the proven v3.1 architecture: patch only this plugin class.
            Harmony.CreateAndPatchAll(typeof(ConfigurableStartSector), null);

            Logger.LogInfo("ConfigurableStartSector plugin loaded successfully!");
        }

        private void OnSunSizeSettingChanged(object sender, EventArgs e)
        {
            SnapSunSizeConfigValue();
        }

        private void SnapSunSizeConfigValue()
        {
            if (snappingSunSize || startSectorSunSize == null)
                return;

            float snapped = RoundToHundredth(startSectorSunSize.Value);

            if (snapped < 0.10f)
                snapped = 0.10f;
            else if (snapped > 1.50f)
                snapped = 1.50f;

            if (Math.Abs(startSectorSunSize.Value - snapped) < 0.0001f)
                return;

            try
            {
                snappingSunSize = true;
                startSectorSunSize.Value = snapped;
            }
            finally
            {
                snappingSunSize = false;
            }
        }

        private void LoadConfig()
        {
            pluginEnabled = Config.Bind(
                "General",
                "PluginEnabled",
                true,
                "Enable or disable the Configurable Start Sector plugin"
            );
            startSectorSize = Config.Bind(
                "Settings",
                "StartSectorSize",
                SectorSize.VerySmall,
                "Size of the start sector (VerySmall (default), Medium, Small, Large)"
            );
            mineralRichness = Config.Bind(
                "Settings",
                "MineralRichness",
                MineralRichness.VeryPoor,
                "Mineral richness of the start sector (VeryPoor=0.20, Poor=0.35, Medium=0.50, Rich=0.70, VeryRich=1.00)"
            );
            bigAsteroidMode = Config.Bind(
                "Settings",
                "BigAsteroidMode",
                BigAsteroidMode.VanillaRandom,
                "Big asteroid mode for the start sector (None, VanillaRandom (default), VeryDense, Maximum). Maximum uses the proven max-count override."
            );
            startSectorType = Config.Bind(
                "Settings",
                "StartSectorType",
                SectorType.Normal,
                "Type of the start sector (Normal, Nebula, AsteroidRush, PitchBlack)"
            );
            spawnTinheadWorkshop = Config.Bind(
                "Settings.Spawns",
                "SpawnTinheadWorkshop",
                false,
                "When enabled, will force the start sector to spawn with a Tinhead Workshop, otherwise it will be left to RNG."
            );
            overrideSolarOutput = Config.Bind(
                "Solar Output",
                "OverrideSolarOutput",
                false,
                "When enabled, overrides gameplay solar output in the starting sector only. Pitch Black is always locked to 0."
            );
            startSectorSolarOutput = Config.Bind(
                "Solar Output",
                "StartSectorSolarOutput",
                SolarOutputPreset.Medium,
                "Starting-sector gameplay solar output preset: None=0%, Very Low=30%, Low=45%, Medium=60%, High=75%, Very High=90%, Extremely High=100%."
            );
            overrideSunSize = Config.Bind(
                "Sun Visuals",
                "OverrideSunSize",
                false,
                "Override the visual size of the starting-sector sun. Gameplay Solar Output is preserved separately."
            );
            startSectorSunSize = Config.Bind(
                "Sun Visuals",
                "SunSize",
                1.0f,
                new ConfigDescription(
                    "Visual sun size. User-tested safe range is 0.10 to 1.50. Config values snap to the nearest 0.01.",
                    new AcceptableValueRange<float>(0.10f, 1.50f)
                )
            );
            sunColorPreset = Config.Bind(
                "Sun Visuals",
                "SunColor",
                SunColorPreset.Vanilla,
                "Visual sun color preset for the starting sector. Vanilla leaves the generated color unchanged."
            );
            startingSectorPopulation = Config.Bind(
                "Settings",
                "StartingSectorPopulation",
                StartingSectorPopulation.Normal,
                "Starting-sector population only: Empty=0, Sparse=25, Normal=50, Busy=75, Crowded=100. Native ship density responds automatically."
            );
        }

        private static float RoundToHundredth(float value)
        {
            return (float)(Math.Round(value * 100.0) / 100.0);
        }

        private static object GetMemberValue(object obj, string name)
        {
            if (obj == null)
                return null;

            Type t = obj.GetType();
            FieldInfo f = t.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (f != null)
                return f.GetValue(obj);

            PropertyInfo p = t.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (p != null && p.CanRead)
                return p.GetValue(obj, null);

            return null;
        }

        private static bool TryGetFloatMember(object obj, string name, out float value)
        {
            value = 0f;
            if (obj == null)
                return false;

            Type t = obj.GetType();
            FieldInfo f = t.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (f != null && f.FieldType == typeof(float))
            {
                value = (float)f.GetValue(obj);
                return true;
            }

            PropertyInfo p = t.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (p != null && p.CanRead && p.PropertyType == typeof(float))
            {
                value = (float)p.GetValue(obj, null);
                return true;
            }

            return false;
        }

        private static bool TrySetFloatMember(object obj, string name, float value)
        {
            if (obj == null)
                return false;

            Type t = obj.GetType();
            FieldInfo f = t.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (f != null && f.FieldType == typeof(float))
            {
                f.SetValue(obj, value);
                return true;
            }

            PropertyInfo p = t.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (p != null && p.CanWrite && p.PropertyType == typeof(float))
            {
                p.SetValue(obj, value, null);
                return true;
            }

            return false;
        }

        private static bool TrySetTColor(object tColor, Color value)
        {
            if (tColor == null)
                return false;

            bool r = TrySetFloatMember(tColor, "r", value.r);
            bool g = TrySetFloatMember(tColor, "g", value.g);
            bool b = TrySetFloatMember(tColor, "b", value.b);
            bool a = TrySetFloatMember(tColor, "a", value.a);

            return r && g && b && a;
        }

        private static Color GetPresetColor(SunColorPreset preset)
        {
            switch (preset)
            {
                case SunColorPreset.White:
                    return new Color(1f, 1f, 1f, 1f);
                case SunColorPreset.WarmYellow:
                    return new Color(1f, 0.82f, 0.45f, 1f);
                case SunColorPreset.Orange:
                    return new Color(1f, 0.48f, 0.12f, 1f);
                case SunColorPreset.Red:
                    return new Color(1f, 0.15f, 0.08f, 1f);
                case SunColorPreset.BlueWhite:
                    return new Color(0.72f, 0.86f, 1f, 1f);
                case SunColorPreset.Blue:
                    return new Color(0.22f, 0.48f, 1f, 1f);
                case SunColorPreset.Purple:
                    return new Color(0.68f, 0.30f, 1f, 1f);
                case SunColorPreset.Green:
                    return new Color(0.28f, 1f, 0.38f, 1f);
                default:
                    return Color.white;
            }
        }

        private static void ApplySunVisuals(TSector sector)
        {
            if (sector == null)
                return;

            object bg = GetMemberValue(sector, "bg");
            if (bg == null)
                return;

            object sun = GetMemberValue(bg, "sunColor");
            if (sun == null)
                return;

            float originalSize;
            if (TryGetFloatMember(sun, "sunSize", out originalSize))
            {
                originalSunSizeBySector[sector.Index] = originalSize;

                if (overrideSunSize.Value)
                    TrySetFloatMember(sun, "sunSize", RoundToHundredth(startSectorSunSize.Value));
            }
            else if (overrideSunSize.Value && !warnedSunSize)
            {
                warnedSunSize = true;
                Log.LogWarning("Sun Visuals: could not find verified sunSize member; Sun Size override was not applied.");
            }

            if (sunColorPreset.Value != SunColorPreset.Vanilla)
            {
                if (!TrySetTColor(sun, GetPresetColor(sunColorPreset.Value)) && !warnedSunColor)
                {
                    warnedSunColor = true;
                    Log.LogWarning("Sun Visuals: verified TColor r/g/b/a fields could not all be written; color override was not applied.");
                }
            }
        }

        private static float GetMineralRichnessValue(MineralRichness value)
        {
            switch (value)
            {
                case MineralRichness.Poor:
                    return 0.35f;

                case MineralRichness.Medium:
                    return 0.50f;

                case MineralRichness.Rich:
                    return 0.70f;

                case MineralRichness.VeryRich:
                    return 1.00f;

                case MineralRichness.VeryPoor:
                default:
                    return 0.20f;
            }
        }

        [HarmonyPatch(typeof(TSector), "GenerateExtraSectorData")]
        [HarmonyPrefix]
        public static void Prefix_TSector_GenerateExtraSectorData(TSector __instance, int mode)
        {
            if (!pluginEnabled.Value)
                return;

            // Preserve the proven start-sector generation condition from v3.1.
            if (mode != 1)
                return;

            __instance.SetSectorSize((int)startSectorSize.Value);
            __instance.mineralLevel = GetMineralRichnessValue(mineralRichness.Value);
            switch (bigAsteroidMode.Value)
            {
                case BigAsteroidMode.None:
                    __instance.bigAsteroidDensity = -1;
                    break;

                case BigAsteroidMode.VeryDense:
                    __instance.bigAsteroidDensity = 1;
                    break;

                case BigAsteroidMode.Maximum:
                    // Reuse native VeryDense density so big-asteroid generation remains enabled.
                    // Final quantity is then forced by the proven GetObjectsQuantity override below.
                    __instance.bigAsteroidDensity = 1;
                    break;

                case BigAsteroidMode.VanillaRandom:
                default:
                    __instance.bigAsteroidDensity = 0;
                    break;
            }
            __instance.type = (global::SectorType)(int)startSectorType.Value;
        }

        [HarmonyPatch(typeof(TSector), "GenerateExtraSectorData")]
        [HarmonyPostfix]
        public static void PostFix_TSector_GenerateExtraSectorData_SunVisuals(TSector __instance, int mode)
        {
            if (!pluginEnabled.Value)
                return;

            if (mode != 1)
                return;

            ApplySunVisuals(__instance);
        }

        [HarmonyPatch(typeof(TSector), "GenerateExtraSectorData")]
        [HarmonyPostfix]
        public static void PostFix_TSector_GenerateExtraSectorData_Population(TSector __instance, int mode)
        {
            if (!pluginEnabled.Value)
                return;

            if (mode != 1 || __instance == null || !__instance.isStarterSector)
                return;

            // Starter-sector-only local population control.
            // Does not modify Quadrant.sectorDensity.
            __instance.population = (int)startingSectorPopulation.Value;
        }

        [HarmonyPatch(
            typeof(TSector),
            MethodType.Constructor,
            new Type[]
            {
                typeof(int),
                typeof(int),
                typeof(int),
                typeof(int),
                typeof(int),
                typeof(bool)
            }
        )]
        [HarmonyPostfix]
        public static void PostFix_TSector_Ctor(TSector __instance, int mode)
        {
            if (!pluginEnabled.Value || !spawnTinheadWorkshop.Value)
                return;

            if (mode != 1)
                return;

            // Native faction index used by the proven working v3.1 Tinhead Workshop path.
            if (__instance.HasStationOfFaction(6))
                return;

            Coordenates coords = __instance.GetMainCoords(true);

            // Preserve the exact proven constructor argument pattern from v3.1.
            new Workshop(
                __instance.level,
                coords,
                6,
                __instance.Index,
                0,
                -1
            );
        }

        [HarmonyPatch(typeof(TSector), "GetObjectsQuantity")]
        [HarmonyPostfix]
        public static void PostFix_TSector_GetObjectsQuantity(
            TSector __instance,
            int objType,
            ref int __result)
        {
            if (!pluginEnabled.Value)
                return;

            if (!__instance.isStarterSector)
                return;

            if (objType != 7)
                return;

            if (bigAsteroidMode.Value != BigAsteroidMode.Maximum)
                return;

            // Proven former SpawnMaxBigAsteroids behavior.
            __result = __instance.sizeSqr + 3;
        }

        [HarmonyPatch(typeof(TSector), "get_SolarOutput")]
        [HarmonyPostfix]
        public static void PostFix_TSector_GetSolarOutput(
            TSector __instance,
            ref float __result)
        {
            if (!pluginEnabled.Value)
                return;

            if (__instance == null || !__instance.isStarterSector)
                return;

            // User decision: Pitch Black is always hard-locked to zero.
            // Check the actual native sector type, not only the original return value.
            if ((int)__instance.type == (int)global::SectorType.PitchBlack)
            {
                __result = 0.0f;
                return;
            }

            if (overrideSolarOutput.Value)
            {
                __result = ((int)startSectorSolarOutput.Value) / 100f;
                return;
            }

            // Sun Size is visual-only. Preserve the original generated sunSize-derived
            // solar output when only the visual size has been overridden.
            if (overrideSunSize.Value)
            {
                float originalValue;
                if (originalSunSizeBySector.TryGetValue(__instance.Index, out originalValue))
                    __result = originalValue;
            }
        }
    }
}
