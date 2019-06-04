angular.module("umbraco").controller("Umbraco.Core.TrixController",
function ($scope, assetsService) {
    
	assetsService.load(["~/App_Plugins/TrixEditor/trix.js"]).then(
        function() {
            
		}
    );

	// load the separate css for the editor to avoid it blocking our JavaScript loading
	assetsService.loadCss("~/App_Plugins/TrixEditor/trix.css");
    
    
});
