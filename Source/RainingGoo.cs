using Verse;

namespace RainingGoo {
    public class WeatherDefExtension : Verse.DefModExtension {
        public RimWorld.ThoughtDef weatherThoughtNudist;
    }

    [Verse.StaticConstructorOnStartup]
    public static class Patch {
        static Patch() {
            // Get the method we want to patch.
            var m = typeof(Verse.AI.Pawn_MindState).GetMethod("MindStateTick");
            // Get the method we want to run after the original.
            var post = typeof(RainingGoo.Patch).GetMethod("PostMindStateTick", 
                System.Reflection.BindingFlags.Static|System.Reflection.BindingFlags.Public);
            // Patch Stuff! The string passed to the Harmony constructor can be anyting, and can 
            // used to identify/remove patches if need be.
            new HarmonyLib.Harmony("Nefastus.RainingGoo").Patch(
                m,
                postfix: new HarmonyLib.HarmonyMethod(post));
        }

        // The special __instance parameter has the original class instance
        // we're extending. This is based on the argument name.
        public static void PostMindStateTick(Verse.AI.Pawn_MindState __instance) {
            var pawn = __instance.pawn;
            if (
                // Verse.Find.TickManager.TicksGame % 123 != 0 
                pawn.IsHashIntervalTick(123)
                || !pawn.Spawned 
                || !pawn.RaceProps.IsFlesh
                || pawn.needs.mood == null)
            {
                return;
            }

            // Is this Pawn is a Nudist and naked? If not, then there's nothing to do.
            // This just checks the "nudity" trait on colonists. TODO add chekc for nudity.
            if (!pawn.story.traits.HasTrait(RimWorld.TraitDefOf.Nudist))
                return;

            // Chek if the current Wheater has our new wheaterThoughtNudist
            var wheater = pawn.Map.weatherManager.CurWeatherLerped;
            if (!wheater.HasModExtension<WeatherDefExtension>())
                return;
            var thought = wheater.GetModExtension<WeatherDefExtension>().weatherThoughtNudist;
            if (thought == null)
                return;
            
            // remove any existing thouth that was applied and apply our nudist thoughts.
            if (wheater.weatherThought != null)
                pawn.needs.mood.thoughts.memories.RemoveMemoriesOfDef(wheater.weatherThought);
            pawn.needs.mood.thoughts.memories.TryGainMemoryFast(thought);


        }

    }


}