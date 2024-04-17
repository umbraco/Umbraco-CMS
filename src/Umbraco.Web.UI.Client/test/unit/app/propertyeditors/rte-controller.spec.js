/*describe('RTE controller tests', function () {
    var scope, controllerFactory, element;

    //mock tinymce globals
    if ((typeof tinymce) === "undefined") {
        tinymce = {
            DOM: {
                events: {
                    domLoaded: false
                }
            },
            baseUrl: ""
        }
    }

    beforeEach(module('LocalStorageModule'));

    beforeEach(module('umbraco', function ($provide) {
        $provide.value('tinyMceAssets', []);
    }));

    beforeEach(inject(function ($rootScope, $controller) {
        controllerFactory = $controller;
        scope = $rootScope.$new();
        scope.model = {value: "<p>hello</p>"};
        element = $("<div></div>");
    }));


    describe('initialization', function () {

        it('should define the default properties on construction', function () {
            controllerFactory('Umbraco.PropertyEditors.RTEController', {
                $scope: scope,
                $routeParams: routeParams,
                $element: element
            });
        });

    });

});
*/
