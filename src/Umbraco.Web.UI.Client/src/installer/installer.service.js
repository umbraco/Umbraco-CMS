angular.module("umbraco.install").factory('installerService', function($q, $timeout, $http, $location){
	
	var _status = {
		index: 0,
		current: undefined,
		steps: undefined,
		loading: true
	};


	var _installerModel = {
	    installId: undefined,
        instructions: {
            DatabaseConfigure: { dbType: 0 },
            StarterKitDownload: Umbraco.Sys.ServerVariables.defaultStarterKit
        }		
	};

    /* 
        Returns the description for the step at a given index based on the order of the serverOrder of steps
        Since they don't execute on the server in the order that they are displayed in the UI.
    */
	function getDescriptionForStepAtIndex(steps, index) {
	    var sorted = _.sortBy(steps, "serverOrder");
	    if (sorted[index]) {
	        return sorted[index].description;
	    }
	    return null;
	}

    /* Returns the description for the given step name */ 
	function getDescriptionForStepName(steps, name) {
	    var found = _.find(steps, function(i) {
	        return i.name == name;
	    });
	    return (found) ? found.description : null;
	}

	var service = {

		status : _status,
		
		getPackages : function(){
			return $http.get(Umbraco.Sys.ServerVariables.installApiBaseUrl + "GetPackages");
		},

		getSteps : function(){
			return $http.get(Umbraco.Sys.ServerVariables.installApiBaseUrl + "GetSetup");
		},

		init : function(){
			service.status.loading = true;
			if(!_status.all){
				service.getSteps().then(function(response){
					service.status.steps = response.data.steps;
					service.status.index = 0;
					_installerModel.installId = response.data.installId;
					service.findNextStep();
					
					$timeout(function(){
						service.status.loading = false;
						service.status.installing = true;
					}, 2000);
				});
			}
		},

		gotoStep : function(index){
			var step = service.status.steps[index];
			if(step.view.indexOf(".html") < 0){
				step.view = step.view + ".html";
			}
			if(step.view.indexOf("/") < 0){
				step.view = "views/install/" + step.view;
			}
			if(!step.model){
				step.model = {};
			}
			service.status.index = index;
			service.status.current = step;
		},

		findNextStep : function(){
			var step = _.find(service.status.steps, function(s, index){ 
				if(s.view && index >= service.status.index){
					service.status.index = index;
					return true;
				}
			});

			if(step.view.indexOf(".html") < 0){
				step.view = step.view + ".html";
			}

			if(step.view.indexOf("/") < 0){
				step.view = "views/install/" + step.view;
			}

			if(!step.model){
				step.model = {};
			}

			service.status.current = step;
		}, 

		storeCurrentStep : function(){
			_installerModel.instructions[service.status.current.name] = service.status.current.model;
		},

		forward : function(){
			service.storeCurrentStep();
			service.status.index++;
			service.findNextStep();
		},

		backwards : function(){
			service.storeCurrentStep();
			service.gotoStep(service.status.index--);
		},

		install : function(){
			service.storeCurrentStep();
			service.status.current = undefined;
			service.status.feedback = [];
			service.status.loading = true;

			var feedback = 0;
			service.status.feedback = getDescriptionForStepAtIndex(service.status.steps, 0);

			function processInstallStep(){
				$http.post(Umbraco.Sys.ServerVariables.installApiBaseUrl + "PostPerformInstall", 
					_installerModel).then(function(response){
						if(!response.data.complete){
							feedback++;

							var desc = getDescriptionForStepName(service.status.steps, response.data.nextStep);
							if (desc) {
							    service.status.feedback = desc;
							}

							processInstallStep();
						}else{
							service.status.done = true;
							service.status.feedback = undefined;
							service.status.loading = false;
							service.complete();
						}
					});
			}
			processInstallStep();
		},
		complete : function(){
			window.location.href = Umbraco.Sys.ServerVariables.umbracoBaseUrl;
		}
	};

	return service;
});