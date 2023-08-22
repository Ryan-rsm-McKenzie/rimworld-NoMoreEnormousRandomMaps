#pragma warning disable IDE1006 // Naming Styles

using System;
using HarmonyLib;
using RimWorld;
using Verse;

namespace NoMoreEnormousRandomMaps
{
	[HarmonyPatch(typeof(GetOrGenerateMapUtility))]
	[HarmonyPatch("GetOrGenerateMap")]
	[HarmonyPatch(new Type[] { typeof(int), typeof(WorldObjectDef) })]
	internal class GetOrGenerateMapUtility_GetOrGenerateMap
	{
		public static bool Prefix(ref Map __result, int tile, WorldObjectDef suggestedMapParentDef)
		{
			__result = GetOrGenerateMapUtility.GetOrGenerateMap(
				tile,
				ModMain.Mod.MapBounds,
				suggestedMapParentDef);
			return true;
		}
	}
}
