using System;
using System.Collections;
using System.Collections.Generic;
using SharpDX;

namespace CrowEngine
{
	public class Transform : Object, ICollection<Transform>
	{
		private Transform m_Parent;
		private List<Transform> m_Children;
		private HashSet<Transform> m_ChildrenToUpdate;

		private Matrix m_WorldMatrix = Matrix.Identity;
		private Vector3 m_WorldScale = Vector3.One;
		private Vector3 m_WorldPosition;
		private Quaternion m_WorldRotation = Quaternion.Identity;

		private Matrix m_LocalMatrix = Matrix.Identity;
		private Vector3 m_LocalScale = Vector3.One;
		private Vector3 m_LocalPosition;
		private Quaternion m_LocalRotation = Quaternion.Identity;

		//SceneManager		m_SceneManager;

		/// <summary>
		/// need a world transform update
		/// </summary>
		private bool m_NeedParentUpdate;

		private bool m_NeedChildUpdate;
		private bool m_NeedMatrixUpdate;
		private bool m_NeedLocalMatrixUpdate;
		private bool m_ParentNotified;

		public GameObject GameObject
		{
			get;
			private set;
		}

		public Transform Root
		{
			get
			{
				Transform last = m_Parent;
				Transform current = m_Parent;
				while ( current != null )
				{
					last = current;
					current = current.m_Parent;
				}
				return last;
			}
		}

		public Transform Parent
		{
			get { return m_Parent; }
			set
			{
				if ( m_Parent != value )
				{
					if ( value != null )
					{
						value.AddChild ( this );
					}
					else if ( m_Parent != null )
					{
						m_Parent.RemoveChild ( this );
					}
				}
			}
		}

		public int Count
		{
			get { return (m_Children != null) ? m_Children.Count : 0; }
		}

		public override string Name
		{
			get;
			set;
		}

		//public SceneManager SceneManager
		//{
		//	get { CheckDestroyed (); return m_SceneManager; }
		//}

		bool ICollection<Transform>.IsReadOnly
		{
			get { return false; }
		}

		/// <summary>
		/// get/set the world matrix
		/// </summary>
		public Matrix WorldMatrix
		{
			get
			{
				if ( m_NeedParentUpdate ) { UpdateFromParent (); }
				if ( m_NeedMatrixUpdate )
				{
					m_NeedMatrixUpdate = false;

					Matrix.RotationQuaternion ( ref m_WorldRotation, out m_WorldMatrix );
					m_WorldMatrix.TranslationVector = m_WorldPosition;

					Matrix scale;
					Matrix.Scaling ( ref m_WorldScale, out scale );

					Matrix.Multiply ( ref m_WorldMatrix, ref scale, out m_WorldMatrix );
				}
				return m_WorldMatrix;
			}
		}

		/// <summary>
		/// get/set the world positon
		/// </summary>
		public Vector3 Position
		{
			get
			{
				if ( m_NeedParentUpdate ) { UpdateFromParent (); }
				return m_WorldPosition;
			}
			set
			{
				if ( m_Parent != null )
				{
					Vector3 position = m_Parent.Position;
					Vector3.Subtract ( ref value, ref position, out value );
				}
				LocalPosition = value;
			}
		}

		/// <summary>
		///
		/// </summary>
		public Vector3 EulerAngles
		{
			get
			{
				if ( m_NeedParentUpdate ) { UpdateFromParent (); }
				Vector3 euler;
				Mathf.QuaternionToYawPitchRoll ( ref m_WorldRotation, out euler );
				Util.Swap ( ref euler.X, ref euler.Y );
				return euler * Mathf.Rad2Deg; // to degrees
			}
			set
			{
				value *= Mathf.Deg2Rad; // to radians
				Quaternion rotation;
				Quaternion.RotationYawPitchRoll ( value.Y, value.X, value.Z, out rotation );
				Rotation = rotation;
			}
		}

