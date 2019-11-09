using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace FrontierDevelopments.DeadCryptosleep
{
    public class WorkGiver_HaulCryptosleep : WorkGiver_Scanner
    {
        public override bool ShouldSkip(Pawn pawn, bool forced = false)
        {
            return !PotentialWorkThingsGlobal(pawn).Any();
        }

        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            return pawn.Map.designationManager.allDesignations
                .Where(designation => designation.def == DeadCryptosleepDefOf.Deadcryptosleep_Haul)
                .Select(designation => designation.target.Thing);
        }

        public override Job JobOnThing(Pawn pawn, Thing thing, bool forced = false)
        {
            var pod = ClosestValidTarget(pawn.Map, thing.Position);
            var corpse = thing as Corpse;

            return new Job(DeadCryptosleepDefOf.HaulCorpseToCryptosleepCasket, corpse, pod)
            {
                count = 1
            };
        }

        private static Building_CryptosleepCasket ClosestValidTarget(Map map, IntVec3 position)
        {
            return (Building_CryptosleepCasket)GenClosest.ClosestThingReachable(
                position,
                map,
                ThingRequest.ForGroup(ThingRequestGroup.BuildingArtificial),
                PathEndMode.ClosestTouch,
                TraverseParms.For(TraverseMode.PassDoors),
                validator: thing => thing is Building_CryptosleepCasket);
        }
    }
}