/// <reference name="MicrosoftAjax.js"/>

if (typeof Umbraco == 'undefined') var Umbraco = {};
if (!Umbraco.Sys) Umbraco.Sys = {};


Umbraco.Sys.UmbracoDependencyLoader = function() {
    this.addedDependencies = [];
    this.jsQueue = new Array();
    this.jsQueueWaiting = false;
}

// ********************* Dependency Loader Methods *********************
Umbraco.Sys.UmbracoDependencyLoader.prototype.AddJs = function(filePath, callbackMethod) {
    if (typeof (Sys) == "undefined") {
        var loader = this;
        setTimeout(function() { loader.AddJs(filePath, callbackMethod); }, 100);
    }
    else {
        if (this.addedDependencies[filePath] == undefined) {
            Sys.Debug.trace("UmbracoDependencyLoader: Queueing '" + filePath + "'.");

            // add script to queue
            var script = new Array();
            script.filePath = filePath;
            script.callbackMethod = callbackMethod;
            script.loaded = false;
            this.jsQueue[this.jsQueue.length] = script;
            this.addedDependencies[filePath] = true;

            Sys.Debug.trace("UmbracoDependencyLoader: Queued '" + filePath + "', queue length " + this.jsQueue.length + ".");

            if (!this.jsQueueWaiting)
                this.LoadNextJs();
        }
        else {
            Sys.Debug.trace("UmbracoDependencyLoader: Javascript file has already been added '" + filePath + "'.");
        }
    }
    return this;
}

Umbraco.Sys.UmbracoDependencyLoader.prototype.AddCss = function(filePath) {
    if (typeof (Sys) == "undefined") {
        var loader = this;
        setTimeout(function() { loader.AddCss(filePath); }, 100);
    }
    else {
        if (this.addedDependencies[filePath] == undefined) {

            var tempCss = document.createElement("link")
            tempCss.setAttribute("href", filePath);
            tempCss.setAttribute("rel", "stylesheet");
            tempCss.setAttribute("type", "text/css");
            document.getElementsByTagName("head")[0].appendChild(tempCss);

            this.addedDependencies[filePath] = "loaded";

        }
        else {
            Sys.Debug.trace("UmbracoDependencyLoader: Css file has already been added: '" + filePath + "'.");
        }
    }
    return this;
}

Umbraco.Sys.UmbracoDependencyLoader.prototype.LoadNextJs = function() {
    if (this.jsQueue.length > 0) {
        this.jsQueueWaiting = true;
        
        var script = this.jsQueue[0];
        this.jsQueue.splice(0, 1);

        Sys.Debug.trace("UmbracoDependencyLoader: Loading '" + script.filePath + "'. (" + this.jsQueue.length + " scripts left)");
        var tempJs = document.createElement('script');
        tempJs.setAttribute("src", script.filePath);
        tempJs.setAttribute("type", "text/javascript");

        tempJs.onload = function() { Umbraco.Sys.onScriptAvailable(script); };
        tempJs.onreadystatechange = function() {
            if (this.readyState == 'loaded' || this.readyState == 'complete') {
                Umbraco.Sys.onScriptAvailable(script);
            }
        };
        document.getElementsByTagName("head")[0].appendChild(tempJs);
    }
    else {
        Sys.Debug.trace('UmbracoDependencyLoader: No scripts left.');
        this.jsQueueWaiting = false;
    }
}

Umbraco.Sys.UmbracoDependencyLoader.prototype.RunMethod = function(methodName) {
    var invoker = new UmbracoMethodInvoker();
    invoker.Run(methodName);
}

// Initialize
var UmbDependencyLoader = new Umbraco.Sys.UmbracoDependencyLoader();

Umbraco.Sys.onScriptAvailable = function(script) {
    /// <summary>
    /// This method checks if the string representation of a method (sMethod) is registered as a function yet,
    /// and if it is it'll call the function (oCallback), if not it'll try again after 50 ms.
    /// </summary>
    if (!script.loaded) {
        Sys.Debug.trace("UmbracoDependencyLoader: Loaded '" + script.filePath + "'.");
        script.loaded = true;
    }
    if (script.callbackMethod == '') {
        UmbDependencyLoader.LoadNextJs();
    }
    else {
        if (typeof (eval(script.callbackMethod)) == 'function') {
            UmbDependencyLoader.LoadNextJs();
            Sys.Debug.trace("UmbracoDependencyLoader: Executing '" + script.filePath + "' callback '" + script.callbackMethod + "'.");
            var func = eval(script.callbackMethod);
            alert(func);
            func.call(this);
            //eval(script.callbackMethod + "()");
        }
        else {
            setTimeout(function() {
                Umbraco.Sys.onScriptAvailable(script);
            }, 50);
        }
    }
}