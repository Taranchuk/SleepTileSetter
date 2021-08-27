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
	[StaticConstructorOnStartup]
	public class Dialog_SetTilesForSleeping : Window
	{
		public static readonly Texture2D BGTex = ContentFinder<Texture2D>.Get("UI/Widgets/DesButBG");
		public static readonly Texture2D GrayTextBG = ContentFinder<Texture2D>.Get("UI/Overlays/GrayTextBG");
		public static readonly Color LowLightBgColor = new Color(0.8f, 0.8f, 0.7f, 0.5f);

		private Vector2 scrollPosition;
		public override Vector2 InitialSize => new Vector2(620f, 500f);

		private CompSleepingTileSetter comp;
		private CompAssignableToPawn compAssignable;
		public Building_Bed bed;
		public Dialog_SetTilesForSleeping(CompSleepingTileSetter comp)
		{
			doCloseButton = true;
			doCloseX = true;
			//closeOnClickedOutside = true;
			//absorbInputAroundWindow = true;
			this.comp = comp;
			this.compAssignable = comp.parent.TryGetComp<CompAssignableToPawn>();
			this.bed = comp.parent as Building_Bed;

		}

		private int GetPawnRow()
        {
			if (bed.def.building.bed_humanlike)
            {
				return 1;
            }
			else
            {
				return bed.def.size.z;
			}
        }
		private int GetPawnSlotCount()
		{
			return bed.def.size.x;
		}

		[TweakValue("0AS", 0, 300)] public static float buttonSize = 100f;
		[TweakValue("0AS", 0, 300)] public static float buttonYMargin = 25f;
		[TweakValue("0AS", 0, 300)] public static float buttonXMargin = 25f;
		[TweakValue("0AS", 0, 300)] public static float xMargin = 10;
		[TweakValue("0AS", 0, 300)] public static float yMargin = 10;

		public override void DoWindowContents(Rect inRect)
		{
			Text.Font = GameFont.Small;
			Rect outRect = new Rect(inRect);
			outRect.x += xMargin;
			outRect.y += yMargin;
			outRect.yMax -= 70f;
			outRect.width -= 16f;

			var pawnRowCount = GetPawnRow();
			var pawnColumnCount = GetPawnSlotCount();
			var width = xMargin + (pawnColumnCount * (buttonSize + buttonXMargin)) - 16f;
			var height = yMargin + (pawnRowCount * (buttonSize + buttonYMargin));
			Rect viewRect = new Rect(outRect.x, outRect.y, width, height);
			Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);
			try
			{
				for (var i = 0; i < pawnRowCount; i++)
                {
					for (var j = 0; j < pawnColumnCount; j++)
                    {
						Rect rect = new Rect(viewRect.x + (j * (buttonSize + buttonXMargin)), viewRect.y + (i * (buttonSize + buttonYMargin)), buttonSize, buttonSize);
						IntVec3 matchingCell = new IntVec3(i, 0, j);
						bool allowedCell = comp.IsAllowedCell(matchingCell);
						if (allowedCell)
                        {
							GUI.DrawTexture(rect, BGTex);
							var pawn = comp.GetAssignedPawnFor(matchingCell);
							if (pawn != null)
							{
								GUI.DrawTexture(rect, PortraitsCache.Get(pawn, new Vector2(rect.width, rect.height), Rot4.South));
							}
						}
						else
                        {
							GUI.DrawTexture(rect, BGTex, ScaleMode.ScaleAndCrop, true, 1f, LowLightBgColor, 1f, 1f); ;
                        }

						if (allowedCell && rect.IsLeftClicked())
                        {
							var floatList = new List<FloatMenuOption>();
							foreach (var candidate in compAssignable.AssigningCandidates)
                            {
								floatList.Add(new FloatMenuOption(candidate.LabelShortCap, delegate
								{
									comp.assignedTiles.RemoveAll(x => x.Value == candidate);
									comp.assignedTiles[matchingCell] = candidate;
									if (candidate.ownership.OwnedBed != bed)
                                    {
										candidate.ownership.UnclaimBed();
									}
									if (!compAssignable.AssignedPawns.Contains(candidate))
                                    {
										compAssignable.TryAssignPawn(candidate);
									}
								}, PortraitsCache.Get(candidate, new Vector2(50, 50), Rot4.South).MakeReadableTextureInstance(), Color.white));
							}
							Find.WindowStack.Add(new FloatMenu(floatList));
                        }
					}
				}
			}
			finally
			{
				Widgets.EndScrollView();
			}
		}
	}
}
