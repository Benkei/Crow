/**
 * Lexer.cs
 *   JSON lexer implementation based on a finite state machine.
 *
 * The authors disclaim copyright to this source code. For more details, see
 * the COPYING file included with this distribution.
 **/

using System.IO;
using System.Text;

namespace CrowSerialization.LitJson
{
	internal partial class Lexer
	{
		private bool allow_comments;

		private bool allow_single_quoted_strings;

		private bool end_of_input;

		private FsmContext fsm_context;

		private int input_buffer;

		private int input_char;

		private TextReader reader;

		private int state;

		private StringBuilder string_buffer;

		private string string_value;

		private int token;

		private int unichar;

		public Lexer ( TextReader reader )
		{
			allow_comments = true;
			allow_single_quoted_strings = true;

			input_buffer = 0;
			string_buffer = new StringBuilder ( 128 );
			state = 1;
			end_of_input = false;
			this.reader = reader;

			fsm_context = new FsmContext ();
			fsm_context.L = this;
		}

		public bool AllowComments
		{
			get { return allow_comments; }
			set { allow_comments = value; }
		}

		public bool AllowSingleQuotedStrings
		{
			get { return allow_single_quoted_strings; }
			set { allow_single_quoted_strings = value; }
		}

		public bool EndOfInput
		{
			get { return end_of_input; }
		}

		public string StringValue
		{
			get { return string_value; }
		}

		public int Token
		{
			get { return token; }
		}

		public bool NextToken ( bool readStringValue )
		{
			StateHandler handler;
			fsm_context.Return = false;

			while ( true )
			{
				handler = fsm_handler_table[state - 1];

				if ( !handler ( ref fsm_context ) )
					throw new JsonException ( input_char );

				if ( end_of_input )
					return false;

				if ( fsm_context.Return )
				{
					string_value = readStringValue ? string_buffer.ToString () : null;
					string_buffer.Length = 0;
					token = fsm_return_table[state - 1];

					if ( token == (int)ParserToken.Char )
						token = input_char;

					state = fsm_context.NextState;

					return true;
				}

				state = fsm_context.NextState;
			}
		}

		private bool GetChar ()
		{
			if ( (input_char = NextChar ()) != -1 )
				return true;

			end_of_input = true;
			return false;
		}

		private int NextChar ()
		{
			if ( input_buffer != 0 )
			{
				int tmp = input_buffer;
				input_buffer = 0;

				return tmp;
			}

			return reader.Read ();
		}

		private void UngetChar ()
		{
			input_buffer = input_char;
		}

		private struct FsmContext
		{
			public Lexer L;
			public int NextState;
			public bool Return;
			public int StateStack;
		}
	}
}