		/// <summary>
		/// get/set the world rotation
		/// </summary>
		public Quaternion Rotation
		{
			get
			{
				if ( m_NeedParentUpdate ) { UpdateFromParent (); }
				return m_WorldRotation;
			}
			set
			{
				if ( m_Parent != null )
				{
					Quaternion rotation = m_Parent.Rotation;
					rotation.Invert ();
					Quaternion.Multiply ( ref rotation, ref value, out value );
				}
				LocalRotation = value;
			}
		}

		/// <summary>
		/// get the world scale of the node
		/// </summary>
		public Vector3 Scale
		{
			get
			{
				if ( m_NeedParentUpdate ) { UpdateFromParent (); }
				return m_WorldScale;
			}
			//set { throw new System.NotImplementedException (); }
		}

		/// <summary>
		/// get/set the matrix relative to here parent
		/// </summary>
		public Matrix LocalMatrix
		{
			get
			{
				if ( m_NeedLocalMatrixUpdate )
				{
					m_NeedLocalMatrixUpdate = false;

					Matrix.RotationQuaternion ( ref m_LocalRotation, out m_LocalMatrix );
					m_LocalMatrix.TranslationVector = m_LocalPosition;

					Matrix scale;
					Matrix.Scaling ( ref m_LocalScale, out scale );

					Matrix.Multiply ( ref m_LocalMatrix, ref scale, out m_LocalMatrix );
				}
				return m_LocalMatrix;
			}
		}

		/// <summary>
		///
		/// </summary>
		public Vector3 LocalPosition
		{
			get { return m_LocalPosition; }
			set
			{
				m_LocalPosition = value;
				NeedUpdate ();
			}
		}

		/// <summary>
		///
		/// </summary>
		public Vector3 LocalEulerAngles
		{
			get
			{
				Vector3 euler;
				Mathf.QuaternionToYawPitchRoll ( ref m_LocalRotation, out euler );
				Util.Swap ( ref euler.X, ref euler.Y );
				return euler * Mathf.Rad2Deg; // to degrees
			}
			set
			{
				value *= Mathf.Deg2Rad; // to radians
				Quaternion rotation;
				Quaternion.RotationYawPitchRoll ( value.Y, value.X, value.Z, out rotation );
				LocalRotation = rotation;
			}
		}

		/// <summary>
		///
		/// </summary>
		public Quaternion LocalRotation
		{
			get { return m_LocalRotation; }
			set
			{
				m_LocalRotation = value;
				NeedUpdate ();
			}
		}

		/// <summary>
		///
		/// </summary>
		public Vector3 LocalScale
		{
			get { return m_LocalScale; }
			set
			{
				m_LocalScale = value;
				NeedUpdate ();
			}
		}

		public Vector3 Right
		{
			get
			{
				Quaternion rot = Rotation;
				Vector3 direction = Vector3.UnitX;
				Vector3.Transform ( ref direction, ref rot, out direction );
				return direction;
			}
		}

		public Vector3 Up
		{
			get
			{
				Quaternion rot = Rotation;
				Vector3 direction = Vector3.UnitY;
				Vector3.Transform ( ref direction, ref rot, out direction );
				return direction;
			}
		}

		public Vector3 Forward
		{
			get
			{
				Quaternion rot = Rotation;
				Vector3 direction = Vector3.UnitZ;
				Vector3.Transform ( ref direction, ref rot, out direction );
				return direction;
			}
		}

		/// <summary>
		/// Has the transform changed since the last time the flag was set to 'false'?
		/// </summary>
		public bool HasChanged { get; set; }

		public Transform this[int index]
		{
			get
			{
				if ( m_Children == null || index < 0 || index >= m_Children.Count )
					throw new ArgumentOutOfRangeException ( "index" );

				return m_Children[index];
			}
		}

		//internal Transform ( SceneManager sceneManager, string name )
		//{
		//	m_SceneManager = sceneManager;
		//	Name = name;
		//}

