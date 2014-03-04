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

	function resolveView(view){

		if(view.indexOf(".html") < 0){
			view = view + ".html";
		}
		if(view.indexOf("/") < 0){
			view = "views/install/" + view;
		}

		return view;
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
						service.status.configuring = true;
					}, 2000);
				});
			}
		},

		gotoStep : function(index){
			var step = service.status.steps[index];
			step.view = resolveView(step.view);

			if(!step.model){
				step.model = {};
			}

			service.status.index = index;
			service.status.current = step;
			service.retrieveCurrentStep();
		},

		gotoNamedStep : function(step){
				var step = _.find(service.status.steps, function(s, index){
					if(s.view && s.name === step){
						service.status.index = index;
						return true;
					}
				});

				step.view = resolveView(step.view);
				if(!step.model){
					step.model = {};
				}
				service.retrieveCurrentStep();
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
			service.retrieveCurrentStep();
		},

		storeCurrentStep : function(){
			_installerModel.instructions[service.status.current.name] = service.status.current.model;
		},

		retrieveCurrentStep : function(){
			if(_installerModel.instructions[service.status.current.name]){
					service.status.current.model = _installerModel.instructions[service.status.current.name];
			}
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
			var feedback = 0;
			service.storeCurrentStep();
			service.switchToFeedback();
			service.status.feedback = getDescriptionForStepAtIndex(service.status.steps, 0);

			function processInstallStep(){
				$http.post(Umbraco.Sys.ServerVariables.installApiBaseUrl + "PostPerformInstall",
					_installerModel).then(function(response){
						if(!response.data.complete){
							feedback++;

							if(response.data.view){

								//set the current view and model to whatever the process returns, the view is responsible for retriggering install();
								var v = resolveView(response.data.view);
								service.status.current = {view: v, model: response.data.model};

								//turn off loading bar and feedback
								service.switchToConfiguration();
							}else{
								var desc = getDescriptionForStepAtIndex(service.status.steps, feedback);
								if (desc) {
										service.status.feedback = desc;
								}

								processInstallStep();
							}
						}else{
							service.status.done = true;
							service.status.feedback = "Redirecting you to Umbraco, please wait";
							service.status.loading = false;
							service.complete();
						}
					}, function(err){
							//this is where we handle installer error
							var v = err.data.view ? resolveView(err.data.view) : resolveView("error");
							var model = err.data.model ? err.data.model : err.data;

							service.status.current = {view: v, model: model};
							service.switchToConfiguration();
					});
			}
			processInstallStep();
		},

		switchToFeedback : function(){
			service.status.current = undefined;
			service.status.loading = true;
			service.status.configuring = false;
		},

		switchToConfiguration : function(){
			service.status.loading = false;
			service.status.configuring = true;
			service.status.feedback = undefined;
		},

		complete : function(){
			$timeout(function(){
				window.location.href = Umbraco.Sys.ServerVariables.umbracoBaseUrl;
			}, 1500);
		}
	};

	return service;
});
