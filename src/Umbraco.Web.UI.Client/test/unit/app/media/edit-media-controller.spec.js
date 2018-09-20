describe('edit media controller tests', function () {
    var scope, controller, routeParams, httpBackend;
    routeParams = { id: 1234, create: false };

    beforeEach(module('umbraco'));

    //inject the contentMocks service
    beforeEach(inject(function ($rootScope, $controller, angularHelper, $httpBackend, mediaMocks, entityMocks, mocksUtils, localizationMocks) {
        //for these tests we don't want any authorization to occur
        mocksUtils.disableAuth();

        httpBackend = $httpBackend;
        scope = $rootScope.$new();

        //have the contentMocks register its expect urls on the httpbackend
        //see /mocks/content.mocks.js for how its setup
        mediaMocks.register();
        entityMocks.register();
        localizationMocks.register();

        //this controller requires an angular form controller applied to it
        scope.contentForm = angularHelper.getNullForm("contentForm");

        controller = $controller('Umbraco.Editors.Media.EditController', {
            $scope: scope,
            $routeParams: routeParams
        });

        //For controller tests its easiest to have the digest and flush happen here
        //since its intially always the same $http calls made

        //scope.$digest resolves the promise against the httpbackend
        scope.$digest();
        //httpbackend.flush() resolves all request against the httpbackend
        //to fake a async response, (which is what happens on a real setup)
        httpBackend.flush();
    }));

    describe('media edit controller init', function () {

        it('it should have an media object', function () {

            //controller should have a content object
            expect(scope.content).not.toBeUndefined();

            //if should be the same as the routeParams defined one
            expect(scope.content.id).toBe(1234);
        });

        it('it should have an apps collection', function () {
            expect(scope.content.apps.length).toBe(2);
        });

        it('it should have added an info app', function () {
            expect(scope.content.apps[1].alias).toBe("info");
        });
        
    });
});
