using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Xml.Serialization;

namespace umbraco.cms.businesslogic.packager.repositories
{



    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "4.0.30319.1")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Web.Services.WebServiceBindingAttribute(Name = "RepositorySoap", Namespace = "http://packages.umbraco.org/webservices/")]
    public partial class RepositoryWebservice : System.Web.Services.Protocols.SoapHttpClientProtocol
    {

        private System.Threading.SendOrPostCallback CategoriesOperationCompleted;

        private System.Threading.SendOrPostCallback NitrosOperationCompleted;

        private System.Threading.SendOrPostCallback NitrosByVersionOperationCompleted;

        private System.Threading.SendOrPostCallback NitrosCategorizedOperationCompleted;

        private System.Threading.SendOrPostCallback NitrosCategorizedByVersionOperationCompleted;

        private System.Threading.SendOrPostCallback StarterKitsOperationCompleted;

        private System.Threading.SendOrPostCallback StarterKitModulesCategorizedOperationCompleted;

        private System.Threading.SendOrPostCallback StarterKitModulesOperationCompleted;

        private System.Threading.SendOrPostCallback authenticateOperationCompleted;

        private System.Threading.SendOrPostCallback fetchPackageByVersionOperationCompleted;

        private System.Threading.SendOrPostCallback fetchPackageOperationCompleted;

        private System.Threading.SendOrPostCallback fetchProtectedPackageOperationCompleted;

        private System.Threading.SendOrPostCallback SubmitPackageOperationCompleted;

        private System.Threading.SendOrPostCallback PackageByGuidOperationCompleted;

        private System.Threading.SendOrPostCallback SkinByGuidOperationCompleted;

        private System.Threading.SendOrPostCallback SkinsOperationCompleted;

        /// <remarks/>
        public RepositoryWebservice(string url)
        {
            this.Url = url;//"http://our.umbraco.org/umbraco/webservices/api/repository.asmx";
        }

        /// <remarks/>
        public event CategoriesCompletedEventHandler CategoriesCompleted;

        /// <remarks/>
        public event NitrosCompletedEventHandler NitrosCompleted;

        /// <remarks/>
        public event NitrosByVersionCompletedEventHandler NitrosByVersionCompleted;

        /// <remarks/>
        public event NitrosCategorizedCompletedEventHandler NitrosCategorizedCompleted;

        /// <remarks/>
        public event NitrosCategorizedByVersionCompletedEventHandler NitrosCategorizedByVersionCompleted;

        /// <remarks/>
        public event StarterKitsCompletedEventHandler StarterKitsCompleted;

        /// <remarks/>
        public event StarterKitModulesCategorizedCompletedEventHandler StarterKitModulesCategorizedCompleted;

        /// <remarks/>
        public event StarterKitModulesCompletedEventHandler StarterKitModulesCompleted;

        /// <remarks/>
        public event authenticateCompletedEventHandler authenticateCompleted;

        /// <remarks/>
        public event fetchPackageByVersionCompletedEventHandler fetchPackageByVersionCompleted;

        /// <remarks/>
        public event fetchPackageCompletedEventHandler fetchPackageCompleted;

        /// <remarks/>
        public event fetchProtectedPackageCompletedEventHandler fetchProtectedPackageCompleted;

        /// <remarks/>
        public event SubmitPackageCompletedEventHandler SubmitPackageCompleted;

        /// <remarks/>
        public event PackageByGuidCompletedEventHandler PackageByGuidCompleted;

        /// <remarks/>
        public event SkinByGuidCompletedEventHandler SkinByGuidCompleted;

        /// <remarks/>
        public event SkinsCompletedEventHandler SkinsCompleted;

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://packages.umbraco.org/webservices/Categories", RequestNamespace = "http://packages.umbraco.org/webservices/", ResponseNamespace = "http://packages.umbraco.org/webservices/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public Category[] Categories(string repositoryGuid)
        {
            object[] results = this.Invoke("Categories", new object[] {
                    repositoryGuid});
            return ((Category[])(results[0]));
        }

        /// <remarks/>
        public System.IAsyncResult BeginCategories(string repositoryGuid, System.AsyncCallback callback, object asyncState)
        {
            return this.BeginInvoke("Categories", new object[] {
                    repositoryGuid}, callback, asyncState);
        }

        /// <remarks/>
        public Category[] EndCategories(System.IAsyncResult asyncResult)
        {
            object[] results = this.EndInvoke(asyncResult);
            return ((Category[])(results[0]));
        }

        /// <remarks/>
        public void CategoriesAsync(string repositoryGuid)
        {
            this.CategoriesAsync(repositoryGuid, null);
        }

        /// <remarks/>
        public void CategoriesAsync(string repositoryGuid, object userState)
        {
            if ((this.CategoriesOperationCompleted == null))
            {
                this.CategoriesOperationCompleted = new System.Threading.SendOrPostCallback(this.OnCategoriesOperationCompleted);
            }
            this.InvokeAsync("Categories", new object[] {
                    repositoryGuid}, this.CategoriesOperationCompleted, userState);
        }

        private void OnCategoriesOperationCompleted(object arg)
        {
            if ((this.CategoriesCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.CategoriesCompleted(this, new CategoriesCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://packages.umbraco.org/webservices/Nitros", RequestNamespace = "http://packages.umbraco.org/webservices/", ResponseNamespace = "http://packages.umbraco.org/webservices/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public Package[] Nitros()
        {
            object[] results = this.Invoke("Nitros", new object[0]);
            return ((Package[])(results[0]));
        }

        /// <remarks/>
        public System.IAsyncResult BeginNitros(System.AsyncCallback callback, object asyncState)
        {
            return this.BeginInvoke("Nitros", new object[0], callback, asyncState);
        }

        /// <remarks/>
        public Package[] EndNitros(System.IAsyncResult asyncResult)
        {
            object[] results = this.EndInvoke(asyncResult);
            return ((Package[])(results[0]));
        }

        /// <remarks/>
        public void NitrosAsync()
        {
            this.NitrosAsync(null);
        }

        /// <remarks/>
        public void NitrosAsync(object userState)
        {
            if ((this.NitrosOperationCompleted == null))
            {
                this.NitrosOperationCompleted = new System.Threading.SendOrPostCallback(this.OnNitrosOperationCompleted);
            }
            this.InvokeAsync("Nitros", new object[0], this.NitrosOperationCompleted, userState);
        }

        private void OnNitrosOperationCompleted(object arg)
        {
            if ((this.NitrosCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.NitrosCompleted(this, new NitrosCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://packages.umbraco.org/webservices/NitrosByVersion", RequestNamespace = "http://packages.umbraco.org/webservices/", ResponseNamespace = "http://packages.umbraco.org/webservices/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public Package[] NitrosByVersion(Version version)
        {
            object[] results = this.Invoke("NitrosByVersion", new object[] {
                    version});
            return ((Package[])(results[0]));
        }

        /// <remarks/>
        public System.IAsyncResult BeginNitrosByVersion(Version version, System.AsyncCallback callback, object asyncState)
        {
            return this.BeginInvoke("NitrosByVersion", new object[] {
                    version}, callback, asyncState);
        }

        /// <remarks/>
        public Package[] EndNitrosByVersion(System.IAsyncResult asyncResult)
        {
            object[] results = this.EndInvoke(asyncResult);
            return ((Package[])(results[0]));
        }

        /// <remarks/>
        public void NitrosByVersionAsync(Version version)
        {
            this.NitrosByVersionAsync(version, null);
        }

        /// <remarks/>
        public void NitrosByVersionAsync(Version version, object userState)
        {
            if ((this.NitrosByVersionOperationCompleted == null))
            {
                this.NitrosByVersionOperationCompleted = new System.Threading.SendOrPostCallback(this.OnNitrosByVersionOperationCompleted);
            }
            this.InvokeAsync("NitrosByVersion", new object[] {
                    version}, this.NitrosByVersionOperationCompleted, userState);
        }

        private void OnNitrosByVersionOperationCompleted(object arg)
        {
            if ((this.NitrosByVersionCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.NitrosByVersionCompleted(this, new NitrosByVersionCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://packages.umbraco.org/webservices/NitrosCategorized", RequestNamespace = "http://packages.umbraco.org/webservices/", ResponseNamespace = "http://packages.umbraco.org/webservices/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public Category[] NitrosCategorized()
        {
            object[] results = this.Invoke("NitrosCategorized", new object[0]);
            return ((Category[])(results[0]));
        }

        /// <remarks/>
        public System.IAsyncResult BeginNitrosCategorized(System.AsyncCallback callback, object asyncState)
        {
            return this.BeginInvoke("NitrosCategorized", new object[0], callback, asyncState);
        }

        /// <remarks/>
        public Category[] EndNitrosCategorized(System.IAsyncResult asyncResult)
        {
            object[] results = this.EndInvoke(asyncResult);
            return ((Category[])(results[0]));
        }

        /// <remarks/>
        public void NitrosCategorizedAsync()
        {
            this.NitrosCategorizedAsync(null);
        }

        /// <remarks/>
        public void NitrosCategorizedAsync(object userState)
        {
            if ((this.NitrosCategorizedOperationCompleted == null))
            {
                this.NitrosCategorizedOperationCompleted = new System.Threading.SendOrPostCallback(this.OnNitrosCategorizedOperationCompleted);
            }
            this.InvokeAsync("NitrosCategorized", new object[0], this.NitrosCategorizedOperationCompleted, userState);
        }

        private void OnNitrosCategorizedOperationCompleted(object arg)
        {
            if ((this.NitrosCategorizedCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.NitrosCategorizedCompleted(this, new NitrosCategorizedCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://packages.umbraco.org/webservices/NitrosCategorizedByVersion", RequestNamespace = "http://packages.umbraco.org/webservices/", ResponseNamespace = "http://packages.umbraco.org/webservices/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public Category[] NitrosCategorizedByVersion(Version version)
        {
            object[] results = this.Invoke("NitrosCategorizedByVersion", new object[] {
                    version});
            return ((Category[])(results[0]));
        }

        /// <remarks/>
        public System.IAsyncResult BeginNitrosCategorizedByVersion(Version version, System.AsyncCallback callback, object asyncState)
        {
            return this.BeginInvoke("NitrosCategorizedByVersion", new object[] {
                    version}, callback, asyncState);
        }

        /// <remarks/>
        public Category[] EndNitrosCategorizedByVersion(System.IAsyncResult asyncResult)
        {
            object[] results = this.EndInvoke(asyncResult);
            return ((Category[])(results[0]));
        }

        /// <remarks/>
        public void NitrosCategorizedByVersionAsync(Version version)
        {
            this.NitrosCategorizedByVersionAsync(version, null);
        }

        /// <remarks/>
        public void NitrosCategorizedByVersionAsync(Version version, object userState)
        {
            if ((this.NitrosCategorizedByVersionOperationCompleted == null))
            {
                this.NitrosCategorizedByVersionOperationCompleted = new System.Threading.SendOrPostCallback(this.OnNitrosCategorizedByVersionOperationCompleted);
            }
            this.InvokeAsync("NitrosCategorizedByVersion", new object[] {
                    version}, this.NitrosCategorizedByVersionOperationCompleted, userState);
        }

        private void OnNitrosCategorizedByVersionOperationCompleted(object arg)
        {
            if ((this.NitrosCategorizedByVersionCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.NitrosCategorizedByVersionCompleted(this, new NitrosCategorizedByVersionCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://packages.umbraco.org/webservices/StarterKits", RequestNamespace = "http://packages.umbraco.org/webservices/", ResponseNamespace = "http://packages.umbraco.org/webservices/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public Package[] StarterKits()
        {
            object[] results = this.Invoke("StarterKits", new object[0]);
            return ((Package[])(results[0]));
        }

        /// <remarks/>
        public System.IAsyncResult BeginStarterKits(System.AsyncCallback callback, object asyncState)
        {
            return this.BeginInvoke("StarterKits", new object[0], callback, asyncState);
        }

        /// <remarks/>
        public Package[] EndStarterKits(System.IAsyncResult asyncResult)
        {
            object[] results = this.EndInvoke(asyncResult);
            return ((Package[])(results[0]));
        }

        /// <remarks/>
        public void StarterKitsAsync()
        {
            this.StarterKitsAsync(null);
        }

        /// <remarks/>
        public void StarterKitsAsync(object userState)
        {
            if ((this.StarterKitsOperationCompleted == null))
            {
                this.StarterKitsOperationCompleted = new System.Threading.SendOrPostCallback(this.OnStarterKitsOperationCompleted);
            }
            this.InvokeAsync("StarterKits", new object[0], this.StarterKitsOperationCompleted, userState);
        }

        private void OnStarterKitsOperationCompleted(object arg)
        {
            if ((this.StarterKitsCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.StarterKitsCompleted(this, new StarterKitsCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://packages.umbraco.org/webservices/StarterKitModulesCategorized", RequestNamespace = "http://packages.umbraco.org/webservices/", ResponseNamespace = "http://packages.umbraco.org/webservices/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public Category[] StarterKitModulesCategorized()
        {
            object[] results = this.Invoke("StarterKitModulesCategorized", new object[0]);
            return ((Category[])(results[0]));
        }

        /// <remarks/>
        public System.IAsyncResult BeginStarterKitModulesCategorized(System.AsyncCallback callback, object asyncState)
        {
            return this.BeginInvoke("StarterKitModulesCategorized", new object[0], callback, asyncState);
        }

        /// <remarks/>
        public Category[] EndStarterKitModulesCategorized(System.IAsyncResult asyncResult)
        {
            object[] results = this.EndInvoke(asyncResult);
            return ((Category[])(results[0]));
        }

        /// <remarks/>
        public void StarterKitModulesCategorizedAsync()
        {
            this.StarterKitModulesCategorizedAsync(null);
        }

        /// <remarks/>
        public void StarterKitModulesCategorizedAsync(object userState)
        {
            if ((this.StarterKitModulesCategorizedOperationCompleted == null))
            {
                this.StarterKitModulesCategorizedOperationCompleted = new System.Threading.SendOrPostCallback(this.OnStarterKitModulesCategorizedOperationCompleted);
            }
            this.InvokeAsync("StarterKitModulesCategorized", new object[0], this.StarterKitModulesCategorizedOperationCompleted, userState);
        }

        private void OnStarterKitModulesCategorizedOperationCompleted(object arg)
        {
            if ((this.StarterKitModulesCategorizedCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.StarterKitModulesCategorizedCompleted(this, new StarterKitModulesCategorizedCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://packages.umbraco.org/webservices/StarterKitModules", RequestNamespace = "http://packages.umbraco.org/webservices/", ResponseNamespace = "http://packages.umbraco.org/webservices/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public Package[] StarterKitModules()
        {
            object[] results = this.Invoke("StarterKitModules", new object[0]);
            return ((Package[])(results[0]));
        }

        /// <remarks/>
        public System.IAsyncResult BeginStarterKitModules(System.AsyncCallback callback, object asyncState)
        {
            return this.BeginInvoke("StarterKitModules", new object[0], callback, asyncState);
        }

        /// <remarks/>
        public Package[] EndStarterKitModules(System.IAsyncResult asyncResult)
        {
            object[] results = this.EndInvoke(asyncResult);
            return ((Package[])(results[0]));
        }

        /// <remarks/>
        public void StarterKitModulesAsync()
        {
            this.StarterKitModulesAsync(null);
        }

        /// <remarks/>
        public void StarterKitModulesAsync(object userState)
        {
            if ((this.StarterKitModulesOperationCompleted == null))
            {
                this.StarterKitModulesOperationCompleted = new System.Threading.SendOrPostCallback(this.OnStarterKitModulesOperationCompleted);
            }
            this.InvokeAsync("StarterKitModules", new object[0], this.StarterKitModulesOperationCompleted, userState);
        }

        private void OnStarterKitModulesOperationCompleted(object arg)
        {
            if ((this.StarterKitModulesCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.StarterKitModulesCompleted(this, new StarterKitModulesCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://packages.umbraco.org/webservices/authenticate", RequestNamespace = "http://packages.umbraco.org/webservices/", ResponseNamespace = "http://packages.umbraco.org/webservices/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public string authenticate(string email, string md5Password)
        {
            object[] results = this.Invoke("authenticate", new object[] {
                    email,
                    md5Password});
            return ((string)(results[0]));
        }

        /// <remarks/>
        public System.IAsyncResult Beginauthenticate(string email, string md5Password, System.AsyncCallback callback, object asyncState)
        {
            return this.BeginInvoke("authenticate", new object[] {
                    email,
                    md5Password}, callback, asyncState);
        }

        /// <remarks/>
        public string Endauthenticate(System.IAsyncResult asyncResult)
        {
            object[] results = this.EndInvoke(asyncResult);
            return ((string)(results[0]));
        }

        /// <remarks/>
        public void authenticateAsync(string email, string md5Password)
        {
            this.authenticateAsync(email, md5Password, null);
        }

        /// <remarks/>
        public void authenticateAsync(string email, string md5Password, object userState)
        {
            if ((this.authenticateOperationCompleted == null))
            {
                this.authenticateOperationCompleted = new System.Threading.SendOrPostCallback(this.OnauthenticateOperationCompleted);
            }
            this.InvokeAsync("authenticate", new object[] {
                    email,
                    md5Password}, this.authenticateOperationCompleted, userState);
        }

        private void OnauthenticateOperationCompleted(object arg)
        {
            if ((this.authenticateCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.authenticateCompleted(this, new authenticateCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://packages.umbraco.org/webservices/fetchPackageByVersion", RequestNamespace = "http://packages.umbraco.org/webservices/", ResponseNamespace = "http://packages.umbraco.org/webservices/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        [return: System.Xml.Serialization.XmlElementAttribute(DataType = "base64Binary")]
        public byte[] fetchPackageByVersion(string packageGuid, Version schemaVersion)
        {
            object[] results = this.Invoke("fetchPackageByVersion", new object[] {
                    packageGuid,
                    schemaVersion});
            return ((byte[])(results[0]));
        }

        /// <remarks/>
        public System.IAsyncResult BeginfetchPackageByVersion(string packageGuid, Version schemaVersion, System.AsyncCallback callback, object asyncState)
        {
            return this.BeginInvoke("fetchPackageByVersion", new object[] {
                    packageGuid,
                    schemaVersion}, callback, asyncState);
        }

        /// <remarks/>
        public byte[] EndfetchPackageByVersion(System.IAsyncResult asyncResult)
        {
            object[] results = this.EndInvoke(asyncResult);
            return ((byte[])(results[0]));
        }

        /// <remarks/>
        public void fetchPackageByVersionAsync(string packageGuid, Version schemaVersion)
        {
            this.fetchPackageByVersionAsync(packageGuid, schemaVersion, null);
        }

        /// <remarks/>
        public void fetchPackageByVersionAsync(string packageGuid, Version schemaVersion, object userState)
        {
            if ((this.fetchPackageByVersionOperationCompleted == null))
            {
                this.fetchPackageByVersionOperationCompleted = new System.Threading.SendOrPostCallback(this.OnfetchPackageByVersionOperationCompleted);
            }
            this.InvokeAsync("fetchPackageByVersion", new object[] {
                    packageGuid,
                    schemaVersion}, this.fetchPackageByVersionOperationCompleted, userState);
        }

        private void OnfetchPackageByVersionOperationCompleted(object arg)
        {
            if ((this.fetchPackageByVersionCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.fetchPackageByVersionCompleted(this, new fetchPackageByVersionCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://packages.umbraco.org/webservices/fetchPackage", RequestNamespace = "http://packages.umbraco.org/webservices/", ResponseNamespace = "http://packages.umbraco.org/webservices/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        [return: System.Xml.Serialization.XmlElementAttribute(DataType = "base64Binary")]
        public byte[] fetchPackage(string packageGuid)
        {
            object[] results = this.Invoke("fetchPackage", new object[] {
                    packageGuid});
            return ((byte[])(results[0]));
        }

        /// <remarks/>
        public System.IAsyncResult BeginfetchPackage(string packageGuid, System.AsyncCallback callback, object asyncState)
        {
            return this.BeginInvoke("fetchPackage", new object[] {
                    packageGuid}, callback, asyncState);
        }

        /// <remarks/>
        public byte[] EndfetchPackage(System.IAsyncResult asyncResult)
        {
            object[] results = this.EndInvoke(asyncResult);
            return ((byte[])(results[0]));
        }

        /// <remarks/>
        public void fetchPackageAsync(string packageGuid)
        {
            this.fetchPackageAsync(packageGuid, null);
        }

        /// <remarks/>
        public void fetchPackageAsync(string packageGuid, object userState)
        {
            if ((this.fetchPackageOperationCompleted == null))
            {
                this.fetchPackageOperationCompleted = new System.Threading.SendOrPostCallback(this.OnfetchPackageOperationCompleted);
            }
            this.InvokeAsync("fetchPackage", new object[] {
                    packageGuid}, this.fetchPackageOperationCompleted, userState);
        }

        private void OnfetchPackageOperationCompleted(object arg)
        {
            if ((this.fetchPackageCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.fetchPackageCompleted(this, new fetchPackageCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://packages.umbraco.org/webservices/fetchProtectedPackage", RequestNamespace = "http://packages.umbraco.org/webservices/", ResponseNamespace = "http://packages.umbraco.org/webservices/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        [return: System.Xml.Serialization.XmlElementAttribute(DataType = "base64Binary")]
        public byte[] fetchProtectedPackage(string packageGuid, string memberKey)
        {
            object[] results = this.Invoke("fetchProtectedPackage", new object[] {
                    packageGuid,
                    memberKey});
            return ((byte[])(results[0]));
        }

        /// <remarks/>
        public System.IAsyncResult BeginfetchProtectedPackage(string packageGuid, string memberKey, System.AsyncCallback callback, object asyncState)
        {
            return this.BeginInvoke("fetchProtectedPackage", new object[] {
                    packageGuid,
                    memberKey}, callback, asyncState);
        }

        /// <remarks/>
        public byte[] EndfetchProtectedPackage(System.IAsyncResult asyncResult)
        {
            object[] results = this.EndInvoke(asyncResult);
            return ((byte[])(results[0]));
        }

        /// <remarks/>
        public void fetchProtectedPackageAsync(string packageGuid, string memberKey)
        {
            this.fetchProtectedPackageAsync(packageGuid, memberKey, null);
        }

        /// <remarks/>
        public void fetchProtectedPackageAsync(string packageGuid, string memberKey, object userState)
        {
            if ((this.fetchProtectedPackageOperationCompleted == null))
            {
                this.fetchProtectedPackageOperationCompleted = new System.Threading.SendOrPostCallback(this.OnfetchProtectedPackageOperationCompleted);
            }
            this.InvokeAsync("fetchProtectedPackage", new object[] {
                    packageGuid,
                    memberKey}, this.fetchProtectedPackageOperationCompleted, userState);
        }

        private void OnfetchProtectedPackageOperationCompleted(object arg)
        {
            if ((this.fetchProtectedPackageCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.fetchProtectedPackageCompleted(this, new fetchProtectedPackageCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://packages.umbraco.org/webservices/SubmitPackage", RequestNamespace = "http://packages.umbraco.org/webservices/", ResponseNamespace = "http://packages.umbraco.org/webservices/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public SubmitStatus SubmitPackage(string repositoryGuid, string authorGuid, string packageGuid, [System.Xml.Serialization.XmlElementAttribute(DataType = "base64Binary")] byte[] packageFile, [System.Xml.Serialization.XmlElementAttribute(DataType = "base64Binary")] byte[] packageDoc, [System.Xml.Serialization.XmlElementAttribute(DataType = "base64Binary")] byte[] packageThumbnail, string name, string author, string authorUrl, string description)
        {
            object[] results = this.Invoke("SubmitPackage", new object[] {
                    repositoryGuid,
                    authorGuid,
                    packageGuid,
                    packageFile,
                    packageDoc,
                    packageThumbnail,
                    name,
                    author,
                    authorUrl,
                    description});
            return ((SubmitStatus)(results[0]));
        }

        /// <remarks/>
        public System.IAsyncResult BeginSubmitPackage(string repositoryGuid, string authorGuid, string packageGuid, byte[] packageFile, byte[] packageDoc, byte[] packageThumbnail, string name, string author, string authorUrl, string description, System.AsyncCallback callback, object asyncState)
        {
            return this.BeginInvoke("SubmitPackage", new object[] {
                    repositoryGuid,
                    authorGuid,
                    packageGuid,
                    packageFile,
                    packageDoc,
                    packageThumbnail,
                    name,
                    author,
                    authorUrl,
                    description}, callback, asyncState);
        }

        /// <remarks/>
        public SubmitStatus EndSubmitPackage(System.IAsyncResult asyncResult)
        {
            object[] results = this.EndInvoke(asyncResult);
            return ((SubmitStatus)(results[0]));
        }

        /// <remarks/>
        public void SubmitPackageAsync(string repositoryGuid, string authorGuid, string packageGuid, byte[] packageFile, byte[] packageDoc, byte[] packageThumbnail, string name, string author, string authorUrl, string description)
        {
            this.SubmitPackageAsync(repositoryGuid, authorGuid, packageGuid, packageFile, packageDoc, packageThumbnail, name, author, authorUrl, description, null);
        }

        /// <remarks/>
        public void SubmitPackageAsync(string repositoryGuid, string authorGuid, string packageGuid, byte[] packageFile, byte[] packageDoc, byte[] packageThumbnail, string name, string author, string authorUrl, string description, object userState)
        {
            if ((this.SubmitPackageOperationCompleted == null))
            {
                this.SubmitPackageOperationCompleted = new System.Threading.SendOrPostCallback(this.OnSubmitPackageOperationCompleted);
            }
            this.InvokeAsync("SubmitPackage", new object[] {
                    repositoryGuid,
                    authorGuid,
                    packageGuid,
                    packageFile,
                    packageDoc,
                    packageThumbnail,
                    name,
                    author,
                    authorUrl,
                    description}, this.SubmitPackageOperationCompleted, userState);
        }

        private void OnSubmitPackageOperationCompleted(object arg)
        {
            if ((this.SubmitPackageCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.SubmitPackageCompleted(this, new SubmitPackageCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://packages.umbraco.org/webservices/PackageByGuid", RequestNamespace = "http://packages.umbraco.org/webservices/", ResponseNamespace = "http://packages.umbraco.org/webservices/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public Package PackageByGuid(string packageGuid)
        {
            object[] results = this.Invoke("PackageByGuid", new object[] {
                    packageGuid});
            return ((Package)(results[0]));
        }

        /// <remarks/>
        public System.IAsyncResult BeginPackageByGuid(string packageGuid, System.AsyncCallback callback, object asyncState)
        {
            return this.BeginInvoke("PackageByGuid", new object[] {
                    packageGuid}, callback, asyncState);
        }

        /// <remarks/>
        public Package EndPackageByGuid(System.IAsyncResult asyncResult)
        {
            object[] results = this.EndInvoke(asyncResult);
            return ((Package)(results[0]));
        }

        /// <remarks/>
        public void PackageByGuidAsync(string packageGuid)
        {
            this.PackageByGuidAsync(packageGuid, null);
        }

        /// <remarks/>
        public void PackageByGuidAsync(string packageGuid, object userState)
        {
            if ((this.PackageByGuidOperationCompleted == null))
            {
                this.PackageByGuidOperationCompleted = new System.Threading.SendOrPostCallback(this.OnPackageByGuidOperationCompleted);
            }
            this.InvokeAsync("PackageByGuid", new object[] {
                    packageGuid}, this.PackageByGuidOperationCompleted, userState);
        }

        private void OnPackageByGuidOperationCompleted(object arg)
        {
            if ((this.PackageByGuidCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.PackageByGuidCompleted(this, new PackageByGuidCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://packages.umbraco.org/webservices/SkinByGuid", RequestNamespace = "http://packages.umbraco.org/webservices/", ResponseNamespace = "http://packages.umbraco.org/webservices/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public Skin SkinByGuid(string skinGuid)
        {
            object[] results = this.Invoke("SkinByGuid", new object[] {
                    skinGuid});
            return ((Skin)(results[0]));
        }

        /// <remarks/>
        public System.IAsyncResult BeginSkinByGuid(string skinGuid, System.AsyncCallback callback, object asyncState)
        {
            return this.BeginInvoke("SkinByGuid", new object[] {
                    skinGuid}, callback, asyncState);
        }

        /// <remarks/>
        public Skin EndSkinByGuid(System.IAsyncResult asyncResult)
        {
            object[] results = this.EndInvoke(asyncResult);
            return ((Skin)(results[0]));
        }

        /// <remarks/>
        public void SkinByGuidAsync(string skinGuid)
        {
            this.SkinByGuidAsync(skinGuid, null);
        }

        /// <remarks/>
        public void SkinByGuidAsync(string skinGuid, object userState)
        {
            if ((this.SkinByGuidOperationCompleted == null))
            {
                this.SkinByGuidOperationCompleted = new System.Threading.SendOrPostCallback(this.OnSkinByGuidOperationCompleted);
            }
            this.InvokeAsync("SkinByGuid", new object[] {
                    skinGuid}, this.SkinByGuidOperationCompleted, userState);
        }

        private void OnSkinByGuidOperationCompleted(object arg)
        {
            if ((this.SkinByGuidCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.SkinByGuidCompleted(this, new SkinByGuidCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://packages.umbraco.org/webservices/Skins", RequestNamespace = "http://packages.umbraco.org/webservices/", ResponseNamespace = "http://packages.umbraco.org/webservices/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public Skin[] Skins(string starterKitGuid)
        {
            object[] results = this.Invoke("Skins", new object[] {
                    starterKitGuid});
            return ((Skin[])(results[0]));
        }

        /// <remarks/>
        public System.IAsyncResult BeginSkins(string starterKitGuid, System.AsyncCallback callback, object asyncState)
        {
            return this.BeginInvoke("Skins", new object[] {
                    starterKitGuid}, callback, asyncState);
        }

        /// <remarks/>
        public Skin[] EndSkins(System.IAsyncResult asyncResult)
        {
            object[] results = this.EndInvoke(asyncResult);
            return ((Skin[])(results[0]));
        }

        /// <remarks/>
        public void SkinsAsync(string starterKitGuid)
        {
            this.SkinsAsync(starterKitGuid, null);
        }

        /// <remarks/>
        public void SkinsAsync(string starterKitGuid, object userState)
        {
            if ((this.SkinsOperationCompleted == null))
            {
                this.SkinsOperationCompleted = new System.Threading.SendOrPostCallback(this.OnSkinsOperationCompleted);
            }
            this.InvokeAsync("Skins", new object[] {
                    starterKitGuid}, this.SkinsOperationCompleted, userState);
        }

        private void OnSkinsOperationCompleted(object arg)
        {
            if ((this.SkinsCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.SkinsCompleted(this, new SkinsCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        public new void CancelAsync(object userState)
        {
            base.CancelAsync(userState);
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://packages.umbraco.org/webservices/")]
    public partial class Category
    {

        private string textField;

        private string descriptionField;

        private string urlField;

        private int idField;

        private Package[] packagesField;

        /// <remarks/>
        public string Text
        {
            get
            {
                return this.textField;
            }
            set
            {
                this.textField = value;
            }
        }

        /// <remarks/>
        public string Description
        {
            get
            {
                return this.descriptionField;
            }
            set
            {
                this.descriptionField = value;
            }
        }

        /// <remarks/>
        public string Url
        {
            get
            {
                return this.urlField;
            }
            set
            {
                this.urlField = value;
            }
        }

        /// <remarks/>
        public int Id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        /// <remarks/>
        public Package[] Packages
        {
            get
            {
                return this.packagesField;
            }
            set
            {
                this.packagesField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://packages.umbraco.org/webservices/")]
    public partial class Package
    {

        private System.Guid repoGuidField;

        private string textField;

        private string descriptionField;

        private string iconField;

        private string thumbnailField;

        private string documentationField;

        private string demoField;

        private bool acceptedField;

        private bool editorsPickField;

        private bool protectedField;

        private bool hasUpgradeField;

        private string upgradeVersionField;

        private string upgradeReadMeField;

        private string urlField;

        /// <remarks/>
        public System.Guid RepoGuid
        {
            get
            {
                return this.repoGuidField;
            }
            set
            {
                this.repoGuidField = value;
            }
        }

        /// <remarks/>
        public string Text
        {
            get
            {
                return this.textField;
            }
            set
            {
                this.textField = value;
            }
        }

        /// <remarks/>
        public string Description
        {
            get
            {
                return this.descriptionField;
            }
            set
            {
                this.descriptionField = value;
            }
        }

        /// <remarks/>
        public string Icon
        {
            get
            {
                return this.iconField;
            }
            set
            {
                this.iconField = value;
            }
        }

        /// <remarks/>
        public string Thumbnail
        {
            get
            {
                return this.thumbnailField;
            }
            set
            {
                this.thumbnailField = value;
            }
        }

        /// <remarks/>
        public string Documentation
        {
            get
            {
                return this.documentationField;
            }
            set
            {
                this.documentationField = value;
            }
        }

        /// <remarks/>
        public string Demo
        {
            get
            {
                return this.demoField;
            }
            set
            {
                this.demoField = value;
            }
        }

        /// <remarks/>
        public bool Accepted
        {
            get
            {
                return this.acceptedField;
            }
            set
            {
                this.acceptedField = value;
            }
        }

        /// <remarks/>
        public bool EditorsPick
        {
            get
            {
                return this.editorsPickField;
            }
            set
            {
                this.editorsPickField = value;
            }
        }

        /// <remarks/>
        public bool Protected
        {
            get
            {
                return this.protectedField;
            }
            set
            {
                this.protectedField = value;
            }
        }

        /// <remarks/>
        public bool HasUpgrade
        {
            get
            {
                return this.hasUpgradeField;
            }
            set
            {
                this.hasUpgradeField = value;
            }
        }

        /// <remarks/>
        public string UpgradeVersion
        {
            get
            {
                return this.upgradeVersionField;
            }
            set
            {
                this.upgradeVersionField = value;
            }
        }

        /// <remarks/>
        public string UpgradeReadMe
        {
            get
            {
                return this.upgradeReadMeField;
            }
            set
            {
                this.upgradeReadMeField = value;
            }
        }

        /// <remarks/>
        public string Url
        {
            get
            {
                return this.urlField;
            }
            set
            {
                this.urlField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://packages.umbraco.org/webservices/")]
    public partial class Skin
    {

        private System.Guid repoGuidField;

        private string textField;

        private string thumbnailField;

        private string previewField;

        private string descriptionField;

        private string authorField;

        private string authorUrlField;

        private string licenseField;

        private string licenseUrlField;

        /// <remarks/>
        public System.Guid RepoGuid
        {
            get
            {
                return this.repoGuidField;
            }
            set
            {
                this.repoGuidField = value;
            }
        }

        /// <remarks/>
        public string Text
        {
            get
            {
                return this.textField;
            }
            set
            {
                this.textField = value;
            }
        }

        /// <remarks/>
        public string Thumbnail
        {
            get
            {
                return this.thumbnailField;
            }
            set
            {
                this.thumbnailField = value;
            }
        }

        /// <remarks/>
        public string Preview
        {
            get
            {
                return this.previewField;
            }
            set
            {
                this.previewField = value;
            }
        }

        /// <remarks/>
        public string Description
        {
            get
            {
                return this.descriptionField;
            }
            set
            {
                this.descriptionField = value;
            }
        }

        /// <remarks/>
        public string Author
        {
            get
            {
                return this.authorField;
            }
            set
            {
                this.authorField = value;
            }
        }

        /// <remarks/>
        public string AuthorUrl
        {
            get
            {
                return this.authorUrlField;
            }
            set
            {
                this.authorUrlField = value;
            }
        }

        /// <remarks/>
        public string License
        {
            get
            {
                return this.licenseField;
            }
            set
            {
                this.licenseField = value;
            }
        }

        /// <remarks/>
        public string LicenseUrl
        {
            get
            {
                return this.licenseUrlField;
            }
            set
            {
                this.licenseUrlField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://packages.umbraco.org/webservices/")]
    public enum Version
    {

        /// <remarks/>
        Version3,

        /// <remarks/>
        Version4,

        /// <remarks/>
        Version41,

        /// <remarks/>
        Version41Legacy,

        /// <remarks/>
        Version50,
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://packages.umbraco.org/webservices/")]
    public enum SubmitStatus
    {

        /// <remarks/>
        Complete,

        /// <remarks/>
        Exists,

        /// <remarks/>
        NoAccess,

        /// <remarks/>
        Error,
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "4.0.30319.1")]
    public delegate void CategoriesCompletedEventHandler(object sender, CategoriesCompletedEventArgs e);

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "4.0.30319.1")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class CategoriesCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
    {

        private object[] results;

        internal CategoriesCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
            base(exception, cancelled, userState)
        {
            this.results = results;
        }

        /// <remarks/>
        public Category[] Result
        {
            get
            {
                this.RaiseExceptionIfNecessary();
                return ((Category[])(this.results[0]));
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "4.0.30319.1")]
    public delegate void NitrosCompletedEventHandler(object sender, NitrosCompletedEventArgs e);

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "4.0.30319.1")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class NitrosCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
    {

        private object[] results;

        internal NitrosCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
            base(exception, cancelled, userState)
        {
            this.results = results;
        }

        /// <remarks/>
        public Package[] Result
        {
            get
            {
                this.RaiseExceptionIfNecessary();
                return ((Package[])(this.results[0]));
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "4.0.30319.1")]
    public delegate void NitrosByVersionCompletedEventHandler(object sender, NitrosByVersionCompletedEventArgs e);

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "4.0.30319.1")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class NitrosByVersionCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
    {

        private object[] results;

        internal NitrosByVersionCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
            base(exception, cancelled, userState)
        {
            this.results = results;
        }

        /// <remarks/>
        public Package[] Result
        {
            get
            {
                this.RaiseExceptionIfNecessary();
                return ((Package[])(this.results[0]));
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "4.0.30319.1")]
    public delegate void NitrosCategorizedCompletedEventHandler(object sender, NitrosCategorizedCompletedEventArgs e);

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "4.0.30319.1")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class NitrosCategorizedCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
    {

        private object[] results;

        internal NitrosCategorizedCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
            base(exception, cancelled, userState)
        {
            this.results = results;
        }

        /// <remarks/>
        public Category[] Result
        {
            get
            {
                this.RaiseExceptionIfNecessary();
                return ((Category[])(this.results[0]));
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "4.0.30319.1")]
    public delegate void NitrosCategorizedByVersionCompletedEventHandler(object sender, NitrosCategorizedByVersionCompletedEventArgs e);

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "4.0.30319.1")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class NitrosCategorizedByVersionCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
    {

        private object[] results;

        internal NitrosCategorizedByVersionCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
            base(exception, cancelled, userState)
        {
            this.results = results;
        }

        /// <remarks/>
        public Category[] Result
        {
            get
            {
                this.RaiseExceptionIfNecessary();
                return ((Category[])(this.results[0]));
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "4.0.30319.1")]
    public delegate void StarterKitsCompletedEventHandler(object sender, StarterKitsCompletedEventArgs e);

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "4.0.30319.1")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class StarterKitsCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
    {

        private object[] results;

        internal StarterKitsCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
            base(exception, cancelled, userState)
        {
            this.results = results;
        }

        /// <remarks/>
        public Package[] Result
        {
            get
            {
                this.RaiseExceptionIfNecessary();
                return ((Package[])(this.results[0]));
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "4.0.30319.1")]
    public delegate void StarterKitModulesCategorizedCompletedEventHandler(object sender, StarterKitModulesCategorizedCompletedEventArgs e);

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "4.0.30319.1")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class StarterKitModulesCategorizedCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
    {

        private object[] results;

        internal StarterKitModulesCategorizedCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
            base(exception, cancelled, userState)
        {
            this.results = results;
        }

        /// <remarks/>
        public Category[] Result
        {
            get
            {
                this.RaiseExceptionIfNecessary();
                return ((Category[])(this.results[0]));
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "4.0.30319.1")]
    public delegate void StarterKitModulesCompletedEventHandler(object sender, StarterKitModulesCompletedEventArgs e);

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "4.0.30319.1")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class StarterKitModulesCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
    {

        private object[] results;

        internal StarterKitModulesCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
            base(exception, cancelled, userState)
        {
            this.results = results;
        }

        /// <remarks/>
        public Package[] Result
        {
            get
            {
                this.RaiseExceptionIfNecessary();
                return ((Package[])(this.results[0]));
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "4.0.30319.1")]
    public delegate void authenticateCompletedEventHandler(object sender, authenticateCompletedEventArgs e);

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "4.0.30319.1")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class authenticateCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
    {

        private object[] results;

        internal authenticateCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
            base(exception, cancelled, userState)
        {
            this.results = results;
        }

        /// <remarks/>
        public string Result
        {
            get
            {
                this.RaiseExceptionIfNecessary();
                return ((string)(this.results[0]));
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "4.0.30319.1")]
    public delegate void fetchPackageByVersionCompletedEventHandler(object sender, fetchPackageByVersionCompletedEventArgs e);

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "4.0.30319.1")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class fetchPackageByVersionCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
    {

        private object[] results;

        internal fetchPackageByVersionCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
            base(exception, cancelled, userState)
        {
            this.results = results;
        }

        /// <remarks/>
        public byte[] Result
        {
            get
            {
                this.RaiseExceptionIfNecessary();
                return ((byte[])(this.results[0]));
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "4.0.30319.1")]
    public delegate void fetchPackageCompletedEventHandler(object sender, fetchPackageCompletedEventArgs e);

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "4.0.30319.1")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class fetchPackageCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
    {

        private object[] results;

        internal fetchPackageCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
            base(exception, cancelled, userState)
        {
            this.results = results;
        }

        /// <remarks/>
        public byte[] Result
        {
            get
            {
                this.RaiseExceptionIfNecessary();
                return ((byte[])(this.results[0]));
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "4.0.30319.1")]
    public delegate void fetchProtectedPackageCompletedEventHandler(object sender, fetchProtectedPackageCompletedEventArgs e);

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "4.0.30319.1")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class fetchProtectedPackageCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
    {

        private object[] results;

        internal fetchProtectedPackageCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
            base(exception, cancelled, userState)
        {
            this.results = results;
        }

        /// <remarks/>
        public byte[] Result
        {
            get
            {
                this.RaiseExceptionIfNecessary();
                return ((byte[])(this.results[0]));
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "4.0.30319.1")]
    public delegate void SubmitPackageCompletedEventHandler(object sender, SubmitPackageCompletedEventArgs e);

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "4.0.30319.1")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class SubmitPackageCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
    {

        private object[] results;

        internal SubmitPackageCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
            base(exception, cancelled, userState)
        {
            this.results = results;
        }

        /// <remarks/>
        public SubmitStatus Result
        {
            get
            {
                this.RaiseExceptionIfNecessary();
                return ((SubmitStatus)(this.results[0]));
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "4.0.30319.1")]
    public delegate void PackageByGuidCompletedEventHandler(object sender, PackageByGuidCompletedEventArgs e);

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "4.0.30319.1")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class PackageByGuidCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
    {

        private object[] results;

        internal PackageByGuidCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
            base(exception, cancelled, userState)
        {
            this.results = results;
        }

        /// <remarks/>
        public Package Result
        {
            get
            {
                this.RaiseExceptionIfNecessary();
                return ((Package)(this.results[0]));
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "4.0.30319.1")]
    public delegate void SkinByGuidCompletedEventHandler(object sender, SkinByGuidCompletedEventArgs e);

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "4.0.30319.1")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class SkinByGuidCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
    {

        private object[] results;

        internal SkinByGuidCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
            base(exception, cancelled, userState)
        {
            this.results = results;
        }

        /// <remarks/>
        public Skin Result
        {
            get
            {
                this.RaiseExceptionIfNecessary();
                return ((Skin)(this.results[0]));
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "4.0.30319.1")]
    public delegate void SkinsCompletedEventHandler(object sender, SkinsCompletedEventArgs e);

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "4.0.30319.1")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class SkinsCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
    {

        private object[] results;

        internal SkinsCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
            base(exception, cancelled, userState)
        {
            this.results = results;
        }

        /// <remarks/>
        public Skin[] Result
        {
            get
            {
                this.RaiseExceptionIfNecessary();
                return ((Skin[])(this.results[0]));
            }
        }
    }
}
