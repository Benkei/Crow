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

		float m_AspectRatio;
		float m_FieldOfView; // fov in radians
		EProjectionType m_ProjectionType;

		Vector3 m_LastPosition;
		Quaternion m_LastRotation;

		Viewport m_Viewport;

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
			set
			{
				m_FieldOfView = value * MathUtil.Deg2Rad;
				m_UpdateProjectionMatrix = true;
			}
		}
		/// <summary>
		/// aspect ratio (Width / Height)
		/// </summary>
		public float AspectRatio
		{
			get { return m_AspectRatio; }
			set
			{
				m_AspectRatio = value;
				m_CustomAspect = true;
				m_UpdateProjectionMatrix = true;
			}
		}
		//todo mal kucken ob wir es brauchen.
		//public float OrthographicSize { get; set; }
		/// <summary>
		/// 
		/// </summary>
		public float MinimumClipPlane
		{
			get { return m_Viewport.MinDepth; }
			set
			{
				if ( m_Viewport.MinDepth != value )
				{
					m_Viewport.MinDepth = Math.Max ( MIN_NEAR_CLIP_PLANE, value );
					m_UpdateProjectionMatrix = true;
				}
			}
		}
		/// <summary>
		/// 
		/// </summary>
		public float MaximumClipPlane
		{
			get { return m_Viewport.MaxDepth; }
			set
			{
				if ( m_Viewport.MaxDepth != value )
				{
					m_Viewport.MaxDepth = Math.Max ( MIN_FAR_CLIP_PLANE, value );
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
				ProjectionType = EProjectionType.Custom;
				m_UpdateProjectionMatrix = true;
				m_ProjectionMatrix = value;
			}
		}

		public Viewport Viewport
		{
			get { return m_Viewport; }
			set
			{
				m_Viewport = value;
				if ( !m_CustomAspect && m_ProjectionType == EProjectionType.Perspective )
				{
					m_AspectRatio = value.Height != 0 ? (float)value.Width / (float)value.Height : 0f;
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

		public void ResetAspectRatio ()
		{
			if ( m_CustomAspect )
			{
				m_AspectRatio = m_Viewport.Height == 0 ? (float)m_Viewport.Width / (float)m_Viewport.Height : 0f;
				m_CustomAspect = false;
				m_UpdateProjectionMatrix = true;
			}
		}


		public Ray ScreenPointToRay ( Vector3 screenPosition )
		{
			UpdateViewMatrix ();
			UpdateProjectionMatrix ();

			Matrix matrix;
			Ray ray;

			Matrix.Multiply ( ref m_ViewMatrix, ref m_ProjectionMatrix, out matrix );
			matrix.Invert ();

			screenPosition.Z = 0;
			m_Viewport.Unproject ( ref screenPosition, ref matrix, out ray.Position );

			screenPosition.Z = 1;
			m_Viewport.Unproject ( ref screenPosition, ref matrix, out ray.Direction );

			ray.Direction -= ray.Position;
			ray.Direction.Normalize ();

			return ray;
		}
		public Vector3 ScreenToViewportPoint ( Vector3 screenPosition )
		{
			Vector3 view = Transform.Position;
			view.X = (m_Viewport.Width == 0 ? (1f / m_Viewport.Width) : 0f) * screenPosition.X;
			view.Y = (m_Viewport.Height == 0 ? (1f / m_Viewport.Height) : 0f) * screenPosition.Y;
			return view;
		}
		public Vector3 ScreenToWorldPoint ( Vector3 screenPosition )
		{
			UpdateViewMatrix ();
			UpdateProjectionMatrix ();

			Matrix matrix;

			Matrix.Multiply ( ref m_ViewMatrix, ref m_ProjectionMatrix, out matrix );
			matrix.Invert ();

			screenPosition.Z = 0;
			m_Viewport.Unproject ( ref screenPosition, ref matrix, out screenPosition );

			return screenPosition;
		}

		/// <summary>
		/// Viewport coordinates are normalized and relative to the camera. The bottom-left of the camera is (0,0); the top-right is (1,1)
		/// </summary>
		/// <param name="viewPosition"></param>
		/// <returns></returns>
		public Ray ViewportPointToRay ( Vector3 viewPosition )
		{
			viewPosition.X *= m_Viewport.Width;
			viewPosition.Y *= m_Viewport.Height;
			return ScreenPointToRay ( viewPosition );
		}
		public Vector3 ViewportToScreenPoint ( Vector3 viewPosition )
		{
			viewPosition.X *= m_Viewport.Width;
			viewPosition.Y *= m_Viewport.Height;
			return viewPosition;
		}
		public Vector3 ViewportToWorldPoint ( Vector3 viewPosition )
		{
			viewPosition.X *= m_Viewport.Width;
			viewPosition.Y *= m_Viewport.Height;
			return ScreenToWorldPoint ( viewPosition );
		}

		public Vector3 WorldToScreenPoint ( Vector3 worldPosition )
		{
			UpdateViewMatrix ();
			UpdateProjectionMatrix ();

			Matrix matrix;

			Matrix.Multiply ( ref m_ViewMatrix, ref m_ProjectionMatrix, out matrix );
			matrix.Invert ();

			m_Viewport.Project ( ref worldPosition, ref matrix, out worldPosition );

			return worldPosition;
		}
		public Vector3 WorldToViewportPoint ( Vector3 worldPosition )
		{
			return ScreenToViewportPoint ( WorldToScreenPoint ( worldPosition ) );
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
					Matrix.PerspectiveFovLH ( m_FieldOfView, m_AspectRatio, m_Viewport.MinDepth, m_Viewport.MaxDepth, out m_ProjectionMatrix );
				}
				else if ( m_ProjectionType == EProjectionType.Orthographic )
				{
					// Create an orthographic projection matrix.
					Matrix.OrthoLH ( m_Viewport.Width, m_Viewport.Height, m_Viewport.MinDepth, m_Viewport.MaxDepth, out m_ProjectionMatrix );
				}
			}
		}
	}
}
