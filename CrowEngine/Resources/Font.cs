using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpFont;

namespace CrowEngine.Resources
{
	class Font
	{
		private Bitmap m_Bmp;
		private System.Drawing.Graphics m_Gdi;
		private StringFormat m_StringFormat;

		public void Muh ()
		{
			System.Drawing.Font f;
			//m_Gdi.MeasureString()
		}


		static public int MeasureDisplayStringWidth ( System.Drawing.Graphics graphics, string text, System.Drawing.Font font )
		{
			StringFormat format = new StringFormat ();
			RectangleF rect = new RectangleF ( 0, 0, 1000, 1000 );
			CharacterRange[] ranges = { new System.Drawing.CharacterRange ( 0, text.Length ) };
			Region[] regions;

			format.SetMeasurableCharacterRanges ( ranges );

			regions = graphics.MeasureCharacterRanges ( text, font, rect, format );
			rect = regions[0].GetBounds ( graphics );

			return (int)(rect.Right + 1.0f);
		}
	}


	class FontManager
	{
		static Library m_Library = new Library ();

	}
}
