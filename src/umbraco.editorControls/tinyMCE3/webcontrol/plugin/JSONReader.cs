/*
 * $Id: JSONReader.cs 439 2007-11-26 13:26:10Z spocke $
 *
 * Copyright © 2007, Moxiecode Systems AB, All rights reserved. 
 */

using System;
using System.IO;
using System.Text;
using System.Collections;

namespace umbraco.editorControls.tinyMCE3.webcontrol.plugin
{
    /// <summary>
	/// 
	/// </summary>
    [Obsolete("IDataType and all other references to the legacy property editors are no longer used this will be removed from the codebase in future versions")]
    public enum JSONToken
    {
		/// <summary> </summary>
		Boolean,

		/// <summary> </summary>
		Integer,

		/// <summary> </summary>
		String,

		/// <summary> </summary>
		Null,

		/// <summary> </summary>
		Float,

		/// <summary> </summary>
		StartArray,

		/// <summary> </summary>
		EndArray,

		/// <summary> </summary>
		PropertyName,

		/// <summary> </summary>
		StartObject,

		/// <summary> </summary>
		EndObject
	}

	/// <summary>
	///  Description of JSONReader.
	/// </summary>
    [Obsolete("IDataType and all other references to the legacy property editors are no longer used this will be removed from the codebase in future versions")]
    public class JSONReader
    {
		private TextReader reader;
		private JSONToken token;
		private object val;
		private JSONLocation location;
		private Stack lastLocations;
		private bool needProp;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="reader"></param>
		public JSONReader(TextReader reader) {
			this.reader = reader;
			this.val = null;
			this.token = JSONToken.Null;
			this.location = JSONLocation.Normal;
			this.lastLocations = new Stack();
			this.needProp = false;
		}

		/// <summary>
		/// 
		/// </summary>
		public JSONLocation Location {
			get { return location; }
		}

