angular.module("umbraco")
    .controller("Umbraco.PropertyEditors.GridPrevalueEditor.LayoutConfigController",
    function ($scope, localizationService) {

            var vm = this;

            vm.labels = {};
            vm.layout = {
                sections: []
            };
        
            function init() {

                $scope.currentLayout = $scope.model.currentLayout;
                $scope.columns = $scope.model.columns;
                $scope.rows = $scope.model.rows;
                $scope.currentSection = null;

                // Setup copy of rows on sections
                if ($scope.currentLayout && $scope.currentLayout.sections) {
                    $scope.currentLayout.sections.forEach(section => {
                        section.rows = Utilities.copy($scope.rows);

                        // Check if rows are selected
                        section.rows.forEach(row => {
                            row.selected = section.allowed && section.allowed.includes(row.name);
                        });
                    });
                }

                var labelKeys = [
                    "grid_addGridLayout",
                    "grid_allowAllRowConfigurations"
                ];

                localizationService.localizeMany(labelKeys).then(function (data) {

                    vm.labels.title = data[0];
                    vm.labels.allowAllRowConfigurations = data[1];

                    setTitle(vm.labels.title);
                });
            }

            function setTitle(value) {
                if (!$scope.model.title) {
                    $scope.model.title = value;
                }
            }

    		$scope.scaleUp = function(section, max, overflow){
    		   var add = 1;
    		   if (overflow !== true){
    		       add = (max > 1) ? 1 : max;
    		   }
    		   //var add = (max > 1) ? 1 : max;
    		   section.grid = section.grid+add;
    		};

    		$scope.scaleDown = function(section){
    		   var remove = (section.grid > 1) ? 1 : 0;
    		   section.grid = section.grid-remove;
    		};

    		$scope.percentage = function(spans){
    		    return ((spans / $scope.columns) * 100).toFixed(8);
    		};

    		/****************
    		    Section
    		*****************/
    		$scope.configureSection = function(section, template) {
    		    if (section === null || section === undefined) {
    		        var space = ($scope.availableLayoutSpace > 4) ? 4 : $scope.availableLayoutSpace;
    		        section = {
                        grid: space,
                        rows: []
    		        };
    		        template.sections.push(section);
                }

                if (section.allowAll && (!section.allowed || !section.allowed.length)) {
                    section.allowAll = true;
                }

                $scope.currentSection = section;
    		};

            vm.toggleAllowed = function (section) {
                section.allowAll = !section.allowAll;

                if (section.allowed) {
                    delete section.allowed;
                }
                else {
                    section.allowed = [];
                }
            };

    		$scope.deleteSection = function(section, template) {
    			if ($scope.currentSection === section) {
    				$scope.currentSection = null;
    			}
    			var index = template.sections.indexOf(section)
    			template.sections.splice(index, 1);
    		};

            $scope.selectRow = function (section, row) {

                section.allowed = section.allowed || [];

                var index = section.allowed.indexOf(row.name);
                if (row.selected === true) {
                    if (index === -1) {
                        section.allowed.push(row.name); 
                    }
                }
                else {
                    section.allowed.splice(index, 1);
                }
            };
    		
            $scope.close = function() {
                if ($scope.model.close) {
                    $scope.model.close();
                }
            };

            $scope.submit = function () {
                if ($scope.model.submit) {

                    // Delete temporary rows configurated in init.
                    if ($scope.currentLayout.rows) {
                        delete $scope.currentLayout.rows;
                    }

                    $scope.model.submit($scope.currentLayout);
                }
            };

    		$scope.$watch("currentLayout", function(layout){
    		    if (layout) {
    		        var total = 0;
    		        _.forEach(layout.sections, function(section){
    		            total = (total + section.grid);
    		        });

    		        $scope.availableLayoutSpace = $scope.columns - total;
    		    }
    		}, true);
            
            init();
    });
