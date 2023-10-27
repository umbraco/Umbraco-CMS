describe('Content picker controller tests', function () {
    var scope, controller, httpBackend;
    routeParams = {id: 1234, create: false};

    beforeEach(module('umbraco'));

    //inject the contentMocks service
    beforeEach(inject(function ($rootScope, $controller, angularHelper, $httpBackend, entityMocks, mocksUtils, localizationMocks, userMocks) {

        //for these tests we don't want any authorization to occur
        mocksUtils.setAuth();
		
        httpBackend = $httpBackend;
        scope = $rootScope.$new();

        scope.model = {
                        alias: "property",
                        value:"1233,1231,23121",
                        label: "My content picker",
                        description: "desc",
                        config: {
                            startNode: {
                                type: "content"
                            }
                        }
                      };

        //this controller requires an angular form controller applied to it
        scope.contentPickerForm = angularHelper.getNullForm("contentPickerForm");
        scope.contentPickerForm.minCount = angularHelper.getNullForm("minCount");
        scope.contentPickerForm.maxCount = angularHelper.getNullForm("maxCount");
        
        //have the contentMocks register its expect urls on the httpbackend
        //see /mocks/content.mocks.js for how its setup
        entityMocks.register();
        localizationMocks.register();
        userMocks.register();

        controller = $controller('Umbraco.PropertyEditors.ContentPickerController', {
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
        
        var item;

        beforeEach(function () {
            item = {
                name: "meh",
                id: 666,
                icon: "woop"
            };
        });

        it('should define the default properties on construction', function () {
            expect(scope.model.value).not.toBeUndefined();            
        });
        
        it("should populate scope.renderModel", function(){
            expect(scope.renderModel).not.toBeUndefined();
            expect(scope.renderModel.length).toBe(3);
        });

        it("Each rendermodel item should contain name, id and icon", function(){
            var item = scope.renderModel[0];
            expect(item.name).not.toBeUndefined();
            expect(item.id).toBe(1233);
            expect(item.icon).not.toBeUndefined();
        });

        it("Removing an item should update renderModel, ids and model.value", function(){
            scope.remove(1);
            scope.$apply();
            expect(scope.renderModel.length).toBe(2);
            expect(scope.model.value).toBe("1233,23121");
        });

        it("Adding an item should update renderModel, ids and model.value", function(){
            
            scope.add(item);
            scope.$apply();
            httpBackend.flush();

            expect(scope.renderModel.length).toBe(4);
            expect(scope.model.value).toBe("1233,1231,23121,666");
        });

        it("Adding a duplicate item should note update renderModel, ids and model.value", function(){
            scope.add(item);
            scope.$apply();
            httpBackend.flush();
            expect(scope.renderModel.length).toBe(4);
            expect(scope.model.value).toBe("1233,1231,23121,666");

            scope.add(item);
            scope.$apply();
            
            expect(scope.renderModel.length).toBe(4);
            expect(scope.model.value).toBe("1233,1231,23121,666");

        });  
    });
});
