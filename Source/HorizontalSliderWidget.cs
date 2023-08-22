using System;
using System.Text.RegularExpressions;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace NoMoreEnormousRandomMaps
{
	[StaticConstructorOnStartup]
	internal static class WidgetTextures
	{
		public static readonly Texture2D SliderHandle = ContentFinder<Texture2D>.Get("UI/Buttons/SliderHandle", true);

		public static readonly Texture2D SliderRailAtlas = ContentFinder<Texture2D>.Get("UI/Buttons/SliderRail", true);
	}

	internal class HorizontalSliderWidget
	{
		public const float CompactHeight = Widgets.RangeControlCompactHeight;

		public const float IdealHeight = Widgets.RangeControlIdealHeight;

		public LabelConfig DrawLabel = new LabelConfig(true, true, true);

		private const float RangeSliderSize = 8f;

		private const float SliderHandleSize = 12f;

		private static readonly Color s_rangeControlTextColor = new Color(0.6f, 0.6f, 0.6f);

		private string _buffer = "50";

		private bool _dragging = false;

		private FloatRange _range = new FloatRange(0, 100);

		private int _roundTo = 0;

		private Regex _validator = new Regex(@"^\d*$");

		private float _value = 50;

		public FloatRange Range {
			get => this._range;
			set {
				this._range = new FloatRange(
					(float)Math.Round(Math.Min(value.min, value.max), this.RoundTo),
					(float)Math.Round(Math.Max(value.min, value.max), this.RoundTo));
				this.Value = this.Value;
			}
		}

		public int RoundTo {
			get => this._roundTo;
			set {
				this._roundTo = Mathf.Clamp(value, 0, 15);
				this.Value = this.Value;
			}
		}

		public float Value {
			get => this._value;
			set {
				this._value = (float)Math.Round(this.Range.ClampToRange(value), this.RoundTo);
				this._buffer = this.Value.ToString();
			}
		}

		public void Draw(Rect canvas)
		{
			var config = FontConfig.Save();
			this.DrawSilder(canvas);
			this.DrawLabels(canvas);
			config.Restore();
		}

		private static float ConvertRelative(float value, FloatRange from, FloatRange to)
		{
			return Mathf.Lerp(
				to.min,
				to.max,
				Mathf.InverseLerp(
					from.min,
					from.max,
					Mathf.Clamp(
						value,
						from.min,
						from.max)));
		}

		private void DrawLabels(Rect canvas)
		{
			Text.Font = GameFont.Tiny;
			GUI.color = Color.white;

			if (this.DrawLabel.Left) {
				Text.Anchor = TextAnchor.UpperLeft;
				string label = this.Range.min.ToString();
				var size = Text.CalcSize(label);
				Widgets.Label(new Rect(canvas.position, size), label);
			}

			if (this.DrawLabel.Middle) {
				Text.Anchor = TextAnchor.UpperCenter;
				var size = Text.CalcSize(this._buffer);
				var textfield = new Rect(
					canvas.center.x,
					canvas.y,
					0,
					size.y)
					.ExpandedBy((size.x + Text.CalcSize("0").x) / 2, 0);
				this._buffer =
					Widgets.TextField(
						textfield,
						this._buffer,
						this.Range.max.ToString().Length,
						this._validator);
				if (float.TryParse(this._buffer, out float value) && this.Range.Includes(value)) {
					this.Value = value;
				}
			}

			if (this.DrawLabel.Right) {
				Text.Anchor = TextAnchor.UpperRight;
				string label = this.Range.max.ToString();
				var size = Text.CalcSize(label);
				Widgets.Label(
					new Rect(
						canvas.xMax - size.x,
						canvas.y,
						size.x,
						size.y),
					label);
			}
		}

		private void DrawSilder(Rect canvas)
		{
			float rangeSliderSize = (RangeSliderSize / IdealHeight) * canvas.height;
			float sliderHandleSize = (SliderHandleSize / IdealHeight) * canvas.height;
			canvas = canvas.BottomPartPixels(sliderHandleSize);

			switch (Event.current.type) {
				case EventType.MouseDown:
					if (Mouse.IsOver(canvas)) {
						this._dragging = true;
						SoundDefOf.DragSlider.PlayOneShotOnCamera();
						Event.current.Use();
					} else {
						this._dragging = false;
					}
					break;
				case EventType.MouseUp:
					this._dragging = false;
					if (Mouse.IsOver(canvas)) {
						Event.current.Use();
					}
					break;
			}

			var handleRange = new FloatRange(canvas.xMin, canvas.xMax - sliderHandleSize);
			var handle = new Rect(
				ConvertRelative(this.Value, this.Range, handleRange),
				canvas.yMax - sliderHandleSize,
				sliderHandleSize,
				sliderHandleSize);
			var rail = new Rect(
				canvas.x + sliderHandleSize / 2,
				0,
				canvas.width - sliderHandleSize,
				rangeSliderSize)
				.CenteredOnYIn(handle);

			GUI.color = s_rangeControlTextColor;
			Widgets.DrawAtlas(rail, WidgetTextures.SliderRailAtlas);

			GUI.color = Color.white;
			GUI.DrawTexture(handle, WidgetTextures.SliderHandle);

			if (this._dragging && UnityGUIBugsFixer.MouseDrag(0)) {
				this.Value = ConvertRelative(Event.current.mousePosition.x, handleRange, this.Range);
				Event.current.Use();
			}
		}

		public struct LabelConfig
		{
			public bool Left;

			public bool Middle;

			public bool Right;

			public LabelConfig(bool left, bool middle, bool right)
			{
				this.Left = left;
				this.Middle = middle;
				this.Right = right;
			}
		}

		private class FontConfig
		{
			private TextAnchor _anchor = Text.Anchor;

			private Color _color = GUI.color;

			private GameFont _font = Text.Font;

			private FontConfig() { }

			public static FontConfig Save() { return new FontConfig(); }

			public void Restore()
			{
				GUI.color = this._color;
				Text.Font = this._font;
				Text.Anchor = this._anchor;
			}
		}
	}
}
