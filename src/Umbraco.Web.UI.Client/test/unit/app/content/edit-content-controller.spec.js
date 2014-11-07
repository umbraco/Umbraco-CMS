describe('edit content controller tests', function () {
    var scope, controller, routeParams, httpBackend;
    routeParams = {id: 1234, create: false};

    beforeEach(module('umbraco'));

    //inject the contentMocks service
    beforeEach(inject(function ($rootScope, $controller, angularHelper, $httpBackend, contentMocks, entityMocks, mocksUtils) {

        //for these tests we don't want any authorization to occur
        mocksUtils.disableAuth();

        httpBackend = $httpBackend;
        scope = $rootScope.$new();
        
        //have the contentMocks register its expect urls on the httpbackend
        //see /mocks/content.mocks.js for how its setup
        contentMocks.register();
        entityMocks.register();

        //this controller requires an angular form controller applied to it
        scope.contentForm = angularHelper.getNullForm("contentForm");
        
        controller = $controller('Umbraco.Editors.Content.EditController', {
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

    describe('content edit controller save and publish', function () {
        
        it('it should have an content object', function() {

            //controller should have a content object
            expect(scope.content).toNotBe(undefined);

            //if should be the same as the routeParams defined one
            expect(scope.content.id).toBe(1234);
        });

        it('it should have a tabs collection', function () {
          expect(scope.content.tabs.length).toBe(7);
        });

        it('it should have a properties collection on each tab', function () {
              $(scope.content.tabs).each(function(i, tab){
                  expect(tab.properties.length).toBeGreaterThan(0);
              });
        });

        
        it('it should change updateDate on save', function () {
          var currentUpdateDate = scope.content.updateDate;

          setTimeout(function(){
              scope.save(scope.content);
              expect(scope.content.updateDate).toBeGreaterThan(currentUpdateDate);
              }, 1000);
        });

        it('it should change publishDate on publish', function () {
          var currentPublishDate = scope.content.publishDate;

          //wait a sec before you publish
          setTimeout(function(){
              scope.saveAndPublish(scope.content);
              
              expect(scope.content.publishDate).toBeGreaterThan(currentPublishDate);
              
              }, 1000);
        });


    });
});