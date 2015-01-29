using System;
using CrowEngine.Mathematics;

namespace CrowEngine.Components
{
	public enum EClearFlags : byte
	{
		Skybox = 1,
		SolidColor,
		Depth,
		Nothing
	}
	public enum EProjectionType : byte
	{
		Perspective,
		Orthographic,
		Custom
	}

	public class Camera : Behavior
	{
		public const float MIN_NEAR_CLIP_PLANE = 0.01f;
		public const float MIN_FAR_CLIP_PLANE = 0.02f;

		bool m_UpdateProjectionMatrix = true;
		bool m_CustomAspect;

		float m_Aspect;
		float m_FieldOfView; // fov in radians
		float m_NearClipPlane;
		float m_FarClipPlane;
		EProjectionType m_ProjectionType;

		Vector3 m_LastPosition;
		Quaternion m_LastRotation;

		Rectangle m_PixelScreenSize;

		Matrix m_ViewMatrix;
		Matrix m_ProjectionMatrix;



		public EProjectionType ProjectionType
		{
			get { return m_ProjectionType; }
			set
			{
				m_ProjectionType = value;
				m_UpdateProjectionMatrix = true;
			}
		}
		public bool IsPerspective
		{
			get { return m_ProjectionType == EProjectionType.Perspective; }
		}
		public bool IsOrthographic
		{
			get { return m_ProjectionType == EProjectionType.Orthographic; }
		}

		/// <summary>
		/// field of view in degrees
		/// </summary>
		public float FieldOfView
		{
			get { return m_FieldOfView * MathUtil.Rad2Deg; }
			set { m_FieldOfView = value * MathUtil.Deg2Rad; }
		}
		/// <summary>
		/// aspect ratio (Width / Height)
		/// </summary>
		public float Aspect
		{
			get { return m_Aspect; }
			set
			{
				m_Aspect = value;
				m_CustomAspect = true;
			}
		}
		//todo mal kucken ob wir es brauchen.
		//public float OrthographicSize { get; set; }
		/// <summary>
		/// 
		/// </summary>
		public float NearClipPlane
		{
			get { return m_NearClipPlane; }
			set
			{
				if ( m_NearClipPlane != value )
				{
					m_NearClipPlane = Math.Max ( MIN_NEAR_CLIP_PLANE, value );
					m_UpdateProjectionMatrix = true;
				}
			}
		}
		/// <summary>
		/// 
		/// </summary>
		public float FarClipPlane
		{
			get { return m_FarClipPlane; }
			set
			{
				if ( m_FarClipPlane != value )
				{
					m_FarClipPlane = Math.Max ( MIN_FAR_CLIP_PLANE, value );
					m_UpdateProjectionMatrix = true;
				}
			}
		}
		/// <summary>
		/// camera rendering order (low value render first)
		/// </summary>
		public int Depth
		{
			get;
			set;
		}


		public EClearFlags ClearFlags
		{
			get;
			set;
		}

		public Color4 BackgroundColor
		{
			get;
			set;
		}
		/// <summary>
		/// camera view matrix
		/// </summary>
		public Matrix ViewMatrix
		{
			get { UpdateViewMatrix (); return m_ViewMatrix; }
		}
		/// <summary>
		/// camera projection matrix
		/// </summary>
		public Matrix ProjectionMatrix
		{
			get { UpdateProjectionMatrix (); return m_ProjectionMatrix; }
			set
			{
				m_ProjectionMatrix = value;
				ProjectionType = EProjectionType.Custom;
				m_UpdateProjectionMatrix = true;
			}
		}

		public Rectangle PixelScreenSize
		{
			get { return m_PixelScreenSize; }
			set
			{
				m_PixelScreenSize = value;
				if ( !m_CustomAspect && m_ProjectionType == EProjectionType.Perspective )
				{
					Aspect = (float)value.Width / (float)value.Height;
				}
				m_UpdateProjectionMatrix = true;
			}
		}

		public int CullingMask
		{
			get;
			set;
		}


		public Camera ()
		{
			CullingMask = ~0;
		}

		public void ResetProjectionMatrix ( EProjectionType target )
		{
			m_ProjectionType = target;
			UpdateProjectionMatrix ();
		}

		public void ResetAspect ()
		{
			if ( m_CustomAspect )
			{
				m_CustomAspect = false;
				Aspect = (float)m_PixelScreenSize.Width / (float)m_PixelScreenSize.Height;
			}
		}

		void UpdateViewMatrix ()
		{
			if ( !m_LastPosition.Equals ( Transform.Position ) || !m_LastRotation.Equals ( Transform.Rotation ) )
			{
				m_LastPosition = Transform.Position;
				m_LastRotation = Transform.Rotation;

				// Setup where the camera is looking by default.
				Vector3 forward = Vector3.UnitZ;
				Vector3.Transform ( ref forward, ref m_LastRotation, out forward );

				// Setup the vector that points upwards.
				Vector3 up = Vector3.UnitY;
				Vector3.Transform ( ref up, ref m_LastRotation, out up );

				// Translate the rotated camera position to the location of the viewer.
				forward = m_LastPosition + forward;

				// Finally create the view matrix from the three updated vectors.
				Matrix.LookAtLH ( ref m_LastPosition, ref forward, ref up, out m_ViewMatrix );
			}
		}

		void UpdateProjectionMatrix ()
		{
			if ( m_UpdateProjectionMatrix && m_ProjectionType != EProjectionType.Custom )
			{
				m_UpdateProjectionMatrix = false;
				if ( m_ProjectionType == EProjectionType.Perspective )
				{
					// Create an perspective projection matrix.
					Matrix.PerspectiveFovLH ( m_FieldOfView, Aspect, m_NearClipPlane, m_FarClipPlane, out m_ProjectionMatrix );
				}
				else if ( m_ProjectionType == EProjectionType.Orthographic )
				{
					// Create an orthographic projection matrix.
					Matrix.OrthoLH ( m_PixelScreenSize.Width, m_PixelScreenSize.Height, m_NearClipPlane, m_FarClipPlane, out m_ProjectionMatrix );
				}
			}
		}
	}
}
