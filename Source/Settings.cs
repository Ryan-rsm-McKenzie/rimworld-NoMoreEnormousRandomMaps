using System;
using System.Linq;
using UnityEngine;
using Verse;

namespace NoMoreEnormousRandomMaps
{
	internal static class RectExt
	{
		public static Rect Allocate(this ref Rect rect, float height)
		{
			rect.SplitHorizontallyWithMargin(out var top, out var bottom, out _, topHeight: height);
			rect = bottom;
			return top;
		}

		public static void Pad(this ref Rect rect, float value)
		{
			rect.y += value;
			rect.height -= value;
		}
	}

	internal class Settings : ModSettings
	{
		private static readonly IntVec2 s_defaultBounds = new IntVec2(200, 200);

		private IntVec2 _bounds = s_defaultBounds;

		private Window _window = null;

		public IntVec3 MapBounds => new IntVec3(this._bounds.x, 1, this._bounds.z);

		public void DoWindowContents(Rect canvas)
		{
			if (this._window == null) {
				// settings menu opened
				this._window = new Window(this._bounds);
			}
			this._window.Draw(canvas);
		}

		public override void ExposeData()
		{
			if (Scribe.mode == LoadSaveMode.Saving) {
				// settings menu closed
				this._bounds = new IntVec2(this._window.X, this._window.Z);
				this._window = null;
			}

			Scribe_Values.Look(ref this._bounds.x, "width", defaultValue: s_defaultBounds.x);
			Scribe_Values.Look(ref this._bounds.z, "height", defaultValue: s_defaultBounds.z);
		}

		private class Window
		{
			private const GameFont Font = GameFont.Small;

			private readonly BoundsWidget[] _bounds;

			private readonly float _labelMax;

			public Window(IntVec2 bounds)
			{
				Text.Font = Font;

				this._bounds = new BoundsWidget[] {
					new BoundsWidget(bounds.x, "Width:"),
					new BoundsWidget(bounds.z, "Height:"),
				};

				this._labelMax = this._bounds
					.Select((bound) => Text.CalcSize(bound.Label).x)
					.Max();
			}

			public int X => this._bounds[0].Value;

			public int Z => this._bounds[1].Value;

			public void Draw(Rect canvas)
			{
				Text.Font = Font;
				Text.Anchor = TextAnchor.UpperLeft;
				GUI.color = Color.white;

				foreach (var bound in this._bounds) {
					var line = canvas.Allocate(Math.Max(Widgets.RangeControlIdealHeight, Text.LineHeight));
					line.SplitVerticallyWithMargin(
						out var left,
						out var input,
						out _,
						compressibleMargin: GenUI.GapSmall,
						leftWidth: this._labelMax);
					var label = new Rect(left.x, 0, left.width, Text.LineHeight).CenteredOnYIn(left);
					bound.Draw(label, input);
					canvas.Pad(GenUI.GapTiny);
				}
			}

			private class BoundsWidget
			{
				public string Label;

				private HorizontalSliderWidget _slider = new HorizontalSliderWidget() {
					Range = new FloatRange(125, 1000)
				};

				public BoundsWidget(int value, string label)
				{
					this.Label = label;
					this._slider.Value = value;
				}

				public int Value => (int)this._slider.Value;

				public void Draw(Rect label, Rect input)
				{
					Widgets.Label(label, this.Label);
					this._slider.Draw(input);
				}
			}
		}
	}
}
