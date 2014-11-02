using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace CrowEngine
{
	class Transform
	{
		private Vector3 m_LocalPosition;
		private Vector3 m_LocalScale = Vector3.One;
		private Quaternion m_LocalRotation = Quaternion.Identity;

		public Matrix m_LocalMatrix = Matrix.Identity;

		public Transform ()
		{
		}

		public Vector3 Position
		{
			get { return m_LocalPosition; }
			set { m_LocalPosition = value; UpdateMatrix (); }
		}

		public Vector3 Scale
		{
			get { return m_LocalScale; }
			set { m_LocalScale = value; UpdateMatrix (); }
		}

		public Quaternion Rotation
		{
			get { return m_LocalRotation; }
			set { m_LocalRotation = value; UpdateMatrix (); }
		}

		private void UpdateMatrix ()
		{
			Matrix3x3 m1, m2;
			Matrix3x3.Scaling ( ref m_LocalScale, out m1 );
			Matrix3x3.RotationQuaternion ( ref m_LocalRotation, out m2 );

			Matrix3x3.Multiply ( ref m1, ref m2, out m1 );

			m_LocalMatrix = (Matrix)m1;
			m_LocalMatrix.TranslationVector = m_LocalPosition;
		}
	}
}
