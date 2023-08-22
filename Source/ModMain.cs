using HarmonyLib;
using UnityEngine;
using Verse;

namespace NoMoreEnormousRandomMaps
{
	public class ModMain : Mod
	{
		public static ModMain Mod = null;

		private readonly Harmony _harmony;

		private readonly Settings _settings;

		public ModMain(ModContentPack content)
			: base(content)
		{
			this._settings = this.GetSettings<Settings>();
			Mod = this;

			this._harmony = new Harmony(this.Content.PackageIdPlayerFacing);
			this._harmony.PatchAll();
		}

		public IntVec3 MapBounds => this._settings.MapBounds;

		public override void DoSettingsWindowContents(Rect inRect)
		{
			this._settings.DoWindowContents(inRect);
		}

		public override string SettingsCategory()
		{
			return this.Content.Name;
		}
	}
}
