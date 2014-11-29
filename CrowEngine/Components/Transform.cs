using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using CrowEngine.Mathematics;

namespace CrowEngine.Components
{
	public class Transform : Component, ICollection<Transform>, IDisposable
	{
		[Flags]
		enum Dirty : byte
		{
			NeedParentUpdate = 1 << 0,
			NeedChildUpdate = 1 << 1,
			WorldMatrix = 1 << 2,
			LocalMatrix = 1 << 3,
			ParentNotified = 1 << 4,
		}

		private Transform m_Parent;
		private List<Transform> m_Children;
		private HashSet<Transform> m_ChildrenToUpdate;

		private Matrix3x3 m_WorldMatrix = Matrix3x3.Identity;
		private Vector3 m_WorldScale = Vector3.One;
		private Vector3 m_WorldPosition;
		private Quaternion m_WorldRotation = Quaternion.Identity;

		private Matrix3x3 m_LocalMatrix = Matrix3x3.Identity;
		private Vector3 m_LocalScale = Vector3.One;
		private Vector3 m_LocalPosition;
		private Quaternion m_LocalRotation = Quaternion.Identity;

		//SceneManager		m_SceneManager;

		private Dirty m_Dirty;

		public Transform Root
		{
			[MethodImpl ( MethodImplOptions.AggressiveInlining )]
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
			[MethodImpl ( MethodImplOptions.AggressiveInlining )]
			get { return m_Parent; }
			[MethodImpl ( MethodImplOptions.AggressiveInlining )]
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
			[MethodImpl ( MethodImplOptions.AggressiveInlining )]
			get { return m_Children != null ? m_Children.Count : 0; }
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
			[MethodImpl ( MethodImplOptions.AggressiveInlining )]
			get
			{
				CheckWorldMatrix ();
				Matrix m = (Matrix)m_WorldMatrix;
				m.TranslationVector = m_WorldPosition;
				return m;
			}
		}

		/// <summary>
		/// get/set the world positon
		/// </summary>
		public Vector3 Position
		{
			[MethodImpl ( MethodImplOptions.AggressiveInlining )]
			get
			{
				if ( (m_Dirty & Dirty.NeedParentUpdate) > 0 )
					UpdateFromParent ();
				return m_WorldPosition;
			}
			[MethodImpl ( MethodImplOptions.AggressiveInlining )]
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
			[MethodImpl ( MethodImplOptions.AggressiveInlining )]
			get
			{
				if ( (m_Dirty & Dirty.NeedParentUpdate) > 0 )
					UpdateFromParent ();
				Vector3 euler;
				Mathf.QuaternionToYawPitchRoll ( ref m_WorldRotation, out euler );
				Util.Swap ( ref euler.X, ref euler.Y );
				return euler * Mathf.Rad2Deg; // to degrees
			}
			[MethodImpl ( MethodImplOptions.AggressiveInlining )]
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
			[MethodImpl ( MethodImplOptions.AggressiveInlining )]
			get
			{
				if ( (m_Dirty & Dirty.NeedParentUpdate) > 0 )
					UpdateFromParent ();
				return m_WorldRotation;
			}
			[MethodImpl ( MethodImplOptions.AggressiveInlining )]
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
			[MethodImpl ( MethodImplOptions.AggressiveInlining )]
			get
			{
				if ( (m_Dirty & Dirty.NeedParentUpdate) > 0 )
					UpdateFromParent ();
				return m_WorldScale;
			}
			//set { throw new System.NotImplementedException (); }
		}

		/// <summary>
		/// get/set the matrix relative to here parent
		/// </summary>
		public Matrix LocalMatrix
		{
			[MethodImpl ( MethodImplOptions.AggressiveInlining )]
			get
			{
				CheckLocalMatrix ();
				Matrix m = (Matrix)m_LocalMatrix;
				m.TranslationVector = m_LocalPosition;
				return m;
			}
		}

		/// <summary>
		///
		/// </summary>
		public Vector3 LocalPosition
		{
			[MethodImpl ( MethodImplOptions.AggressiveInlining )]
			get { return m_LocalPosition; }
			[MethodImpl ( MethodImplOptions.AggressiveInlining )]
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
			[MethodImpl ( MethodImplOptions.AggressiveInlining )]
			get
			{
				Vector3 euler;
				Mathf.QuaternionToYawPitchRoll ( ref m_LocalRotation, out euler );
				Util.Swap ( ref euler.X, ref euler.Y );
				return euler * Mathf.Rad2Deg; // to degrees
			}
			[MethodImpl ( MethodImplOptions.AggressiveInlining )]
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
			[MethodImpl ( MethodImplOptions.AggressiveInlining )]
			get { return m_LocalRotation; }
			[MethodImpl ( MethodImplOptions.AggressiveInlining )]
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
			[MethodImpl ( MethodImplOptions.AggressiveInlining )]
			get { return m_LocalScale; }
			[MethodImpl ( MethodImplOptions.AggressiveInlining )]
			set
			{
				m_LocalScale = value;
				NeedUpdate ();
			}
		}

		public Vector3 Right
		{
			[MethodImpl ( MethodImplOptions.AggressiveInlining )]
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
			[MethodImpl ( MethodImplOptions.AggressiveInlining )]
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
			[MethodImpl ( MethodImplOptions.AggressiveInlining )]
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
			[MethodImpl ( MethodImplOptions.AggressiveInlining )]
			get
			{
				if ( m_Children == null || index < 0 || index >= m_Children.Count )
					throw new ArgumentOutOfRangeException ( "index" );

				return m_Children[index];
			}
		}


