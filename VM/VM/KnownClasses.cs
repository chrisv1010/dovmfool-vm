﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VM.VMObjects;
using System.Reflection;

namespace VM {
	public static class KnownClasses {
		public static Handle<Class> Object { get; private set; }
		public static Handle<Class> ObjectSet { get; private set; }
		public static Handle<Class> System { get; private set; }
		public static Handle<Class> System_Array { get; private set; }
		public static Handle<Class> System_Integer { get; private set; }
		public static Handle<Class> System_String { get; private set; }
		public static Handle<Class> System_Reflection_Class { get; private set; }
		public static Handle<Class> System_Reflection_Message_Handler { get; private set; }
		public static Handle<Class> System_Reflection_Visibility { get; private set; }
		public static Handle<Class> System_Threading { get; private set; }
		public static Handle<Class> System_Threading_Thread { get; private set; }
		public static Handle<Class> System_Exception { get; private set; }
		public static Handle<Class> System_OutOfMemoryException { get; private set; }
		public static Handle<Class> System_InvalidVMProgramException { get; private set; }
		public static Handle<Class> System_InvalidThreadIdException { get; private set; }
		public static Handle<Class> System_ClassLoaderException { get; private set; }
		public static Handle<Class> System_InterpretorException { get; private set; }
		public static Handle<Class> System_InterpretorFailedToStopException { get; private set; }
		public static Handle<Class> System_ApplicationException { get; private set; }
		public static Handle<Class> System_InvalidCastException { get; private set; }
		public static Handle<Class> System_ArgumentException { get; private set; }
		public static Handle<Class> System_ArgumentOutOfRangeException { get; private set; }
		public static Handle<Class> System_MessageNotUnderStoodException { get; private set; }
		public static Handle<Class> System_ClassNotFoundException { get; private set; }
		public static Handle<Class> System_UnknownExternalCallException { get; private set; }

		static Handle<Class>[] handles;

		static KnownClasses() {
			var dummyId = -1;
			var props = typeof( KnownClasses ).GetProperties( BindingFlags.Static | BindingFlags.Public );
			global::System.Diagnostics.Debugger.Break();
			handles = new Handle<Class>[props.Count()];
			props.ForEach( p => {
				var h = new DummyClassHandle( p.Name, dummyId-- );
				handles[h.Start * -1] = h;
				p.SetValue( null, h, null );
			} );
		}

		internal static void Update() {
			handles.ForEach( ( h, i ) => handles[i] = VirtualMachine.ResolveClass( null, ((DummyClassHandle) h).Name.ToVMString() ).ToHandle() );
		}

		public static Class Resolve( int start ) {
			if (start > 0)
				return (Class) start;
			return handles[start * -1];
		}

		#region DummyClassHandle
		class DummyClassHandle : Handle<VMObjects.Class> {
			public readonly string Name;
			int value;
			public new int Value { get { return value; } }
			public override bool IsValid { get { return true; } }
			public override int Start { get { return Value; } }
			internal override MemoryManagerBase.HandleBase.HandleUpdater Updater { get { return null; } }

			public DummyClassHandle( string name, int value )
				: base( (VMObjects.Class) 0 ) {
				this.Name = name.Replace( "_", "." );
				Init( value );
			}

			protected override void Init( int value ) {
				this.value = value;
			}

			protected override void InternalUnregister() { }
			internal override void Unregister() { }
		}
		#endregion
	}
}