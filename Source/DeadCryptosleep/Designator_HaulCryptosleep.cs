using System;
using System.Collections.Generic;
using System.Linq;
using Harmony;
using RimWorld;
using Verse;

namespace FrontierDevelopments.DeadCryptosleep
{
    public class Designator_HaulCryptosleep : Designator
    {
        public Designator_HaulCryptosleep()
        {
            defaultLabel = "FrontierDevelopments.DeadCryptosleep.Designator.Label".Translate();
            defaultDesc = "FrontierDevelopments.DeadCryptosleep.Designator.Desc".Translate();
            icon = DeadCryptosleepDefOf.CryptosleepCasket.uiIcon;
            iconAngle = DeadCryptosleepDefOf.CryptosleepCasket.uiIconAngle;
            iconOffset = DeadCryptosleepDefOf.CryptosleepCasket.uiIconOffset;
            iconProportions = DeadCryptosleepDefOf.CryptosleepCasket.graphicData.drawSize.RotatedBy(DeadCryptosleepDefOf.CryptosleepCasket.defaultPlacingRot);
            iconDrawScale = GenUI.IconDrawScale(DeadCryptosleepDefOf.CryptosleepCasket);
        }

        public override int DraggableDimensions => 2;
        
        protected override DesignationDef Designation => DeadCryptosleepDefOf.Deadcryptosleep_Haul;

        private bool PodsAreAvailable()
        {
            var designated = Map.designationManager.allDesignations
                .Count(designation => designation.def == DeadCryptosleepDefOf.Deadcryptosleep_Haul);

            var availableCaskets = Map.listerBuildings.allBuildingsColonist
                .OfType<Building_CryptosleepCasket>()
                .Count(casket => !casket.HasAnyContents);

            return availableCaskets >= designated;
        }

        public override AcceptanceReport CanDesignateCell(IntVec3 cell)
        {
            if (!cell.InBounds(Map) || cell.Fogged(Map))
                return false;

            try
            {
                CanDesignateThing(
                    Map.thingGrid.ThingsListAt(cell)
                        .OfType<Corpse>()
                        .First());
                return true;
            }
            catch (InvalidOperationException)
            {
                return "FrontierDevelopments.DeadCryptosleep.Designator.NoCorpses".Translate();
            }
        }

        public override void DesignateSingleCell(IntVec3 cell)
        {
            cell.GetThingList(Map).OfType<Corpse>().ToList().ForEach(DesignateThing);
        }

        public override AcceptanceReport CanDesignateThing(Thing thing)
        {
            switch (thing)
            {
                case Corpse _:
                    return true;
            }

            return "FrontierDevelopments.DeadCryptosleep.Designator.MustBeCorpse".Translate();
        }

        public override void DesignateThing(Thing thing)
        {
            if (CanDesignateThing(thing).Accepted)
            {
                Map.designationManager.AddDesignation(new Designation((LocalTargetInfo) thing, Designation));
            }
        }
    }

    [HarmonyPatch(typeof(ReverseDesignatorDatabase), "InitDesignators")]
    static class Patch_ReverseDesignatorDatabase
    {
        [HarmonyPostfix]
        private static void AddDesignator(List<Designator> ___desList)
        {
            var designator = new Designator_HaulCryptosleep();
            if (Current.Game.Rules.DesignatorAllowed(designator))
            {
                ___desList.Add(designator);
            }
        }
    }
}