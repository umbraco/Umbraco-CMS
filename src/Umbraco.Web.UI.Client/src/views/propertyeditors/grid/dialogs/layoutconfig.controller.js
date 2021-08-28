angular.module("umbraco")
    .controller("Umbraco.PropertyEditors.GridPrevalueEditor.LayoutConfigController",
    function ($scope, localizationService) {

            var vm = this;

            vm.toggleAllowed = toggleAllowed;
            vm.configureSection = configureSection;
            vm.deleteSection = deleteSection;
            vm.selectRow = selectRow;
            vm.percentage = percentage;
            vm.scaleUp = scaleUp;
            vm.scaleDown = scaleDown;
            vm.close = close;
            vm.submit = submit;

            vm.labels = {};
        
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

            function scaleUp(section, max, overflow){
    		   var add = 1;
    		   if (overflow !== true){
    		       add = (max > 1) ? 1 : max;
    		   }
    		   //var add = (max > 1) ? 1 : max;
    		   section.grid = section.grid+add;
    		}

            function scaleDown(section){
    		   var remove = (section.grid > 1) ? 1 : 0;
    		   section.grid = section.grid-remove;
    		}

    		function percentage(spans){
    		    return ((spans / $scope.columns) * 100).toFixed(8);
    		}

    		/****************
    		    Section
    		*****************/
            function configureSection(section, template) {
    		    if (section === null || section === undefined) {
    		        var space = ($scope.availableLayoutSpace > 4) ? 4 : $scope.availableLayoutSpace;
    		        section = {
                        grid: space,
                        rows: Utilities.copy($scope.rows)
    		        };
    		        template.sections.push(section);
                }

                section.allowAll = section.allowAll || !section.allowed || !section.allowed.length;

                $scope.currentSection = section;
    		}

            function toggleAllowed(section) {
                section.allowAll = !section.allowAll;

                if (section.allowed) {
                    delete section.allowed;
                }
                else {
                    section.allowed = [];
                }
            }

            function deleteSection(section, template) {
    			if ($scope.currentSection === section) {
    				$scope.currentSection = null;
    			}
    			var index = template.sections.indexOf(section)
    			template.sections.splice(index, 1);
    		}

            function selectRow(section, row) {

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
            }
    		
            function close() {
                if ($scope.model.close) {
                    cleanUpRows();
                    $scope.model.close();
                }
            }

            function submit() {
                if ($scope.model.submit) {
                    cleanUpRows();
                    $scope.model.submit($scope.currentLayout);
                }
            }

            function cleanUpRows () {
                $scope.currentLayout.sections.forEach(section => {
                    if (section.rows) {
                        delete section.rows;
                    }
                });
            }

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