		/// <summary>
		/// 
		/// </summary>
		public JSONToken TokenType {
			get {
				return this.token;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public object Value {
			get {
				return this.val;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public bool Read() {
			int chr = this.reader.Read();

			if (chr != -1) {
				switch ((char) chr) {
					case '[':
						this.lastLocations.Push(this.location);
						this.location = JSONLocation.InArray;
						this.token = JSONToken.StartArray;
						this.val = null;
						this.ReadAway();
						return true;

					case ']':
						this.location = (JSONLocation) this.lastLocations.Pop();
						this.token = JSONToken.EndArray;
						this.val = null;
						this.ReadAway();

						if (this.location == JSONLocation.InObject)
							this.needProp = true;

						return true;

					case '{':
						this.lastLocations.Push(this.location);
						this.location = JSONLocation.InObject;
						this.needProp = true;
						this.token = JSONToken.StartObject;
						this.val = null;
						this.ReadAway();
						return true;

					case '}':
						this.location = (JSONLocation) this.lastLocations.Pop();
						this.token = JSONToken.EndObject;
						this.val = null;
						this.ReadAway();

						if (this.location == JSONLocation.InObject)
							this.needProp = true;

						return true;

					// String
					case '"':
					case '\'':
						return this.ReadString((char) chr);

					// Null
					case 'n':
						return this.ReadNull();

					// Bool
					case 't':
					case 'f':
						return this.ReadBool((char) chr);

					default:
						// Is number
						if (Char.IsNumber((char) chr) || (char) chr == '-' || (char) chr == '.')
							return this.ReadNumber((char) chr);

						return true;
				}
			}

			return false;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public override string ToString() {
			switch (this.token) {
				case JSONToken.Boolean:
					return "[Boolean] = " + ((bool) this.Value ? "true" : "false");

				case JSONToken.EndArray:
					return "[EndArray]";

				case JSONToken.EndObject:
					return "[EndObject]";

				case JSONToken.Float:
					return "[Float] = " + Convert.ToDouble(this.Value);

				case JSONToken.Integer:
					return "[Integer] = " + ((int) this.Value);

				case JSONToken.Null:
					return "[Null]";

				case JSONToken.StartArray:
					return "[StartArray]";

				case JSONToken.StartObject:
					return "[StartObject]";

				case JSONToken.String:
					return "[String]" + (string) this.Value;

				case JSONToken.PropertyName:
					return "[PropertyName]" + (string) this.Value;
			}

			return base.ToString();
		}
		
		#region private methods

		private bool ReadString(char quote) {
			StringBuilder buff = new StringBuilder();
			this.token = JSONToken.String;
			bool endString = false;
			int chr;

			while ((chr = this.reader.Peek()) != -1) {
				switch (chr) {
					case '\\':
						// Read away slash
						chr = this.reader.Read();

						// Read escape code
						chr = this.reader.Read();
						switch (chr) {
								case 't':
									buff.Append('\t');
									break;

								case 'b':
									buff.Append('\b');
									break;

								case 'f':
									buff.Append('\f');
									break;

								case 'r':
									buff.Append('\r');
									break;

								case 'n':
									buff.Append('\n');
									break;

								case 'u':
									buff.Append((char) Convert.ToInt32(ReadLen(4), 16));
									break;

								default:
									buff.Append((char) chr);
									break;
						}

						break;

						case '\'':
						case '"':
							if (chr == quote)
								endString = true;

							chr = this.reader.Read();
							if (chr != -1 && chr != quote)
								buff.Append((char) chr);

							break;

						default:
							buff.Append((char) this.reader.Read());
							break;
				}

				// String terminated
				if (endString)
					break;
			}

			this.ReadAway();

			this.val = buff.ToString();

			// Needed a property
			if (this.needProp) {
				this.token = JSONToken.PropertyName;
				this.needProp = false;
				return true;
			}

			if (this.location == JSONLocation.InObject && !this.needProp)
				this.needProp = true;

			return true;
		}

		private bool ReadNull() {
			this.token = JSONToken.Null;
			this.val = null;

			this.ReadAway(3); // ull
			this.ReadAway();

			if (this.location == JSONLocation.InObject && !this.needProp)
				this.needProp = true;

			return true;
		}

		private bool ReadNumber(char start) {
			StringBuilder buff = new StringBuilder();
			int chr;
			bool isFloat = false;

			this.token = JSONToken.Integer;
			buff.Append(start);

			while ((chr = this.reader.Peek()) != -1) {
				if (Char.IsNumber((char) chr) || (char) chr == '-' || (char) chr == '.') {
					if (((char) chr) == '.')
						isFloat = true;

					buff.Append((char) this.reader.Read());
				} else
					break;
			}

			this.ReadAway();

			if (isFloat) {
				this.token = JSONToken.Float;
				this.val = Convert.ToDouble(buff.ToString().Replace('.', ','));
			} else
				this.val = Convert.ToInt32(buff.ToString());

			if (this.location == JSONLocation.InObject && !this.needProp)
				this.needProp = true;

			return true;
		}

		private bool ReadBool(char chr) {
			this.token = JSONToken.Boolean;
			this.val = chr == 't';

			if (chr == 't')
				this.ReadAway(3); // rue
			else
				this.ReadAway(4); // alse

			this.ReadAway();

			if (this.location == JSONLocation.InObject && !this.needProp)
				this.needProp = true;

			return true;
		}

		private void ReadAway() {
			int chr;
			
			while ((chr = this.reader.Peek()) != -1) {
				if (chr != ':' && chr != ',' && !Char.IsWhiteSpace((char) chr))
					break;

				this.reader.Read();
			}
		}

		private string ReadLen(int num) {
			StringBuilder buff = new StringBuilder();
			int chr;

			for (int i=0; i<num && (chr = this.reader.Read()) != -1; i++)
				buff.Append((char) chr);

			return buff.ToString();
		}
		
		private void ReadAway(int num) {
			for (int i=0; i<num && this.reader.Read() != -1; i++) ;
		}

		#endregion
	}
}
