﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VM {
	partial class NoncollectingMemoryManager : MemoryManagerBase {
		Word[] memory;
		int position = 1;

		public override int SizeInWords { get { return memory.Length; } }
		public override int FreeSizeInWords { get { return memory.Length - position; } }
		public override int AllocatedSizeInWords { get { return position; } }

		internal override Word this[int index] {
			get { return memory[index]; }
			set {
//#if DEBUG
//                var frame = new System.Diagnostics.StackTrace().GetFrame( 2 );
//                System.Diagnostics.Trace.TraceInformation( index.ToString( "X8" ) + "=" + ((uint) value).ToString( "X8" ) + ": " + frame.GetMethod().Name );
//#endif
				memory[index] = value;
			}
		}

		public NoncollectingMemoryManager( int size ) {
			if (size < 0)
				throw new ArgumentOutOfRangeException( "size" );

			memory = new Word[size];
		}

		internal override T Allocate<T>( int size ) {
			if (position + size >= memory.Length)
				throw new OutOfMemoryException();

			var pos = position;
			size += 1;
			position += size;

			var obj = new T().New( pos );
			memory[pos] = (size << 4) | (((int) obj.TypeId) & 0x0000000F);
			return obj;
		}
	}
}