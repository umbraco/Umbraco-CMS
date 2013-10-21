/*
 * $Id: JSONWriter.cs 439 2007-11-26 13:26:10Z spocke $
 *
 * Copyright © 2007, Moxiecode Systems AB, All rights reserved. 
 */

using System;
using System.Text;
using System.IO;
using System.Collections;

namespace umbraco.editorControls.tinyMCE3.webcontrol.plugin
{
    /// <summary>
	/// 
	/// </summary>
    [Obsolete("IDataType and all other references to the legacy property editors are no longer used this will be removed from the codebase in future versions")]
    public enum JSONLocation
    {
		/// <summary> </summary>
		InArray,

		/// <summary> </summary>
		InObject,

		/// <summary> </summary>
		Normal
	}

	/// <summary>
	/// Description of JSONWriter.
	/// </summary>
    [Obsolete("IDataType and all other references to the legacy property editors are no longer used this will be removed from the codebase in future versions")]
    public class JSONWriter
    {
		private TextWriter writer;
		private JSONLocation location;
		private Stack lastLocations;
		private int index, lastIndex;
		private bool isProp = false;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="writer"></param>
		public JSONWriter(TextWriter writer) {
			this.writer = writer;
			this.location = JSONLocation.Normal;
			this.lastLocations = new Stack();
			this.index = 0;
			this.lastIndex = 0;
		}

		/// <summary>
		/// 
		/// </summary>
		public void WriteStartObject() {
			this.WriteDelim();
			this.writer.Write('{');
			this.lastLocations.Push(this.location);
			this.lastIndex = this.index;
			this.location = JSONLocation.InObject;
			this.index = 0;
		}

		/// <summary>
		/// 
		/// </summary>
		public void WriteEndObject() {
			this.writer.Write('}');
			this.location = (JSONLocation) lastLocations.Pop();
			this.index = this.lastIndex;
		}

		/// <summary>
		/// 
		/// </summary>
		public void WriteStartArray() {
			this.WriteDelim();
			this.writer.Write('[');
			this.lastLocations.Push(this.location);
			this.lastIndex = this.index;
			this.location = JSONLocation.InArray;
			this.index = 0;
		}

		/// <summary>
		/// 
		/// </summary>
		public void WriteEndArray() {
			this.writer.Write(']');
			this.location = (JSONLocation) lastLocations.Pop();
			this.index = this.lastIndex;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		public void WritePropertyName(string name) {
			this.WriteDelim();
			this.isProp = true;
			this.WriteObject(EncodeString(name));
			this.writer.Write(':');
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <param name="obj"></param>
		public void WriteProperty(string name, object obj) {
			this.WritePropertyName(name);
			this.WriteObject(obj);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="obj"></param>
		public void WriteValue(object obj) {
			this.WriteDelim();
			this.WriteObject(obj);
		}

		/// <summary>
		/// 
		/// </summary>
		public void WriteNull() {
			this.WriteDelim();
			this.writer.Write("null");
		}

		/// <summary>
		/// 
		/// </summary>
		public void Close() {
			this.writer.Close();
		}

		#region private methods

		private void WriteObject(object obj) {
			if (obj == null) {
				this.writer.Write("null");
				return;
			}

			if (obj is bool) {
				this.writer.Write(((bool) obj) == true ? "true" : "false");
				return;
			}

			if (obj is int) {
				this.writer.Write(((int) obj));
				return;
			}

			if (obj is long) {
				this.writer.Write(((long) obj));
				return;
			}

			if (obj is float || obj is double) {
				string str = Convert.ToString(obj);
				this.writer.Write(str.Replace(',', '.'));
				return;
			}

			if (obj is string) {
				this.writer.Write('"');
				this.writer.Write(EncodeString((string) obj));
				this.writer.Write('"');
				return;
			}
		}

		private void WriteDelim() {
			if (this.index > 0 && !this.isProp)
				this.writer.Write(',');

			this.isProp = false;
			this.index++;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="str"></param>
		/// <returns></returns>
		public static string EncodeString(string str) {
			if (str == null)
				return null;

			StringBuilder strBuilder = new StringBuilder(str.Length);
			char[] chars = str.ToCharArray();

			for (int i=0; i<chars.Length; i++) {
				// Unicode it if needed
				if (chars[i] > 128 || Char.IsControl(chars[i])) {
					strBuilder.Append("\\u");
					strBuilder.Append(((int) chars[i]).ToString("X4"));
					continue;
				}

				switch (chars[i]) {
					case '\'':
						strBuilder.Append("\\\'");
						break;

					case '\"':
						strBuilder.Append("\\\"");
						break;

					case '\\':
						strBuilder.Append("\\\\");
						break;

					default:
						strBuilder.Append(chars[i]);
						break;
				}
			}

			return strBuilder.ToString();
		}

		#endregion
	}
}
