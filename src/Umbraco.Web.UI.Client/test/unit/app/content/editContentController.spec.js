describe('edit content controller tests', function () {
    var scope, controller, routeParams, httpBackend;
    routeParams = {id: 1234, create: false};

    beforeEach(module('umbraco'));

    beforeEach(inject(function ($rootScope, $controller, angularHelper, $httpBackend) {
        httpBackend = $httpBackend;
        scope = $rootScope.$new();
        
        //this controller requires an angular form controller applied to it
        scope.contentForm = angularHelper.getNullForm("contentForm");
        
        controller = $controller('Umbraco.Editors.ContentEditController', {
            $scope: scope,
            $routeParams: routeParams
        });
    }));

    describe('content edit controller save and publish', function () {

        it('it should define the default properties on construction', function () {
            expect(scope.files).toNotBe(undefined);
        });
        
        it('adding a file adds to the collection', function () {
            scope.addFiles(123, ["testFile"]);
            expect(scope.files.length).toBe(1);
        });
        
        it('adding a file with the same property id replaces the existing one', function () {
            scope.addFiles(123, ["testFile"]);
            scope.addFiles(123, ["testFile2"]);
            expect(scope.files.length).toBe(1);
            expect(scope.files[0].file).toBe("testFile2");
        });

        //it('it should have an content object', function() {

        /* 
        NOTE: I cannot figure out how to make this work... I've followed along with a few sources like:
          http://stackoverflow.com/questions/15833462/angularjs-need-help-to-unit-test-a-factory-with-promise
          http://www.benlesh.com/2013/05/angularjs-unit-testing-controllers.html

          But it tells me that there is no pending request to flush, so dunno what is going on there?
        */

        //    httpBackend.flush();

        //    rootScope.$apply();

        //    expect(scope.content).toNotBe(undefined);
        //    //expect(scope.content.id).toBe(1234);
        //});

        //it('it should have a tabs collection', function () {
        //  expect(scope.content.tabs.length).toBe(5);
        //});

        //it('it should have a properties collection on each tab', function () {
        //      $(scope.content.tabs).each(function(i, tab){
        //          expect(tab.properties.length).toBeGreaterThan(0);
        //      });
        //});

        //it('it should change updateDate on save', function () {
        //  var currentUpdateDate = scope.content.updateDate;

        //  setTimeout(function(){
        //      scope.save(scope.content);
        //      expect(scope.content.updateDate).toBeGreaterThan(currentUpdateDate);
        //      }, 1000);
        // });

        //it('it should change publishDate on publish', function () {
        //  var currentPublishDate = scope.content.publishDate;

        //  //wait a sec before you publish
        //  setTimeout(function(){
        //      scope.saveAndPublish(scope.content);
        //      expect(scope.content.publishDate).toBeGreaterThan(currentPublishDate);
        //      }, 1000);
        //});


    });
});