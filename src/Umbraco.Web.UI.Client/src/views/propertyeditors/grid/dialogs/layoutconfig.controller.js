angular.module("umbraco")
    .controller("Umbraco.PropertyEditors.GridPrevalueEditor.LayoutConfigController",
    function ($scope) {

    		$scope.currentLayout = $scope.model.currentLayout;
    		$scope.columns = $scope.model.columns;
    		$scope.rows = $scope.model.rows;

    		$scope.scaleUp = function(section, max, overflow){
    		   var add = 1;
    		   if(overflow !== true){
    		        add = (max > 1) ? 1 : max;
    		   }
    		   //var add = (max > 1) ? 1 : max;
    		   section.grid = section.grid+add;
    		};

    		$scope.scaleDown = function(section){
    		   var remove = (section.grid > 1) ? 1 : section.grid;
    		   section.grid = section.grid-remove;
    		};

    		$scope.percentage = function(spans){
    		    return ((spans / $scope.columns) * 100).toFixed(8);
    		};

    		$scope.toggleCollection = function(collection, toggle){
    		    if(toggle){
    		        collection = [];
    		    }else{
    		        delete collection;
    		    }
    		};



    		/****************
    		    Section
    		*****************/
    		$scope.configureSection = function(section, template){
    		   if(section === undefined){
    		        var space = ($scope.availableLayoutSpace > 4) ? 4 : $scope.availableLayoutSpace;
    		        section = {
    		            grid: space
    		        };
    		        template.sections.push(section);
    		    }
    		    
    		    $scope.currentSection = section;
    		};


    		$scope.deleteSection = function(index){
    		    $scope.currentTemplate.sections.splice(index, 1);
    		};
    		
    		$scope.closeSection = function(){
    		    $scope.currentSection = undefined;
    		};

    		$scope.$watch("currentLayout", function(layout){
    		    if(layout){
    		        var total = 0;
    		        _.forEach(layout.sections, function(section){
    		            total = (total + section.grid);
    		        });

    		        $scope.availableLayoutSpace = $scope.columns - total;
    		    }
    		}, true);
    });
