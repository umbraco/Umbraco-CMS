#region Using

using System;
using System.IO;
using System.Web;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;

#endregion

namespace umbraco.presentation
{

    /// <summary>
    /// Moved the viewstate
    /// </summary>
    public class viewstateMoverModule : IHttpModule
    {
        #region IHttpModule Members

        void IHttpModule.Dispose()
        {
            // Nothing to dispose; 
        }

        void IHttpModule.Init(HttpApplication context)
        {
            if (UmbracoSettings.UseViewstateMoverModule)
            {
                context.BeginRequest += new EventHandler(context_BeginRequest);
            }
        }

        #endregion

        void context_BeginRequest(object sender, EventArgs e)
        {
            HttpApplication app = sender as HttpApplication;
            if (app.Request.Url.OriginalString.Contains(".aspx"))
            {
                app.Response.Filter = new ViewstateFilter(app.Response.Filter);
            }
        }

        #region Stream filter

        private class ViewstateFilter : Stream
        {

            public ViewstateFilter(Stream sink)
            {
                _sink = sink;
            }

            private Stream _sink;

            //private Hashtable _html = new Hashtable(); 
            private string html = string.Empty;

            #region Properites

            public override bool CanRead
            {
                get { return true; }
            }

            public override bool CanSeek
            {
                get { return true; }
            }

            public override bool CanWrite
            {
                get { return true; }
            }

            public override void Flush()
            {
                doit();
                _sink.Flush();
            }

            public override long Length
            {
                get { return 0; }
            }

            private long _position;
            public override long Position
            {
                get { return _position; }
                set { _position = value; }
            }

            #endregion

            #region Methods

            public override int Read(byte[] buffer, int offset, int count)
            {
                return _sink.Read(buffer, offset, count);
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                return _sink.Seek(offset, origin);
            }

            public override void SetLength(long value)
            {
                _sink.SetLength(value);
            }

            public override void Close()
            {
                _sink.Close();
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                byte[] data = new byte[count];
                Buffer.BlockCopy(buffer, offset, data, 0, count);
                //string html = System.Text.Encoding.Default.GetString(buffer);
                html += System.Text.Encoding.Default.GetString(buffer);
            }
            private void doit()
            {
                int startPoint = html.IndexOf("<input type=\"hidden\" name=\"__VIEWSTATE\"");
                if (startPoint >= 0)
                {
                    int endPoint = html.IndexOf("/>", startPoint) + 2;
                    string viewstateInput = html.Substring(startPoint, endPoint - startPoint);
                    int formEndStart = html.IndexOf("</form>");
                    if (formEndStart >= 0)
                    {
                        html = html.Remove(startPoint, endPoint - startPoint);
                        html = html.Insert(formEndStart - (endPoint - startPoint), viewstateInput);
                    }
                }

                byte[] outdata = System.Text.Encoding.Default.GetBytes(html);
                _sink.Write(outdata, 0, outdata.GetLength(0));
            }

            #endregion

        }

        #endregion
    }

}