using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace SleepTileSetter
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            var harmony = new Harmony("SleepTileSetter.Mod");
            harmony.PatchAll();
            foreach (var thingDef in DefDatabase<ThingDef>.AllDefs)
            {
                if (!thingDef.HasComp(typeof(CompSleepingTileSetter)) && (thingDef.size.x > 1 || thingDef.size.z > 1) 
                    && typeof(Building_Bed).IsAssignableFrom(thingDef.thingClass) && thingDef.HasAssignableCompFrom(typeof(CompAssignableToPawn)))
                {
                    thingDef.comps.Add(new CompProperties_SleepingTileSetter());
                }
            }
        }
        public static Dictionary<Building_Bed, CompSleepingTileSetter> cachedComps = new Dictionary<Building_Bed, CompSleepingTileSetter>();
        public static bool TryGetCompSleepingTileSetter(this Building_Bed bed, out CompSleepingTileSetter comp)
        {
            if (!cachedComps.TryGetValue(bed, out comp))
            {
                cachedComps[bed] = comp = bed.TryGetComp<CompSleepingTileSetter>();
            }
            return comp != null;
        }
    }

    [HarmonyPatch(typeof(CompAssignableToPawn), "ForceRemovePawn")]
    public class ForceRemovePawn_Patch
    {
        [HarmonyPriority(Priority.First)]
        private static void Prefix(CompAssignableToPawn __instance, Pawn pawn)
        {
            if (__instance.parent is Building_Bed bed && bed.TryGetCompSleepingTileSetter(out var comp) && comp.GetAssignedTileFor(pawn).IsValid)
            {
                comp.assignedTiles.RemoveAll(x => x.Value == pawn);
            }
        }
    }

    [HarmonyPatch(typeof(Building_Bed), "GetCurOccupantSlotIndex")]
    public class GetCurOccupantSlotIndex_Patch
    {
        [HarmonyPriority(Priority.First)]
        private static bool Prefix(ref int __result, Building_Bed __instance, Pawn curOccupant)
        {
            if (__instance.TryGetCompSleepingTileSetter(out var comp) && comp.GetAssignedTileFor(curOccupant).IsValid)
            {
                __result = comp.compAssignableToPawn.assignedPawns.IndexOf(curOccupant);
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(RestUtility), "GetBedSleepingSlotPosFor")]
    public class GetBedSleepingSlotPosFor_Patch
    {
        private static bool Prefix(ref IntVec3 __result, Pawn pawn, Building_Bed bed)
        {
            if (bed.TryGetCompSleepingTileSetter(out var comp))
            {
                var cell = comp.GetAssignedTileFor(pawn);
                if (cell.IsValid)
                {
                    __result = cell;
                    return false;
                }
            }
            return true;
        }
    }
}
