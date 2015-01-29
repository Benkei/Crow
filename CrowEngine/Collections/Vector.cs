using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace CrowEngine.Collections
{
	[Serializable, DebuggerDisplay ( "Count = {Count}" )]
	public class Vector<T> : IList<T>
	{
		private const int DEFAULT_CAPACITY = 4;

		private T[] m_Items;

		private int m_Size;

		private int m_Version;

		public Vector ()
		{
			m_Items = Arrays<T>.Empty;
		}

		public Vector ( int capacity )
		{
			if ( capacity < 0 )
				throw new ArgumentOutOfRangeException ( "value" );
			if ( capacity == 0 )
				m_Items = Arrays<T>.Empty;
			else
				m_Items = new T[capacity];
		}

		public Vector ( IEnumerable<T> collection )
		{
			if ( collection == null )
				throw new ArgumentNullException ();
			ICollection<T> col = collection as ICollection<T>;
			if ( col == null )
			{
				m_Size = 0;
				m_Items = Arrays<T>.Empty;
				foreach ( var item in collection )
				{
					Add ( item );
				}
				return;
			}
			else
			{
				m_Size = col.Count;
				if ( m_Size == 0 )
					m_Items = Arrays<T>.Empty;
				else
				{
					m_Items = new T[m_Size];
					col.CopyTo ( m_Items, 0 );
				}
			}
		}

		public T[] Buffer
		{
			get { return m_Items; }
		}

		public int Capacity
		{
			get { return m_Items.Length; }
			set
			{
				if ( value < m_Size )
					throw new ArgumentOutOfRangeException ();
				if ( value != m_Items.Length )
				{
					if ( value > 0 )
					{
						T[] array = new T[value];
						if ( m_Size > 0 )
							Array.Copy ( m_Items, 0, array, 0, m_Size );
						m_Items = array;
					}
					else m_Items = Arrays<T>.Empty;
				}
			}
		}

		public int Count
		{
			get { return m_Size; }
			set
			{
				if ( value >= m_Items.Length )
					EnsureCapacity ( value );
				m_Size = value;
			}
		}

		bool ICollection<T>.IsReadOnly
		{
			get { return false; }
		}

		public T this[int index]
		{
			get
			{
				if ( index >= m_Size )
					throw new ArgumentOutOfRangeException ( "value" );
				return m_Items[index];
			}
			set
			{
				if ( index >= m_Size )
					throw new ArgumentOutOfRangeException ( "value" );
				m_Items[index] = value;
				m_Version++;
			}
		}

		public void Add ( T item )
		{
			Add ( ref item );
		}

		public void Add ( ref T item )
		{
			if ( m_Size == m_Items.Length )
				EnsureCapacity ( m_Size + 1 );
			m_Items[m_Size++] = item;
			m_Version++;
		}

		public void AddRange ( IEnumerable<T> collection )
		{
			InsertRange ( m_Size, collection );
		}

		public int BinarySearch ( int index, int count, T item, IComparer<T> comparer )
		{
			if ( index < 0 )
				throw new ArgumentOutOfRangeException ();
			if ( count < 0 )
				throw new ArgumentOutOfRangeException ();
			if ( m_Size - index < count )
				throw new ArgumentException ();
			return Array.BinarySearch<T> ( m_Items, index, count, item, comparer );
		}

		public int BinarySearch ( T item )
		{
			return BinarySearch ( 0, Count, item, null );
		}

		public int BinarySearch ( T item, IComparer<T> comparer )
		{
			return BinarySearch ( 0, Count, item, comparer );
		}

		public void Clear ()
		{
			if ( m_Size > 0 )
			{
				Array.Clear ( m_Items, 0, m_Size );
				m_Size = 0;
				m_Version++;
			}
		}

		public void Clear ( bool force )
		{
			if ( force || m_Size > 0 )
			{
				if ( force )
					Array.Clear ( m_Items, 0, m_Items.Length );
				else
					Array.Clear ( m_Items, 0, m_Size );
				m_Size = 0;
				m_Version++;
			}
		}

		public bool Contains ( T item )
		{
			if ( item == null )
			{
				for ( int i = 0; i < m_Size; i++ )
				{
					if ( m_Items[i] == null )
						return true;
				}
				return false;
			}
			EqualityComparer<T> comp = EqualityComparer<T>.Default;
			for ( int j = 0; j < m_Size; j++ )
			{
				if ( comp.Equals ( m_Items[j], item ) )
					return true;
			}
			return false;
		}

		public void CopyTo ( T[] array )
		{
			CopyTo ( array, 0 );
		}

		public void CopyTo ( int index, T[] array, int arrayIndex, int count )
		{
			if ( m_Size - index < count )
				throw new ArgumentException ();
			Array.Copy ( m_Items, index, array, arrayIndex, count );
		}

		public void CopyTo ( T[] array, int arrayIndex )
		{
			Array.Copy ( m_Items, 0, array, arrayIndex, m_Size );
		}

		public int IndexOf ( T item )
		{
			return Array.IndexOf<T> ( m_Items, item, 0, m_Size );
		}

		public int IndexOf ( T item, int index )
		{
			if ( index > m_Size )
				throw new ArgumentOutOfRangeException ( "index" );
			return Array.IndexOf<T> ( m_Items, item, index, m_Size - index );
		}

		public int IndexOf ( T item, int index, int count )
		{
			if ( index < 0 || index > m_Size )
				throw new ArgumentOutOfRangeException ( "index" );
			if ( count < 0 || index > m_Size - count )
				throw new ArgumentOutOfRangeException ( "count" );
			return Array.IndexOf<T> ( m_Items, item, index, count );
		}

		public void Insert ( int index, T item )
		{
			if ( index < 0 || index > m_Size )
				throw new ArgumentOutOfRangeException ( "index" );
			if ( m_Size == m_Items.Length )
				EnsureCapacity ( m_Size + 1 );
			if ( index < m_Size )
				Array.Copy ( m_Items, index, m_Items, index + 1, m_Size - index );
			m_Items[index] = item;
			m_Size++;
			m_Version++;
		}

		public void InsertRange ( int index, IEnumerable<T> collection )
		{
			if ( collection == null )
				throw new ArgumentNullException ( "collection" );
			if ( index < 0 || index > m_Size )
				throw new ArgumentOutOfRangeException ( "index" );
			ICollection<T> col = collection as ICollection<T>;
			if ( col != null )
			{
				int count = col.Count;
				if ( count > 0 )
				{
					EnsureCapacity ( m_Size + count );
					if ( index < m_Size )
						Array.Copy ( m_Items, index, m_Items, index + count, m_Size - index );
					if ( this == col )
					{
						Array.Copy ( m_Items, 0, m_Items, index, index );
						Array.Copy ( m_Items, index + count, m_Items, index * 2, m_Size - index );
					}
					else
						col.CopyTo ( m_Items, index );
					m_Size += count;
				}
			}
			else
			{
				foreach ( var item in collection )
				{
					Insert ( index++, item );
				}
			}
			m_Version++;
		}

		public int LastIndexOf ( T item )
		{
			if ( m_Size == 0 )
				return -1;
			return LastIndexOf ( item, m_Size - 1, m_Size );
		}

		public int LastIndexOf ( T item, int index )
		{
			if ( index >= m_Size )
				throw new ArgumentOutOfRangeException ( "index" );
			return LastIndexOf ( item, index, index + 1 );
		}

		public int LastIndexOf ( T item, int index, int count )
		{
			if ( Count != 0 && index < 0 )
				throw new ArgumentOutOfRangeException ();
			if ( Count != 0 && count < 0 )
				throw new ArgumentOutOfRangeException ();
			if ( m_Size == 0 )
				return -1;
			if ( index >= m_Size )
				throw new ArgumentOutOfRangeException ( "index" );
			if ( count > index + 1 )
				throw new ArgumentOutOfRangeException ( "count" );
			return Array.LastIndexOf<T> ( m_Items, item, index, count );
		}

		public bool Remove ( T item )
		{
			int num = IndexOf ( item );
			if ( num >= 0 )
			{
				RemoveAt ( num );
				return true;
			}
			return false;
		}

		public void RemoveAt ( int index )
		{
			if ( index >= m_Size )
				throw new ArgumentOutOfRangeException ( "index" );
			m_Size--;
			if ( index < m_Size )
				Array.Copy ( m_Items, index + 1, m_Items, index, m_Size - index );
			m_Items[m_Size] = default ( T );
			m_Version++;
		}

		public void RemoveRange ( int index, int count )
		{
			if ( index < 0 )
				throw new ArgumentOutOfRangeException ();
			if ( count < 0 )
				throw new ArgumentOutOfRangeException ();
			if ( m_Size - index < count )
				throw new ArgumentException ();
			if ( count > 0 )
			{
				m_Size -= count;
				if ( index < m_Size )
					Array.Copy ( m_Items, index + count, m_Items, index, m_Size - index );
				Array.Clear ( m_Items, m_Size, count );
				m_Version++;
			}
		}

		public void Reverse ()
		{
			Reverse ( 0, Count );
		}

		public void Reverse ( int index, int count )
		{
			if ( index < 0 )
				throw new ArgumentOutOfRangeException ();
			if ( count < 0 )
				throw new ArgumentOutOfRangeException ();
			if ( m_Size - index < count )
				throw new ArgumentException ();
			Array.Reverse ( m_Items, index, count );
			m_Version++;
		}

		public void Sort ()
		{
			Sort ( 0, Count, null );
		}

		public void Sort ( IComparer<T> comparer )
		{
			Sort ( 0, Count, comparer );
		}

		public void Sort ( int index, int count, IComparer<T> comparer )
		{
			if ( index < 0 )
				throw new ArgumentOutOfRangeException ();
			if ( count < 0 )
				throw new ArgumentOutOfRangeException ();
			if ( m_Size - index < count )
				throw new ArgumentException ();
			Array.Sort<T> ( m_Items, index, count, comparer );
			m_Version++;
		}

		public T[] ToArray ()
		{
			T[] array = new T[m_Size];
			Array.Copy ( m_Items, 0, array, 0, m_Size );
			return array;
		}

		public void TrimExcess ()
		{
			int num = (int)((double)m_Items.Length * 0.9);
			if ( m_Size < num )
				Capacity = m_Size;
		}

		public Vector<T>.Enumerator GetEnumerator ()
		{
			return new Enumerator ( this );
		}

		public Vector<T> GetRange ( int index, int count )
		{
			if ( index < 0 )
				throw new ArgumentOutOfRangeException ();
			if ( count < 0 )
				throw new ArgumentOutOfRangeException ();
			if ( m_Size - index < count )
				throw new ArgumentException ();
			Vector<T> list = new Vector<T> ( count );
			Array.Copy ( m_Items, index, list.m_Items, 0, count );
			list.m_Size = count;
			return list;
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return new Enumerator ( this );
		}

		IEnumerator<T> IEnumerable<T>.GetEnumerator ()
		{
			return new Enumerator ( this );
		}

		private void EnsureCapacity ( int min )
		{
			if ( m_Items.Length < min )
			{
				long num = (m_Items.Length == 0) ? DEFAULT_CAPACITY : (m_Items.Length * 2);
				if ( num > int.MaxValue )
					num = int.MaxValue;
				else if ( num < min )
					num = min;
				Capacity = (int)num;
			}
		}

		[Serializable]
		public struct Enumerator : IEnumerator<T>
		{
			private T m_Current;
			private int m_Index;
			private Vector<T> m_Vector;
			private int m_Version;

			internal Enumerator ( Vector<T> vector )
			{
				m_Vector = vector;
				m_Index = 0;
				m_Version = vector.m_Version;
				m_Current = default ( T );
			}

			public T Current
			{
				get { return m_Current; }
			}

			object IEnumerator.Current
			{
				get
				{
					if ( m_Index == 0 || m_Index == m_Vector.m_Size + 1 )
						throw new InvalidOperationException ();
					return Current;
				}
			}

			public void Dispose ()
			{
			}

			void IEnumerator.Reset ()
			{
				if ( m_Version != m_Vector.m_Version )
					throw new InvalidOperationException ( "Origin list changed." );
				m_Index = 0;
				m_Current = default ( T );
			}

			public bool MoveNext ()
			{
				Vector<T> list = m_Vector;
				if ( m_Version == list.m_Version && m_Index < list.m_Size )
				{
					m_Current = list.m_Items[m_Index];
					m_Index++;
					return true;
				}
				if ( m_Version != list.m_Version )
					throw new InvalidOperationException ( "Origin list changed." );
				m_Index = list.m_Size + 1;
				m_Current = default ( T );
				return false;
			}
		}
	}
}