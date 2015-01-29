using System;
using CrowEngine.Mathematics;

namespace CrowEngine.Resources
{
	public class Sprite
	{
		internal Sprite (
			Texture2D texture,
			Rectangle rect,
			Vector2 pivot,
			Rectangle border,
			float pixelsPerUnit )
		{
			Texture = texture;
			Rect = rect;
			Pivot = pivot;
			Border = border;
			PixelsPerUnit = pixelsPerUnit;
		}

		public Texture2D Texture { get; private set; }

		public Rectangle Rect { get; private set; }

		public Vector2 Pivot { get; private set; }

		public Rectangle Border { get; private set; }

		public float PixelsPerUnit { get; private set; }

		public RectangleF GetOuterUV ()
		{
			float u = 1f / Texture.Width ( 0 );
			float v = 1f / Texture.Height ( 0 );
			RectangleF uv = new RectangleF ();
			uv.Minimum.X = u * Rect.Left;
			uv.Minimum.Y = u * Rect.Right;
			uv.Maximum.X = v * Rect.Top;
			uv.Maximum.Y = v * Rect.Bottom;
			return uv;
		}

		public RectangleF GetInnerUV ()
		{
			float u = 1f / Texture.Width ( 0 );
			float v = 1f / Texture.Height ( 0 );
			RectangleF uv = new RectangleF ();
			uv.Minimum.X = u * (Rect.Left - Border.Left);
			uv.Minimum.Y = u * (Rect.Right - Border.Right);
			uv.Maximum.X = v * (Rect.Top - Border.Top);
			uv.Maximum.Y = v * (Rect.Bottom - Border.Bottom);
			return uv;
		}
	}
}