		public void AddChild ( Transform child )
		{
			if ( child == null )
				throw new ArgumentNullException ( "child" );
			//if ( child.SceneManager != SceneManager )
			//	throw new ArgumentException ( "Given child is created form a another SceneManager!", "child" );

			if ( m_Children == null ) { m_Children = new List<Transform> (); }
			m_Children.Add ( child );
			if ( child.m_Parent != null )
			{
				child.m_Parent.RemoveChild ( child );
			}
			else
			{
				//SceneManager.RemoveRootNode ( child );
			}
			child.m_Parent = this;
		}

		public bool RemoveChild ( Transform child )
		{
			if ( child != null && m_Children != null )
			{
				//if ( child.SceneManager != SceneManager )
				//	throw new ArgumentException ( "Given child is created form a another SceneManager!", "child" );

				int index = m_Children.IndexOf ( child );
				if ( index != -1 )
				{
					RemoveChildAt ( index );
					return true;
				}
			}
			return false;
		}

		public void DetachAllChilds ()
		{
			if ( m_Children == null ) { return; }
			for ( int i = m_Children.Count - 1; i >= 0; i-- )
			{
				RemoveChildAt ( i );
			}
		}

		public bool Contains ( Transform child )
		{
			return child != null && m_Children != null && m_Children.Contains ( child );
		}

		public override string ToString ()
		{
			return string.Format ( "Transform of node '{0}'", Name );
		}

		void ICollection<Transform>.Add ( Transform child )
		{
			AddChild ( child );
		}

		bool ICollection<Transform>.Remove ( Transform child )
		{
			return RemoveChild ( child );
		}

		void ICollection<Transform>.Clear ()
		{
			DetachAllChilds ();
		}

		void ICollection<Transform>.CopyTo ( Transform[] array, int arrayIndex )
		{
			if ( m_Children != null )
			{
				m_Children.CopyTo ( array, arrayIndex );
			}
		}

		internal override void DestroyObject ()
		{
			if ( m_Parent != null )
			{
				Parent = null;
			}
			if ( GameObject != null )
			{
				var tmp = GameObject;
				GameObject = null;
				tmp.DestroyObject ();
			}
			if ( m_Children != null )
			{
				for ( int i = m_Children.Count - 1; i >= 0; i-- )
				{
					var child = m_Children[i];
					child.DestroyObject ();
				}
			}
			base.DestroyObject ();
		}

		private void RemoveChildAt ( int index )
		{
			var child = m_Children[index];
			child.m_Parent = null;
			m_Children.RemoveAt ( index );
			//SceneManager.AddRootNode ( child );
			cancelUpdate ( child );
		}

		private void UpdateFromParent ()
		{
			if ( m_Parent != null )
			{
				// Update orientation
				Quaternion parentRotation = m_Parent.Rotation;

				// Combine orientation with that of parent
				m_WorldRotation = parentRotation * m_LocalRotation;

				// Update scale
				Vector3 parentScale = m_Parent.Scale;
				// Scale own position by parent scale, NB just combine
				// as equivalent axes, no shearing
				Vector3.Multiply ( ref parentScale, ref m_LocalScale, out m_WorldScale );

				// Change position vector based on parent's orientation & scale
				Vector3.Multiply ( ref parentScale, ref m_LocalPosition, out m_WorldPosition );
				Vector3.Transform ( ref m_WorldPosition, ref parentRotation, out m_WorldPosition );

				// Add altered position vector to parents
				m_WorldPosition += m_Parent.Position;
			}
			else
			{
				m_WorldMatrix = m_LocalMatrix;
				m_WorldPosition = m_LocalPosition;
				m_WorldRotation = m_LocalRotation;
				m_WorldScale = m_LocalScale;
			}
			m_NeedParentUpdate = false;
		}

