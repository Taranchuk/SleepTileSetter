using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace SleepTileSetter
{
    public class CompProperties_SleepingTileSetter : CompProperties
    {
        public List<IntVec3> disallowedCells;
        public CompProperties_SleepingTileSetter()
        {
            this.compClass = typeof(CompSleepingTileSetter); 
        }
    }

    public class CompSleepingTileSetter : ThingComp
    {
        public Dictionary<IntVec3, Pawn> assignedTiles = new Dictionary<IntVec3, Pawn>();

        public CompAssignableToPawn compAssignableToPawn;
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            this.compAssignableToPawn = this.parent.TryGetComp<CompAssignableToPawn>();
        }
        public CompProperties_SleepingTileSetter Props => base.props as CompProperties_SleepingTileSetter;

        public IntVec3 GetAssignedTileFor(Pawn pawn)
        {
            var intVec = assignedTiles.FirstOrDefault(x => x.Value == pawn);
            if (intVec.Key.IsValid)
            {
                var cellRect = this.parent.OccupiedRect();
                var topLeft = new IntVec3(cellRect.minX, 0, cellRect.maxZ);
                var result = new IntVec3(topLeft.x + intVec.Key.z, 0, topLeft.z - intVec.Key.x);
                return result;
            }
            return IntVec3.Invalid;
        }

        public Pawn GetAssignedPawnFor(IntVec3 cell)
        {
            if (assignedTiles.TryGetValue(cell, out var pawn))
            {
                return pawn;
            }
            return null;
        }

        public bool IsAllowedCell(IntVec3 cell)
        {
            return Props.disallowedCells is null || !Props.disallowedCells.Contains(cell);
        }
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (var g in base.CompGetGizmosExtra())
            {
                yield return g;
            }
            if (compAssignableToPawn.ShouldShowAssignmentGizmo())
            {
                Command_Action command_Action = new Command_Action();
                command_Action.defaultLabel = "STS.SetSleepingTiles".Translate();
                command_Action.defaultDesc = "STS.SetSleepingTilesDesc".Translate();
                command_Action.icon = ContentFinder<Texture2D>.Get("UI/Commands/AssignOwner");
                command_Action.action = delegate
                {
                    Find.WindowStack.Add(new Dialog_SetTilesForSleeping(this));
                };
                command_Action.hotKey = KeyBindingDefOf.Misc4;
                yield return command_Action;
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Collections.Look(ref assignedTiles, "assignedTiles", LookMode.Value, LookMode.Reference, ref intKeys, ref pawnValues);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (assignedTiles == null)
                {
                    assignedTiles = new Dictionary<IntVec3, Pawn>();
                }
            }
        }

        private List<IntVec3> intKeys;
        private List<Pawn> pawnValues;
    }
}
