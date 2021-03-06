using BepInEx;
using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace ShowPlantProgress
{
    [BepInPlugin("smallo.mods.showplantprogress", "Show Plant Progress", "1.2.1")]
    [HarmonyPatch]
    class ShowPlantProgressPlugin : BaseUnityPlugin
    {

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