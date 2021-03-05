using BepInEx;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace ShowPlantProgress
{
    [BepInPlugin("smallo.mods.showplantprogress", "Show Plant Progress", "1.2.1")]
    [HarmonyPatch]
    class ShowPlantProgressPlugin : BaseUnityPlugin
    {
        private static readonly List<string> bushList = new List<string> { "RaspberryBush(Clone)", "BlueberryBush(Clone)", "CloudberryBush(Clone)" };

        void Awake() => Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), null);

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Plant), "GetHoverText")]
        public static string PlantGetHoverText_Patch(string __result, Plant __instance)
        {
            if (__instance == null) return __result;
            int percentage = (int)Mathf.Floor((float)__instance.TimeSincePlanted() / (float)__instance.GetGrowTime() * 100);
            string colour = GetColour(percentage);
            string growPercentage = $"<color={colour}>{percentage}%</color>";

            return __result.Replace(" )", $", {growPercentage} )");
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Pickable), "GetHoverText")]
        public static string GameObjectHoverText_Patch(string __result, Pickable __instance)
        {
            if (!bushList.Contains(__instance.name)) return __result;

            DateTime startTime = new DateTime(__instance.m_nview.GetZDO().GetLong("picked_time"));
            double percentage = (ZNet.instance.GetTime() - startTime).TotalMinutes / __instance.m_respawnTimeMinutes * 100;
            if (percentage > 99.99f) return __result;

            string colour = GetColour(Math.Round(percentage, 2));
            string growPercentage = $"<color={colour}>{percentage:0.00}%</color>";

            return __result + $"{__instance.name.Replace("(Clone)", "").Replace("Bush", " Bush")} ( {growPercentage} )";
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Plant), "Awake")]
        public static void PlantBiome_Patch(Plant __instance)
        {
            __instance.m_biome = Heightmap.Biome.BiomesMax;
        }

        public static string GetColour(double percentage)
        {
            string colour = "red";
            if (percentage >= 25 && percentage <= 49) colour = "orange";
            if (percentage >= 50 && percentage <= 74) colour = "yellow";
            if (percentage >= 75 && percentage <= 100) colour = "lime";

            return colour;
        }
    }
}