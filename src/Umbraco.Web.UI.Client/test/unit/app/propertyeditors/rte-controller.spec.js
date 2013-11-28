describe('RTE controller tests', function () {
    var scope, controllerFactory;
    
    beforeEach(module('umbraco'));

    beforeEach(inject(function ($rootScope, $controller) {
        controllerFactory = $controller;
        scope = $rootScope.$new();
        scope.model = {value: "<p>hello</p>"};
    }));


    describe('initialization', function () {

        it('should define the default properties on construction', function () {     
            controllerFactory('Umbraco.PropertyEditors.RTEController', {
                $scope: scope,
                $routeParams: routeParams
            });
        });
        
    });
});