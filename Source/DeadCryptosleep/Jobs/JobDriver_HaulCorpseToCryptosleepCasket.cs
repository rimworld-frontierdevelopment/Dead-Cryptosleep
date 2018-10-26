using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace FrontierDevelopments.DeadCryptosleep.Jobs
{
    public class JobDriver_HaulCorpseToCryptosleepCasket : JobDriver
    {
        private const TargetIndex CorpseIndex = TargetIndex.A;
        private const TargetIndex CryptosleepCasketIndex = TargetIndex.B;

        private Corpse Corpse => (Corpse) job.GetTarget(CorpseIndex).Thing;
        private Building_CryptosleepCasket CryptosleepCasket => 
            (Building_CryptosleepCasket) job.GetTarget(CryptosleepCasketIndex).Thing;
        
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.GetTarget(CryptosleepCasketIndex), job, 1, -1, null, errorOnFailed)
                   && pawn.Reserve(job.GetTarget(CorpseIndex), job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyedOrNull(CorpseIndex);
            this.FailOnDestroyedOrNull(CryptosleepCasketIndex);
            this.FailOn(() => !CryptosleepCasket.Accepts(Corpse));
            yield return Toils_Goto
                .GotoThing(CorpseIndex, PathEndMode.OnCell)
                .FailOnDestroyedNullOrForbidden(TargetIndex.A)
                .FailOnDespawnedNullOrForbidden(TargetIndex.B)
                .FailOn(() => CryptosleepCasket.HasAnyContents)
                .FailOn(() => !pawn.CanReach(Corpse, PathEndMode.OnCell, Danger.Deadly))
                .FailOnSomeonePhysicallyInteracting(TargetIndex.A);
            yield return Toils_Haul.StartCarryThing(CorpseIndex);
            yield return Toils_Goto.GotoThing(CryptosleepCasketIndex, PathEndMode.InteractionCell);
            yield return Toils_General
                .Wait(500)
                .FailOnCannotTouch(CryptosleepCasketIndex, PathEndMode.InteractionCell)
                .WithProgressBarToilDelay(CryptosleepCasketIndex);
            yield return new Toil
            {
                initAction = () => CryptosleepCasket.TryAcceptThing(Corpse),
                defaultCompleteMode = ToilCompleteMode.Instant
            };
        }
    }
}
