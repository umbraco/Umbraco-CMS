describe('Drop down controller tests', function () {
    var scope, controllerFactory;
    
    beforeEach(module('umbraco'));

    beforeEach(inject(function ($rootScope, $controller) {

        controllerFactory = $controller;
        scope = $rootScope.$new();
        scope.model = {};
    }));

    describe('initialization', function () {

        it('should define the default properties on construction', function () {
             
            controllerFactory('Umbraco.PropertyEditors.DropdownController', {
                $scope: scope,
                $routeParams: routeParams
            });
            
            expect(scope.model.config).toBeDefined();
            expect(scope.model.config.items).toBeDefined();
            expect(scope.model.config.multiple).toBeDefined();
        });
        
        it("should convert simple array to dictionary", function () {
            
            scope.model = {
                config: {
                    items: ["value0", "value1", "value2"]
                }
            };

            controllerFactory('Umbraco.PropertyEditors.DropdownController', {
                $scope: scope,
                $routeParams: routeParams
            });
             
            expect(scope.model.config.items["value0"]).toBe("value0");
            expect(scope.model.config.items["value1"]).toBe("value1");
            expect(scope.model.config.items["value2"]).toBe("value2");
            
        });
        
        it("should allow an existing valid dictionary", function () {

            scope.model = {
                config: {
                    items: {
                        "value0" : "Value 0",
                        "value1" : "Value 1",
                        "value2" : "Value 2" 
                    }
                }
            };

            var test = function() {
                controllerFactory('Umbraco.PropertyEditors.DropdownController', {
                    $scope: scope,
                    $routeParams: routeParams
                });
            };

            expect(test).not.toThrow();

        });
        
        it("should not allow a non-array or non-dictionary", function () {

            scope.model = {
                config: {
                    items: true
                }
            };

            var test = function () {
                controllerFactory('Umbraco.PropertyEditors.DropdownController', {
                    $scope: scope,
                    $routeParams: routeParams
                });
            };

            expect(test).toThrow();

        });

    });
});