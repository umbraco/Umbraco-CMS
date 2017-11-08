angular.module("umbraco.install").factory('installerService', function($rootScope, $q, $timeout, $http, $location, $log){

	var _status = {
		index: 0,
		current: undefined,
		steps: undefined,
		loading: true,
		progress: "100%"
	};

	var factTimer = undefined;
	var _installerModel = {
	    installId: undefined,
        instructions: {
        }
	};

	//add to umbraco installer facts here
	var facts = ['Umbraco helped millions of people watch a man jump from the edge of space',
					'Over 440 000 websites are currently powered by Umbraco',
					"At least 2 people have named their cat 'Umbraco'",
					'On an average day, more than 1000 people download Umbraco',
					'<a target="_blank" href="https://umbraco.tv">umbraco.tv</a> is the premier source of Umbraco video tutorials to get you started',
					'You can find the world\'s friendliest CMS community at <a target="_blank" href="https://our.umbraco.org">our.umbraco.org</a>',
					'You can become a certified Umbraco developer by attending one of the official courses',
					'Umbraco works really well on tablets',
					'You have 100% control over your markup and design when crafting a website in Umbraco',
					'Umbraco is the best of both worlds: 100% free and open source, and backed by a professional and profitable company',
					"There's a pretty big chance, you've visited a website powered by Umbraco today",
					"'Umbraco-spotting' is the game of spotting big brands running Umbraco",
					"At least 4 people have the Umbraco logo tattooed on them",
					"'Umbraco' is the danish name for an allen key",
					"Umbraco has been around since 2005, that's a looong time in IT",
					"More than 600 people from all over the world meet each year in Denmark in June for our annual conference <a target='_blank' href='https://umbra.co/codegarden'>CodeGarden</a>", 
					"While you are installing Umbraco someone else on the other side of the planet is probably doing it too",
					"You can extend Umbraco without modifying the source code using either JavaScript or C#",
					"Umbraco has been installed in more than 198 countries"
					 ];

    /**
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

	//calculates the offset of the progressbar on the installer
	function calculateProgress(steps, next) {
		var sorted = _.sortBy(steps, "serverOrder");

		var pct = "100%";
		for (var i = sorted.length - 1; i >= 0; i--) {
			if(sorted[i].name == next){
				pct = Math.floor((i+1) / steps.length  * 100) + "%";
				break;
			}
		}
	    return  pct;
	}

	//helpful defaults for the view loading
	function resolveView(view){

		if(view.indexOf(".html") < 0){
			view = view + ".html";
		}
		if(view.indexOf("/") < 0){
			view = "views/install/" + view;
		}

		return view;
	}

    /** Have put this here because we are not referencing our other modules */
	function safeApply (scope, fn) {
	    if (scope.$$phase || scope.$root.$$phase) {
	        if (angular.isFunction(fn)) {
	            fn();
	        }
	    }
	    else {
	        if (angular.isFunction(fn)) {
	            scope.$apply(fn);
	        }
	        else {
	            scope.$apply();
	        }
	    }
	}

	var service = {

		status : _status,
		//loads the needed steps and sets the intial state
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

		//loads available packages from our.umbraco.org
		getPackages : function(){
			return $http.get(Umbraco.Sys.ServerVariables.installApiBaseUrl + "GetPackages");
		},

		getSteps : function(){
			return $http.get(Umbraco.Sys.ServerVariables.installApiBaseUrl + "GetSetup");
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

		gotoNamedStep : function(stepName){
			var step = _.find(service.status.steps, function(s, index){
				if (s.view && s.name === stepName) {
					service.status.index = index;
					return true;
				}
				return false;
			});

			step.view = resolveView(step.view);
			if(!step.model){
				step.model = {};
			}
			service.retrieveCurrentStep();
			service.status.current = step;
		},

	    /** 
            Finds the next step containing a view. If one is found it stores it as the current step 
            and retreives the step information and returns it, otherwise returns null .
        */
		findNextStep : function(){
			var step = _.find(service.status.steps, function(s, index){
				if(s.view && index >= service.status.index){
					service.status.index = index;
					return true;
				}
			    return false;
			});

            if (step) {
                if (step.view.indexOf(".html") < 0) {
                    step.view = step.view + ".html";
                }

                if (step.view.indexOf("/") < 0) {
                    step.view = "views/install/" + step.view;
                }

                if (!step.model) {
                    step.model = {};
                }

                service.status.current = step;
                service.retrieveCurrentStep();

                //returns the next found step
                return step;
            }
            else {
                //there are no more steps found containing a view so return null
                return null;
            }
		},

		storeCurrentStep : function(){
			_installerModel.instructions[service.status.current.name] = service.status.current.model;
		},

		retrieveCurrentStep : function(){
			if(_installerModel.instructions[service.status.current.name]){
				service.status.current.model = _installerModel.instructions[service.status.current.name];
			}
		},

        /** Moves the installer forward to the next view, if there are not more views than the installation will commence */
		forward : function(){
			service.storeCurrentStep();
			service.status.index++;
			var found = service.findNextStep();
            if (!found) {
                //no more steps were found so start the installation process
                service.install();
            }
		},

		backwards : function(){
			service.storeCurrentStep();
			service.gotoStep(service.status.index--);
		},

		install : function(){
			service.storeCurrentStep();
			service.switchToFeedback();

			service.status.feedback = getDescriptionForStepAtIndex(service.status.steps, 0);
			service.status.progress = 0;

			function processInstallStep() {

			    $http.post(Umbraco.Sys.ServerVariables.installApiBaseUrl + "PostPerformInstall", _installerModel)
			        .success(function(data, status, headers, config) {
			            if (!data.complete) {

			                //progress feedback
			                service.status.progress = calculateProgress(service.status.steps, data.nextStep);

			                if (data.view) {
			                    //set the current view and model to whatever the process returns, the view is responsible for retriggering install();
			                    var v = resolveView(data.view);
			                    service.status.current = { view: v, model: data.model };

			                    //turn off loading bar and feedback
			                    service.switchToConfiguration();
			                }
			                else {
			                    var desc = getDescriptionForStepName(service.status.steps, data.nextStep);
			                    if (desc) {
			                        service.status.feedback = desc;
			                    }
			                    processInstallStep();
			                }
			            }
			            else {
			                service.complete();
			            }
			        }).error(function(data, status, headers, config) {
			            //need to handle 500's separately, this will happen if something goes wrong outside
			            // of the installer (like app startup events or something) and these will get returned as text/html
			            // not as json. If this happens we can't actually load in external views since they will YSOD as well!
                        // so we need to display this in our own internal way
			            
			            if (status >= 500 && status < 600) {			                
			                service.status.current = { view: "ysod", model: null };
			                var ysod = data;
                            //we need to manually write the html to the iframe - the html contains full html markup
			                $timeout(function () {
			                    document.getElementById('ysod').contentDocument.write(ysod);
			                }, 500);
			            }
			            else {
			                //this is where we handle installer error
			                var v = data.view ? resolveView(data.view) : resolveView("error");
			                var model = data.model ? data.model : data;
			                service.status.current = { view: v, model: model };			                
			            }

			            service.switchToConfiguration();
			            
			        });
			}
			processInstallStep();
		},

		randomFact: function () {
		    safeApply($rootScope, function() {
		        service.status.fact = facts[_.random(facts.length - 1)];
		    });
		},

		switchToFeedback : function(){
			service.status.current = undefined;
			service.status.loading = true;
			service.status.configuring = false;

			//initial fact
			service.randomFact();

			//timed facts
			factTimer = window.setInterval(function(){
				service.randomFact();
			},6000);
		},

		switchToConfiguration : function(){
			service.status.loading = false;
			service.status.configuring = true;
			service.status.feedback = undefined;
			service.status.fact = undefined;

			if(factTimer){
				clearInterval(factTimer);
			}
		},

		complete : function(){

			service.status.progress = "100%";	
			service.status.done = true;
			service.status.feedback = "Redirecting you to Umbraco, please wait";
			service.status.loading = false;

			if(factTimer){
				clearInterval(factTimer);
			}

			$timeout(function(){
				window.location.href = Umbraco.Sys.ServerVariables.umbracoBaseUrl;
			}, 1500);
		}
	};

	return service;
});
