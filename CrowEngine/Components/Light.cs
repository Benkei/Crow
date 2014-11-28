using System;
using CrowEngine.Mathematics;

namespace CrowEngine.Components
{
	public enum ELightType : byte
	{
		Spot, Directional, Point
	}
	public enum ELightShadows : byte
	{
		None, Hard, Soft
	}
	public enum ELightRenderMode : byte
	{
		Auto, ForcePixel, ForceVertex
	}

	public class Light : Behavior
	{
		Vector3 mDirection;
		Color4 mDiffuseColor;

		public Vector3 Direction
		{
			get { return mDirection; }
			set { mDirection = value;/* mDirection.Normalize ();*/ }
		}
		public Color4 DiffuseColor
		{
			get { return mDiffuseColor; }
			set { mDiffuseColor = value; }
		}
		public Color4 AmbientColor { get; set; }
		public Color4 SpecularColor { get; set; }
		public float SpecularPower { get; set; }



		public ELightType Type { get; set; }
		public ELightRenderMode RenderMode { get; set; }
		public int CullingMask { get; set; }

		public Color4 Color { get; set; }
		public float Intensity { get; set; }
		public float Range { get; set; }
		public float SpotAngle { get; set; }

		public ELightShadows Shadows { get; set; }
		public float ShadowStrength { get; set; }
		public float ShadowBias { get; set; }
		public float ShadowSoftness { get; set; }
		public float ShadowSoftnessFade { get; set; }

		//public Texture Cookie { get; set; }
		//public Flare Flare { get; set; }

	}
}
