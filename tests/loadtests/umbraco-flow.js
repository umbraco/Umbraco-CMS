// Export methods for Artillery to be able to use
module.exports = {
    captureXsrf: captureXsrf,
    sendXsrf: sendXsrf
}

// TODO: This is purely testing perf counters, you can list all perf counters names
// by doing this at the cmd prompt: typeperf -qx > all-counters.txt

// This uses this package: https://github.com/markitondemand/node-perfmon
var perfmon = require('perfmon');

perfmon('\\processor(_total)\\% processor time', function(err, data) {
	console.log(data);
});

// This can bet the working set from a process (like iisexpress)
perfmon('\\Process(iisexpress)\\Private Bytes', function(err, data) {
	console.log(data);
});

// TODO: This should get the managed memory for (all) aspnet apps but I cannot get this to work
// with iisexpress. Even by configuring AppDomain.MonitoringIsEnabled = true; in c# code
// and <appDomainResourceMonitoring enabled="true"/> in the applicationhost.config along with
// C:\Windows\Microsoft.NET\Framework\v4.0.30319\Aspnet.config and
// C:\Windows\Microsoft.NET\Framework64\v4.0.30319\Aspnet.config
// This might work in IIS though, I need to check.
perfmon('\\ASP.NET Applications(__Total__)\\Managed Memory Used (estimated)', function(err, data) {
	console.log(data);
});

/** Called when artillery sends a request */
function sendXsrf(requestParams, context, ee, next) {
    // set the xsrf header if we've captured it
    if (context.vars.umbXsrf) {
        requestParams.headers["X-UMB-XSRF-TOKEN"] = context.vars.umbXsrf;
    }
    return next();
}

/** Called when artillery receives a response */
function captureXsrf(requestParams, response, context, ee, next) {
    // extract the xsrf value
    if (response.headers && response.headers["set-cookie"]) {
        var cookies = response.headers["set-cookie"]; // a string array
        for (var i = 0; i < cookies.length; i++) {
            var parts = cookies[i].split("=");
            if (parts[0] == "UMB-XSRF-TOKEN") {
                var val = parts[1].split(";")[0].trim();
                context.vars.umbXsrf = val;
                // console.log("XSRF VAL: " + context.vars.umbXsrf);
                break;
            }
        }
    }
    return next();
}