		public void AddChild ( Transform child )
		{
			if ( child == null )
				throw new ArgumentNullException ( "child" );
			//if ( child.SceneManager != SceneManager )
			//	throw new ArgumentException ( "Given child is created form a another SceneManager!", "child" );

			if ( child.m_Parent == this )
				return;

			if ( m_Children == null )
				m_Children = new List<Transform> ();

			if ( child.m_Parent != null )
				child.m_Parent.RemoveChild ( child );
			else
			{
				//SceneManager.RemoveRootNode ( child );
			}
			m_Children.Add ( child );
			child.m_Parent = this;
		}

		public bool RemoveChild ( Transform child )
		{
			if ( child != null && m_Children != null && child.m_Parent == this )
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

		public void RemoveChildAt ( int index )
		{
			if ( m_Children == null || index < 0 || index >= m_Children.Count )
				throw new ArgumentOutOfRangeException ();

			var child = m_Children[index];
			child.m_Parent = null;
			m_Children.RemoveAt ( index );
			//SceneManager.AddRootNode ( child );
			CancelUpdate ( child );
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

		public void Dispose ()
		{
			if ( m_Parent != null )
			{
				Parent = null;
			}
			if ( m_Children != null )
			{
				for ( int i = m_Children.Count - 1; i >= 0; i-- )
				{
					var child = m_Children[i];
					RemoveChildAt ( i );
					child.Dispose ();
				}
			}
		}

		[MethodImpl ( MethodImplOptions.AggressiveInlining )]
		private void CheckWorldMatrix ()
		{
			if ( (m_Dirty & Dirty.NeedParentUpdate) > 0 )
				UpdateFromParent ();
			if ( (m_Dirty & Dirty.WorldMatrix) > 0 )
			{
				m_Dirty &= ~Dirty.WorldMatrix;

				Matrix3x3.RotationQuaternion ( ref m_WorldRotation, out m_WorldMatrix );
				Matrix3x3 scale;
				Matrix3x3.Scaling ( ref m_WorldScale, out scale );
				Matrix3x3.Multiply ( ref m_WorldMatrix, ref scale, out m_WorldMatrix );
			}
		}

		[MethodImpl ( MethodImplOptions.AggressiveInlining )]
		private void CheckLocalMatrix ()
		{
			if ( (m_Dirty & Dirty.LocalMatrix) > 0 )
			{
				m_Dirty &= ~Dirty.LocalMatrix;

				Matrix3x3.RotationQuaternion ( ref m_LocalRotation, out m_LocalMatrix );
				Matrix3x3 scale;
				Matrix3x3.Scaling ( ref m_LocalScale, out scale );
				Matrix3x3.Multiply ( ref m_LocalMatrix, ref scale, out m_LocalMatrix );
			}
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
			base.DestroyObject ();
			if ( m_Parent != null )
			{
				Parent = null;
			}
			if ( m_Children != null )
			{
				for ( int i = m_Children.Count - 1; i >= 0; i-- )
				{
					var child = m_Children[i];
					child.DestroyObject ();
				}
			}
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
			m_Dirty &= ~Dirty.NeedParentUpdate;
		}

		private void NeedUpdate ( bool forceParentUpdate = false )
		{
			HasChanged = true;
			m_Dirty = ~(Dirty)0;

			// Make sure we're not root and parent hasn't been notified before
			if ( m_Parent != null && ((m_Dirty & Dirty.ParentNotified) == 0 || forceParentUpdate) )
			{
				m_Parent.RequestUpdate ( this, forceParentUpdate );
				m_Dirty |= Dirty.ParentNotified;
			}

			// all children will be updated
			if ( m_ChildrenToUpdate != null )
				m_ChildrenToUpdate.Clear ();
		}

		private void RequestUpdate ( Transform child, bool forceParentUpdate )
		{
			// If we're already going to update everything this doesn't matter
			if ( (m_Dirty & Dirty.NeedChildUpdate) > 0 )
			{
				return;
			}

			if ( m_ChildrenToUpdate == null )
				m_ChildrenToUpdate = new HashSet<Transform> ();
			m_ChildrenToUpdate.Add ( child );
			// Request selective update of me, if we didn't do it before
			if ( m_Parent != null && ((m_Dirty & Dirty.ParentNotified) == 0 || forceParentUpdate) )
			{
				m_Parent.RequestUpdate ( this, forceParentUpdate );
				m_Dirty |= Dirty.ParentNotified;
			}
		}

		private void CancelUpdate ( Transform child )
		{
			if ( m_ChildrenToUpdate != null )
				m_ChildrenToUpdate.Remove ( child );

			// Propogate this up if we're done
			if ( (m_ChildrenToUpdate != null && m_ChildrenToUpdate.Count == 0)
				&& m_Parent != null && (m_Dirty & Dirty.NeedChildUpdate) == 0 )
			{
				m_Parent.CancelUpdate ( this );
				m_Dirty &= ~Dirty.ParentNotified;
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
			m_Dirty &= ~Dirty.ParentNotified;

			// Short circuit the off case
			if ( !updateChildren
				&& !parentHasChanged
				&& (m_Dirty & (Dirty.NeedParentUpdate & Dirty.NeedChildUpdate)) == 0 )
			{
				return;
			}

			// See if we should process everyone
			if ( (m_Dirty & Dirty.NeedParentUpdate) > 0 || parentHasChanged )
			{
				// Update transforms from parent
				UpdateFromParent ();
			}

			if ( (m_Dirty & Dirty.NeedChildUpdate) > 0 || parentHasChanged )
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

			m_Dirty &= ~Dirty.NeedChildUpdate;
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