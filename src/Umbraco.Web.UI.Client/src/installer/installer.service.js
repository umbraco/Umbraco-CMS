angular.module("umbraco.install").factory('installerService', function($q, $timeout){
	
	var _status = {
		index: 0,
		current: undefined,
		steps: undefined
	};

	var _installerModel = {};
	var service = {

		status : _status,
		getSteps : function(){
			var deferred = $q.defer();
			var s =  [
				{
					name: "User",
					view: "user",
					description: "Configuring your user account",
					completed: false
				},
				{
					name: "Database",
					view: "database",
					description: "Setting up the system database",
					completed: false
				},
				{
					name: "Packages",
					view: "packages",
					description: "Installing a staterkit",
					completed: false
				}
			];
			
			deferred.resolve(s);
			return deferred.promise;
		},

		init : function(){

			if(!_status.all){
				service.getSteps().then(function(steps){
					service.status.steps = steps;
					service.status.index = 0;
					service.gotoStep(0);
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

		storeCurrentStep : function(){
			_installerModel[service.status.current.name] = service.status.current.model;
		},

		forward : function(){
			service.storeCurrentStep();
			service.status.index++;
			service.gotoStep(service.status.index);
		},

		backwards : function(){
			service.storeCurrentStep();
			service.gotoStep(service.status.index--);
		},

		install : function(){
			service.storeCurrentStep();
			service.status.current = undefined;

			
			_.each(service.status.steps, function(step){
				$timeout(function(){
					step.completed = true;
				}, 2000);
			});

			//post the installer model to somewhere...
		}
	};

	return service;
});