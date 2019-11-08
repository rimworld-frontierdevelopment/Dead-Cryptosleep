using System.Collections.Generic;
using Harmony;
using JetBrains.Annotations;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace FrontierDevelopments.DeadCryptosleep
{
    [HarmonyPatch(typeof(FloatMenuMakerMap), nameof(FloatMenuMakerMap.ChoicesAtFor))]
    public class Harmony_FloatMenuMakerMap
    {
        private static TargetingParameters ForCorpses = new TargetingParameters
        {
            canTargetItems = true,
            canTargetPawns = false,
            canTargetBuildings = false,
            mapObjectTargetsMustBeAutoAttackable = false,
            validator = targetInfo =>
                targetInfo.HasThing && targetInfo.Thing is Corpse
        };

        [CanBeNull]
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

        private static Job CreateHaulJob(Corpse corpse, Building_CryptosleepCasket pod)
        {
            return new Job(DeadCryptosleepDefOf.HaulCorpseToCryptosleepCasket, corpse, pod)
            {
                count = 1
            };
        }

        private static bool Capable(Pawn pawn)
        {
            return !pawn.story.DisabledWorkTypes.Contains(WorkTypeDefOf.Hauling);
        }
        
        static List<FloatMenuOption> Postfix(List<FloatMenuOption> __result, Vector3 clickPos, Pawn pawn)
        {
            foreach (var target in GenUI.TargetsAt(clickPos, ForCorpses))
            {
                var corpse = target.Thing as Corpse;
                if (corpse != null)
                {
                    if (Capable(pawn))
                    {
                        var pod = ClosestValidTarget(corpse.Map, corpse.Position);
                        if (pod != null)
                        {
                            __result.Add(FloatMenuUtility.DecoratePrioritizedTask(
                                new FloatMenuOption(
                                    "FrontierDevelopments.DeadCryptosleep.Menu.Haul".Translate(corpse.Label),
                                    () => pawn.jobs.TryTakeOrderedJob(CreateHaulJob(corpse, pod)),
                                    MenuOptionPriority.Default,
                                    null,
                                    corpse),
                                pawn,
                                target));
                        }
                    }
                    else
                    {
                        __result.Add(new FloatMenuOption("FrontierDevelopments.DeadCryptosleep.Menu.CantHaul".Translate(corpse.Label)
                                                         + " (" + "IncapableOfCapacity".Translate(WorkTypeDefOf.Hauling.gerundLabel) + ")", null));
                    }
                }
            }
            return __result;
        }
    }
}
