angular.module("umbraco").controller('Umbraco.Dialogs.Template.QueryBuilderController',
		function($scope, $http, dialogService){
			

            $http.get("backoffice/UmbracoApi/TemplateQuery/GetAllowedProperties").then(function(response) {
                $scope.properties = response.data;
            });

            $http.get("backoffice/UmbracoApi/TemplateQuery/GetContentTypes").then(function (response) {
                $scope.contentTypes = response.data;
            });

            $http.get("backoffice/UmbracoApi/TemplateQuery/GetFilterConditions").then(function (response) {
                $scope.conditions = response.data;
            });


			$scope.query = {
				contentType: {
					name: "Everything"
				},
				source:{
					name: "My website"
				}, 
				filters:[
					{
						property:undefined,
						operator: undefined
					}
				],
				sort:{
					property:{
						alias: "",
						name: "",
					},
					direction: "Ascending"
				}
			};



			$scope.chooseSource = function(query){
				dialogService.contentPicker({
				    callback: function (data) {

				        if (data.id > 0) {
				            query.source = { id: data.id, name: data.name };
				        } else {
				            query.source.name = "My website";
				            delete query.source.id;
				        }
					}
				});
			};

		    $scope.$watch("query", function(value) {
                 
		        $http.post("backoffice/UmbracoApi/TemplateQuery/PostTemplateQuery", $scope.query).then(function (response) {
		            $scope.result = response.data;
		        });

		    }, true);

			$scope.getPropertyOperators = function (property) {

			    var conditions = _.filter($scope.conditions, function(condition) {
			        var index = condition.appliesTo.indexOf(property.type);
			        return index >= 0;
			    });
			    return conditions;
			};

			
			$scope.addFilter = function(query){

				var f = {
						property:{
							alias: "meh",
							name: "Meh"
						},
						operator: "IS",
						value: "nothing"	
					};
				
				query.filters.push(f);
			};

			$scope.changeSortOrder = function(query){
				if(query.sort.direction === "ascending"){
					query.sort.direction = "descending";
				}else{
					query.sort.direction = "ascending";
				}
			};

			$scope.setSortProperty = function(query, property){
				query.sort.property = property;
				if(property.type === "datetime"){
					query.sort.direction = "descending";
				}else{
					query.sort.direction = "ascending";
				}
			};
		});