		private void NeedUpdate ( bool forceParentUpdate = false )
		{
			HasChanged = true;
			m_NeedParentUpdate = true;
			m_NeedChildUpdate = true;
			m_NeedMatrixUpdate = true;
			m_NeedLocalMatrixUpdate = true;

			// Make sure we're not root and parent hasn't been notified before
			if ( m_Parent != null && (!m_ParentNotified || forceParentUpdate) )
			{
				m_Parent.requestUpdate ( this, forceParentUpdate );
				m_ParentNotified = true;
			}

			// all children will be updated
			if ( m_ChildrenToUpdate != null ) m_ChildrenToUpdate.Clear ();
		}

		private void requestUpdate ( Transform child, bool forceParentUpdate )
		{
			// If we're already going to update everything this doesn't matter
			if ( m_NeedChildUpdate )
			{
				return;
			}

			if ( m_ChildrenToUpdate == null ) m_ChildrenToUpdate = new HashSet<Transform> ();
			m_ChildrenToUpdate.Add ( child );
			// Request selective update of me, if we didn't do it before
			if ( m_Parent != null && (!m_ParentNotified || forceParentUpdate) )
			{
				m_Parent.requestUpdate ( this, forceParentUpdate );
				m_ParentNotified = true;
			}
		}

		private void cancelUpdate ( Transform child )
		{
			if ( m_ChildrenToUpdate != null ) m_ChildrenToUpdate.Remove ( child );

			// Propogate this up if we're done
			if ( (m_ChildrenToUpdate != null && m_ChildrenToUpdate.Count == 0) && m_Parent != null && !m_NeedChildUpdate )
			{
				m_Parent.cancelUpdate ( this );
				m_ParentNotified = false;
			}
		}

		/// <summary>
		/// Update the node and here childs
		/// </summary>
		/// <param name="updateChildren">==true update all childrens</param>
		/// <param name="parentHasChanged">==true update matrix and all childrens</param>
		internal void Update ( bool updateChildren, bool parentHasChanged )
		{
			// always clear information about parent notification
			m_ParentNotified = false;

			// Short circuit the off case
			if ( !updateChildren && !m_NeedParentUpdate && !m_NeedChildUpdate && !parentHasChanged )
			{
				return;
			}

			// See if we should process everyone
			if ( m_NeedParentUpdate || parentHasChanged )
			{
				// Update transforms from parent
				UpdateFromParent ();
			}

			if ( m_NeedChildUpdate || parentHasChanged )
			{
				for ( int i = 0, len = m_Children.Count; i < len; ++i )
				{
					m_Children[i].Update ( true, true );
				}
			}
			else if ( m_ChildrenToUpdate != null && m_ChildrenToUpdate.Count > 0 )
			{
				// Just update selected children
				foreach ( var child in m_ChildrenToUpdate )
				{
					child.Update ( true, false );
				}
			}

			if ( m_ChildrenToUpdate != null )
				m_ChildrenToUpdate.Clear ();

			m_NeedChildUpdate = false;
		}

		#region IEnumerable<Transform> Member

		public struct Enumerator : IEnumerator<Transform>
		{
			private int m_Index;
			private List<Transform> m_Transforms;
			private Transform m_Current;

			public Transform Current { get { return m_Current; } }

			object IEnumerator.Current { get { return m_Current; } }

			internal Enumerator ( List<Transform> transforms )
			{
				m_Transforms = transforms;
				m_Index = 0;
				m_Current = null;
			}

			public bool MoveNext ()
			{
				if ( m_Transforms != null && m_Index < m_Transforms.Count )
				{
					m_Current = m_Transforms[m_Index];
					m_Index++;
					return true;
				}
				m_Current = null;
				return false;
			}

			public void Reset ()
			{
				m_Index = 0;
				m_Current = null;
			}

			public void Dispose ()
			{
				m_Transforms = null;
				Reset ();
			}
		}

		public Enumerator GetEnumerator ()
		{
			return new Enumerator ( m_Children );
		}

		IEnumerator<Transform> IEnumerable<Transform>.GetEnumerator ()
		{
			return new Enumerator ( m_Children );
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return new Enumerator ( m_Children );
		}

		#endregion IEnumerable<Transform> Member
	}
}