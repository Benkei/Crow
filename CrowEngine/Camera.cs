using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace CrowEngine
{
	class Camera
	{
		public Vector3 Position;
		public Quaternion Rotation = Quaternion.Identity;

		public float FieldOfView;
		public float Near;
		public float Far;
		public Matrix WorldMatrix;

		public Camera ()
		{
			//Matrix.PerspectiveFovLH( FieldOfView,  )
			//Quaternion.RotationYawPitchRoll()
		}
	}
}
