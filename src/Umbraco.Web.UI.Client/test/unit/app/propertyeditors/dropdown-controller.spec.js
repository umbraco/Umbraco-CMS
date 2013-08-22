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
             
            controllerFactory('Umbraco.Editors.DropdownController', {
                $scope: scope,
                $routeParams: routeParams
            });
            
            expect(scope.model.config).toBeDefined();
            expect(scope.model.config.items).toBeDefined();
            expect(scope.model.config.multiple).toBeDefined();
            expect(scope.model.config.keyName).toBeDefined();
            expect(scope.model.config.valueName).toBeDefined();
        });
        
        it("should convert simple array to dictionary", function () {
            
            scope.model = {
                config: {
                    items: ["value0", "value1", "value2"]
                }
            };

            controllerFactory('Umbraco.Editors.DropdownController', {
                $scope: scope,
                $routeParams: routeParams
            });
             
            expect(scope.model.config.items.length).toBe(3);
            for (var i = 0; i < scope.model.config.items.length; i++) {
                expect(scope.model.config.items[i].alias).toBeDefined();
                expect(scope.model.config.items[i].name).toBeDefined();
                //name and alias will be the same in this case
                expect(scope.model.config.items[i].alias).toBe("value" + i);
                expect(scope.model.config.items[i].name).toBe("value" + i);
            }
            
        });
        
        it("should allow an existing valid dictionary", function () {

            scope.model = {
                config: {
                    items: [
                        { alias: "value0", name: "Value 0" },
                        { alias: "value1", name: "Value 1" },
                        { alias: "value2", name: "Value 2" }
                    ]
                }
            };

            var test = function() {
                controllerFactory('Umbraco.Editors.DropdownController', {
                    $scope: scope,
                    $routeParams: routeParams
                });
            };

            expect(test).not.toThrow();

        });
        
        it("should not allow an existing invalid dictionary", function () {

            scope.model = {
                config: {
                    items: [
                        { blah: "value0", name: "Value 0" },
                        { blah: "value1", name: "Value 1" },
                        { blah: "value2", name: "Value 2" }
                    ]
                }
            };

            var test = function () {
                controllerFactory('Umbraco.Editors.DropdownController', {
                    $scope: scope,
                    $routeParams: routeParams
                });
            };

            expect(test).toThrow();

        });

    });
});