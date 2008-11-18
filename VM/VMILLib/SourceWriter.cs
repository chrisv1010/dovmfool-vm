﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Sekhmet.IO;

namespace VMILLib {
	public sealed class SourceWriter : IDisposable {
		StructuredWriter output;
		bool isDisposed;

		public SourceWriter( Stream output ) {
			this.output = new StructuredWriter( new StreamWriter( output ), "  " );
		}

		public SourceWriter( TextWriter output ) {
			this.output = new StructuredWriter( output );
		}

		public SourceWriter( string output ) : this( new FileStream( output, FileMode.Create, FileAccess.Write ) ) { }

		public void Write( Assembly assembly ) {
			assembly.Classes.ForEach( c => WriteClass( c ) );
		}

		void WriteClass( Class cls ) {
			output.WriteLine( ".class " + cls.Visibility.ToString().ToLower() + " " + cls.Name + (cls.InheritsFrom.Count != 0 ? " extends " + cls.InheritsFrom.Join( ", " ) + " " : "") + "{" );
			output.IndentationLevel++;

			var actions = new List<Action>();

			if (cls.Fields.Count != 0)
				actions.Add( () => output.WriteLine( ".fields { " + cls.Fields.Join( ", " ) + " }" ) );

			if (cls.DefaultHandler != null)
				actions.Add( () => WriteMessageHandler( cls.DefaultHandler ) );

			if (cls.Handlers.Count != 0)
				actions.Add( () => cls.Handlers.Join( h => WriteMessageHandler( h ), () => output.WriteLine() ) );

			if (cls.Classes.Count != 0)
				actions.Add( () => cls.Classes.Join( c => WriteClass( c ), () => output.WriteLine() ) );

			actions.Join( a => a(), () => output.WriteLine() );

			output.IndentationLevel--;
			output.WriteLine( "}" );
		}

		void WriteMessageHandler( MessageHandler handler ) {
			if (handler.Name == null)
				output.WriteLine( ".default {" );
			else {
				var name = handler.Name.Value;
				name = name.IndexOf( ":" ) != -1 ? name.Substring( 0, name.IndexOf( ":" ) ) : name;
				output.WriteLine( ".handler " + handler.Visibility.ToString().ToLower() + " " + name + "(" + handler.Arguments.Join( ", " ) + ") {" );
			}

			output.IndentationLevel += 2;

			var actions = new List<Action>();
			if (handler.IsEntrypoint)
				actions.Add( () => output.WriteLine( ".entrypoint" ) );

			if (handler.Locals.Count != 0)
				actions.Add( () => output.WriteLine( ".locals { " + handler.Locals.Join( ", " ) + " }" ) );

			if (handler.Instructions.Count != 0)
				actions.Add( () => handler.Instructions.ForEach( i => WriteInstruction( i ) ) );

			actions.Join( a => a(), () => output.WriteLine() );

			output.IndentationLevel -= 2;
			output.WriteLine( "}" );
		}

		void WriteInstruction( Instruction i ) {
			switch (i.OpCode) {
				case OpCode.None:
					if (i is Label) {
						output.IndentationLevel--;
						output.WriteLine( ((Label) i).Name + ": " );
						output.IndentationLevel++;
					} else
						throw new ArgumentOutOfRangeException( "Invalid OpCode encountered: '" + i.OpCode + "'." );
					break;
				case OpCode.StoreField:
					output.WriteLine( "store-field " + i.Operand );
					break;
				case OpCode.LoadField:
					output.WriteLine( "load-field " + i.Operand );
					break;
				case OpCode.StoreLocal:
					output.WriteLine( "store-local " + i.Operand );
					break;
				case OpCode.LoadLocal:
					output.WriteLine( "load-local " + i.Operand );
					break;
				case OpCode.PushLiteral:
					output.WriteLine( "push-literal " + (i.Operand is CString ? "\"" + i.Operand + "\"" : i.Operand) );
					break;
				case OpCode.Pop:
					output.WriteLine( "pop" );
					break;
				case OpCode.Dup:
					output.WriteLine( "dup" );
					break;
				case OpCode.NewInstance:
					output.WriteLine( "new-instance" );
					break;
				case OpCode.SendMessage:
					output.WriteLine( "send-message" );
					break;
				case OpCode.ReturnVoid:
					output.WriteLine( "return-void" );
					break;
				case OpCode.Return:
					output.WriteLine( "return" );
					break;
				case OpCode.Jump:
					output.WriteLine( "jump " + ((Label) i.Operand).Name );
					break;
				case OpCode.JumpIfTrue:
					output.WriteLine( "jump-if-true " + ((Label) i.Operand).Name );
					break;
				case OpCode.JumpIfFalse:
					output.WriteLine( "jump-if-false " + ((Label) i.Operand).Name );
					break;
				case OpCode.Throw:
					output.WriteLine( "throw" );
					break;
				case OpCode.Try:
					output.WriteLine( ".try {" );
					output.IndentationLevel++;
					break;
				case OpCode.Catch:
					output.IndentationLevel--;
					output.WriteLine( "} catch {" );
					output.IndentationLevel++;
					break;
				case OpCode.EndTryCatch:
					output.IndentationLevel--;
					output.WriteLine( "}" );
					break;
				default:
					throw new ArgumentOutOfRangeException( "Invalid OpCode encountered: '" + i.OpCode + "'." );
			}
		}

		#region IDisposable Members
		public void Dispose() {
			lock (this) {
				if (isDisposed)
					return;
				isDisposed = true;
			}
			if (output != null) {
				output.Dispose();
				output = null;
			}
		}
		#endregion
	}
}