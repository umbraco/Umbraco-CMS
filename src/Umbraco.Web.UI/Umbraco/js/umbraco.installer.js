(function () {
    'use strict';
    angular.module('umbraco.install', ['umbraco.directives']);
    'use strict';
    angular.module('umbraco.install').controller('Umbraco.InstallerController', function ($scope, installerService) {
        // TODO: Decouple the service from the controller - the controller should be responsible
        // for the model (state) and the service should be responsible for helping the controller,
        // the controller should be passing the model into it's methods for manipulation and not hold
        // state. We should not be assigning properties from a service to a controller's scope.
        // see: https://github.com/umbraco/Umbraco-CMS/commit/b86ef0d7ac83f699aee35d807f7f7ebb6dd0ed2c#commitcomment-5721204
        $scope.stepIndex = 0;
        //comment this out if you just want to see tips
        installerService.init();
        //uncomment this to see tips
        //installerService.switchToFeedback();
        $scope.installer = installerService.status;
        $scope.forward = function () {
            installerService.forward();
        };
        $scope.backward = function () {
            installerService.backward();
        };
        $scope.install = function () {
            installerService.install();
        };
        $scope.gotoStep = function (step) {
            installerService.gotoNamedStep(step);
        };
        $scope.restart = function () {
            installerService.gotoStep(0);
        };
    });
    //this ensure that we start with a clean slate on every install and upgrade
    angular.module('umbraco.install').run(function ($templateCache) {
        $templateCache.removeAll();
    });
    'use strict';
    angular.module('umbraco.install').factory('installerService', function ($rootScope, $q, $timeout, $http, $templateRequest) {
        var _status = {
            index: 0,
            current: null,
            steps: null,
            loading: true,
            progress: '100%'
        };
        var factTimer;
        var _installerModel = {
            installId: null,
            instructions: {}
        };
        //add to umbraco installer facts here
        var facts = [
            'Umbraco helped millions of people watch a man jump from the edge of space',
            'Over 500 000 websites are currently powered by Umbraco',
            'At least 2 people have named their cat \'Umbraco\'',
            'On an average day more than 1000 people download Umbraco',
            '<a target=\'_blank\' href=\'https://umbraco.tv/\'>umbraco.tv</a> is the premier source of Umbraco video tutorials to get you started',
            'You can find the world\'s friendliest CMS community at <a target=\'_blank\' href=\'https://our.umbraco.com/\'>our.umbraco.com</a>',
            'You can become a certified Umbraco developer by attending one of the official courses',
            'Umbraco works really well on tablets',
            'You have 100% control over your markup and design when crafting a website in Umbraco',
            'Umbraco is the best of both worlds: 100% free and open source, and backed by a professional and profitable company',
            'There\'s a pretty big chance you\'ve visited a website powered by Umbraco today',
            '\'Umbraco-spotting\' is the game of spotting big brands running Umbraco',
            'At least 4 people have the Umbraco logo tattooed on them',
            '\'Umbraco\' is the Danish name for an allen key',
            'Umbraco has been around since 2005, that\'s a looong time in IT',
            'More than 700 people from all over the world meet each year in Denmark in May for our annual conference <a target=\'_blank\' href=\'https://umbra.co/codegarden\'>CodeGarden</a>',
            'While you are installing Umbraco someone else on the other side of the planet is probably doing it too',
            'You can extend Umbraco without modifying the source code using either JavaScript or C#',
            'Umbraco has been installed in more than 198 countries'
        ];
        /**
      Returns the description for the step at a given index based on the order of the serverOrder of steps
      Since they don't execute on the server in the order that they are displayed in the UI.
  */
        function getDescriptionForStepAtIndex(steps, index) {
            var sorted = _.sortBy(steps, 'serverOrder');
            if (sorted[index]) {
                return sorted[index].description;
            }
            return null;
        }
        /* Returns the description for the given step name */
        function getDescriptionForStepName(steps, name) {
            var found = _.find(steps, function (i) {
                return i.name == name;
            });
            return found ? found.description : null;
        }
        //calculates the offset of the progressbar on the installer
        function calculateProgress(steps, next) {
            var sorted = _.sortBy(steps, 'serverOrder');
            var pct = '100%';
            for (var i = sorted.length - 1; i >= 0; i--) {
                if (sorted[i].name == next) {
                    pct = Math.floor((i + 1) / steps.length * 100) + '%';
                    break;
                }
            }
            return pct;
        }
        //helpful defaults for the view loading
        function resolveView(view) {
            if (view.indexOf('.html') < 0) {
                view = view + '.html';
            }
            if (view.indexOf('/') < 0) {
                view = 'views/install/' + view;
            }
            return view;
        }
        /** Have put this here because we are not referencing our other modules */
        function safeApply(scope, fn) {
            if (scope.$$phase || scope.$root.$$phase) {
                if (Utilities.isFunction(fn)) {
                    fn();
                }
            } else {
                if (Utilities.isFunction(fn)) {
                    scope.$apply(fn);
                } else {
                    scope.$apply();
                }
            }
        }
        var service = {
            status: _status,
            //loads the needed steps and sets the intial state
            init: function init() {
                service.status.loading = true;
                if (!_status.all) {
                    //pre-load the error page, if an error occurs, the page might not be able to load
                    // so we want to make sure it's available in the templatecache first
                    $templateRequest('views/install/error.html').then(function (x) {
                        service.getSteps().then(function (response) {
                            service.status.steps = response.data.steps;
                            service.status.index = 0;
                            _installerModel.installId = response.data.installId;
                            service.findNextStep();
                            $timeout(function () {
                                service.status.loading = false;
                                service.status.configuring = true;
                            }, 2000);
                        });
                    });
                }
            },
            //loads available packages from our.umbraco.com
            getPackages: function getPackages() {
                return $http.get(Umbraco.Sys.ServerVariables.installApiBaseUrl + 'GetPackages');
            },
            getSteps: function getSteps() {
                return $http.get(Umbraco.Sys.ServerVariables.installApiBaseUrl + 'GetSetup');
            },
            gotoStep: function gotoStep(index) {
                var step = service.status.steps[index];
                step.view = resolveView(step.view);
                if (!step.model) {
                    step.model = {};
                }
                service.status.index = index;
                service.status.current = step;
                service.retrieveCurrentStep();
            },
            gotoNamedStep: function gotoNamedStep(stepName) {
                var step = _.find(service.status.steps, function (s, index) {
                    if (s.view && s.name === stepName) {
                        service.status.index = index;
                        return true;
                    }
                    return false;
                });
                step.view = resolveView(step.view);
                if (!step.model) {
                    step.model = {};
                }
                service.retrieveCurrentStep();
                service.status.current = step;
            },
            /** 
           Finds the next step containing a view. If one is found it stores it as the current step 
           and retreives the step information and returns it, otherwise returns null .
       */
            findNextStep: function findNextStep() {
                var step = _.find(service.status.steps, function (s, index) {
                    if (s.view && index >= service.status.index) {
                        service.status.index = index;
                        return true;
                    }
                    return false;
                });
                if (step) {
                    if (step.view.indexOf('.html') < 0) {
                        step.view = step.view + '.html';
                    }
                    if (step.view.indexOf('/') < 0) {
                        step.view = 'views/install/' + step.view;
                    }
                    if (!step.model) {
                        step.model = {};
                    }
                    service.status.current = step;
                    service.retrieveCurrentStep();
                    //returns the next found step
                    return step;
                } else {
                    //there are no more steps found containing a view so return null
                    return null;
                }
            },
            storeCurrentStep: function storeCurrentStep() {
                _installerModel.instructions[service.status.current.name] = service.status.current.model;
            },
            retrieveCurrentStep: function retrieveCurrentStep() {
                if (_installerModel.instructions[service.status.current.name]) {
                    service.status.current.model = _installerModel.instructions[service.status.current.name];
                }
            },
            /** Moves the installer forward to the next view, if there are not more views than the installation will commence */
            forward: function forward() {
                service.storeCurrentStep();
                service.status.index++;
                var found = service.findNextStep();
                if (!found) {
                    //no more steps were found so start the installation process
                    service.install();
                }
            },
            backwards: function backwards() {
                service.storeCurrentStep();
                service.gotoStep(service.status.index--);
            },
            install: function install() {
                service.storeCurrentStep();
                service.switchToFeedback();
                service.status.feedback = getDescriptionForStepAtIndex(service.status.steps, 0);
                service.status.progress = 0;
                function processInstallStep() {
                    $http.post(Umbraco.Sys.ServerVariables.installApiBaseUrl + 'PostPerformInstall', _installerModel).then(function (response) {
                        var data = response.data;
                        if (!data.complete) {
                            //progress feedback
                            service.status.progress = calculateProgress(service.status.steps, data.nextStep);
                            if (data.view) {
                                //set the current view and model to whatever the process returns, the view is responsible for retriggering install();
                                var v = resolveView(data.view);
                                service.status.current = {
                                    view: v,
                                    model: data.model
                                };
                                //turn off loading bar and feedback
                                service.switchToConfiguration();
                            } else {
                                var desc = getDescriptionForStepName(service.status.steps, data.nextStep);
                                if (desc) {
                                    service.status.feedback = desc;
                                }
                                processInstallStep();
                            }
                        } else {
                            service.complete();
                        }
                    }, function (response) {
                        var data = response.data;
                        var status = response.status;
                        //need to handle 500's separately, this will happen if something goes wrong outside
                        // of the installer (like app startup events or something) and these will get returned as text/html
                        // not as json. If this happens we can't actually load in external views since they will YSOD as well!
                        // so we need to display this in our own internal way
                        if (status >= 500 && status < 600) {
                            service.status.current = {
                                view: 'ysod',
                                model: null
                            };
                            var ysod = data;
                            //we need to manually write the html to the iframe - the html contains full html markup
                            $timeout(function () {
                                document.getElementById('ysod').contentDocument.write(ysod);
                            }, 500);
                        } else {
                            //this is where we handle installer error
                            var v = data.view ? resolveView(data.view) : resolveView('error');
                            var model = data.model ? data.model : data;
                            service.status.current = {
                                view: v,
                                model: model
                            };
                        }
                        service.switchToConfiguration();
                    });
                }
                processInstallStep();
            },
            randomFact: function randomFact() {
                safeApply($rootScope, function () {
                    service.status.fact = facts[_.random(facts.length - 1)];
                });
            },
            switchToFeedback: function switchToFeedback() {
                service.status.current = null;
                service.status.loading = true;
                service.status.configuring = false;
                //initial fact
                service.randomFact();
                //timed facts
                factTimer = window.setInterval(function () {
                    service.randomFact();
                }, 6000);
            },
            switchToConfiguration: function switchToConfiguration() {
                service.status.loading = false;
                service.status.configuring = true;
                service.status.feedback = null;
                service.status.fact = null;
                if (factTimer) {
                    clearInterval(factTimer);
                }
            },
            complete: function complete() {
                service.status.progress = '100%';
                service.status.done = true;
                service.status.feedback = 'Redirecting you to Umbraco, please wait';
                service.status.loading = false;
                if (factTimer) {
                    clearInterval(factTimer);
                }
                $timeout(function () {
                    window.location.href = Umbraco.Sys.ServerVariables.umbracoBaseUrl;
                }, 1500);
            }
        };
        return service;
    });
    'use strict';
    angular.module('umbraco.install').controller('Umbraco.Installer.DataBaseController', function ($scope, $http, installerService) {
        $scope.checking = false;
        $scope.invalidDbDns = false;
        $scope.dbs = [
            {
                name: 'Microsoft SQL Server Compact (SQL CE)',
                id: 0
            },
            {
                name: 'Microsoft SQL Server',
                id: 1
            },
            {
                name: 'Microsoft SQL Azure',
                id: 3
            },
            {
                name: 'Custom connection string',
                id: -1
            }
        ];
        if (Utilities.isUndefined(installerService.status.current.model.dbType) || installerService.status.current.model.dbType === null) {
            installerService.status.current.model.dbType = 0;
        }
        $scope.validateAndForward = function () {
            if (!$scope.checking && this.myForm.$valid) {
                $scope.checking = true;
                $scope.invalidDbDns = false;
                var model = installerService.status.current.model;
                $http.post(Umbraco.Sys.ServerVariables.installApiBaseUrl + 'PostValidateDatabaseConnection', model).then(function (response) {
                    if (response.data === true) {
                        installerService.forward();
                    } else {
                        $scope.invalidDbDns = true;
                    }
                    $scope.checking = false;
                }, function () {
                    $scope.invalidDbDns = true;
                    $scope.checking = false;
                });
            }
        };
    });
    'use strict';
    angular.module('umbraco.install').controller('Umbraco.Installer.MachineKeyController', function ($scope, installerService) {
        $scope.continue = function () {
            installerService.status.current.model = true;
            installerService.forward();
        };
        $scope.ignoreKey = function () {
            installerService.status.current.model = false;
            installerService.forward();
        };
    });
    'use strict';
    angular.module('umbraco.install').controller('Umbraco.Installer.PackagesController', function ($scope, installerService) {
        installerService.getPackages().then(function (response) {
            $scope.packages = response.data;
        });
        $scope.setPackageAndContinue = function (pckId) {
            installerService.status.current.model = pckId;
            installerService.forward();
        };
    });
    'use strict';
    angular.module('umbraco.install').controller('Umbraco.Install.UserController', function ($scope, installerService) {
        $scope.passwordPattern = /.*/;
        $scope.installer.current.model.subscribeToNewsLetter = false;
        if ($scope.installer.current.model.minNonAlphaNumericLength > 0) {
            var exp = '';
            for (var i = 0; i < $scope.installer.current.model.minNonAlphaNumericLength; i++) {
                exp += '.*[\\W].*';
            }
            //replace duplicates
            exp = exp.replace('.*.*', '.*');
            $scope.passwordPattern = new RegExp(exp);
        }
        $scope.validateAndInstall = function () {
            installerService.install();
        };
        $scope.validateAndForward = function () {
            if (this.myForm.$valid) {
                installerService.forward();
            }
        };
    });
}());