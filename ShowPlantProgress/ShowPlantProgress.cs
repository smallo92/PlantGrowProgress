using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace ShowPlantProgress
{
    [BepInPlugin("smallo.mods.showplantprogress", "Show Plant Progress", "1.3.0")]
    [HarmonyPatch]
    class ShowPlantProgressPlugin : BaseUnityPlugin
    {
        private static ConfigEntry<bool> enableMod;
        private static ConfigEntry<int> amountofDecimals;

        private static readonly List<string> bushList = new List<string> { "RaspberryBush(Clone)", "BlueberryBush(Clone)", "CloudberryBush(Clone)" };

        void Awake()
        {
            enableMod = Config.Bind("1 - Global", "Enable Mod", true, "Enable or disable this mod");
            amountofDecimals = Config.Bind("2 - General", "Amount of Decimal Places", 2, "The amount of decimal places to show");

            if (!enableMod.Value) { return; }

            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), null);
        }

        private static string GetColour(double percentage)
        {
            string colour = "red";
            if (percentage >= 25 && percentage <= 49) colour = "orange";
            if (percentage >= 50 && percentage <= 74) colour = "yellow";
            if (percentage >= 75 && percentage <= 100) colour = "lime";

            return colour;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Plant), "GetHoverText")]
        public static string PlantGetHoverText_Patch(string __result, Plant __instance)
        {
            if (__instance == null) return __result;

            double percentage = Mathf.Floor((float)__instance.TimeSincePlanted() / (float)__instance.GetGrowTime() * 100);
            string colour = GetColour(percentage);
            string growPercentage = $"<color={colour}>{decimal.Round((decimal)percentage, amountofDecimals.Value, MidpointRounding.AwayFromZero)}%</color>";

            return __result.Replace(" )", $", {growPercentage} )");
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Pickable), "GetHoverText")]
        public static string BerryBushPickable_Patch(string __result, Pickable __instance)
        {
            if (!bushList.Contains(__instance.name)) return __result;

            DateTime startTime = new DateTime(__instance.m_nview.GetZDO().GetLong("picked_time"));
            double percentage = (ZNet.instance.GetTime() - startTime).TotalMinutes / __instance.m_respawnTimeMinutes * 100;
            if (percentage > 99.99f) return __result;

            string colour = GetColour(percentage);
            string growPercentage = $"<color={colour}>{decimal.Round((decimal)percentage, amountofDecimals.Value, MidpointRounding.AwayFromZero)}%</color>";

            return __result + $"{Localization.instance.Localize(__instance.GetHoverName())} ( {growPercentage} )";
        }

        // Berry bush time reset fix thanks to `3d_printing_dm` from https://old.reddit.com/r/valheim/comments/lzcxr3/bug_in_berry_bush_pick_time_code/
        [HarmonyPatch(typeof(Pickable), "SetPicked")]
        public static class FixPickableTime
        {
            public class PickState
            {
                public long picked_time;
                public bool picked;
            }

            [HarmonyPrefix]
            public static void Prefix(bool picked, ZNetView ___m_nview, bool ___m_picked, ref PickState __state)
            {
                __state = new PickState();
                __state.picked_time = ___m_nview.GetZDO().GetLong("picked_time", 0L);
                __state.picked = ___m_picked;
            }

            [HarmonyPostfix]
            public static void Postfix(bool picked, ZNetView ___m_nview, bool ___m_picked, ref PickState __state)
            {
                if (__state.picked == ___m_picked)
                {
                    ___m_nview.GetZDO().Set("picked_time", __state.picked_time);
                }
            }
        }
    }
}