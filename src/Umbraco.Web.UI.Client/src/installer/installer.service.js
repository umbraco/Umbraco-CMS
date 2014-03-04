angular.module("umbraco.install").factory('installerService', function($q, $timeout, $http, $location){
	
	var _status = {
		index: 0,
		current: undefined,
		steps: undefined,
		loading: true
	};


	var _installerModel = {
		DatabaseConfigure: {dbType: 0},
		StarterKitDownload: "69e44beb-15ff-4cee-8b64-0a7dae498657"
	};

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
			var step = _.find(service.status.steps, function(step, index){ 
				if(step.view && index >= service.status.index){
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
			_installerModel[service.status.current.name] = service.status.current.model;
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

			var _feedback = 0;
			service.status.feedback = service.status.steps[0].description;

			function processInstallStep(){
				$http.post(Umbraco.Sys.ServerVariables.installApiBaseUrl + "PostPerformInstall", 
					_installerModel).then(function(response){
						if(!response.data.complete){
							_feedback++;

							var step = service.status.steps[_feedback];
							if(step){
								service.status.feedback = step.description;
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