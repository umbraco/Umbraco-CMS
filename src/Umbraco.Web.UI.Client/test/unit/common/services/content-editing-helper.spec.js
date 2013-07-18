describe('contentEditingHelper tests', function () {
    var contentEditingHelper, $routeParams, serverValidationManager, mocksUtils, notificationsService;

    beforeEach(module('umbraco.services'));
    beforeEach(module('umbraco.mocks'));

    beforeEach(inject(function ($injector) {
        contentEditingHelper = $injector.get('contentEditingHelper');
        $routeParams = $injector.get('$routeParams');
        serverValidationManager = $injector.get('serverValidationManager');
        mocksUtils = $injector.get('mocksUtils');
        notificationsService = $injector.get('notificationsService');
    }));

    describe('handles http validation errors', function () {

        it('handles validation errors for 403 results', function () {

            //arrange
            var content = mocksUtils.getMockContent(1234);
            var err = {
                data: content,
                status: 403
            };
            err.data.ModelState = {};

            //act
            var handled = contentEditingHelper.handleSaveError(err);

            //assert
            expect(handled).toBe(true);
        });
        
        it('does not handle validation errors that are not 403', function () {

            //arrange
            var err = {
                status: 400
            };
            
            //act
            var handled = contentEditingHelper.handleSaveError(err);

            //assert
            expect(handled).toBe(false);
        });
        
        it('does not handle validation errors that are 403 without model state', function () {

            //arrange
            var content = mocksUtils.getMockContent(1234);
            var err = {
                data: content,
                status: 403
            };

            //act
            var handled = contentEditingHelper.handleSaveError(err);

            //assert
            expect(handled).toBe(false);
        });

    });

    describe('handling validation errors', function () {

        it('adds a field level server validation error when name is invalid', function () {

            //arrange
            var content = mocksUtils.getMockContent(1234);

            //act
            contentEditingHelper.handleValidationErrors(content, { Name: ["Required"] });

            //assert
            expect(serverValidationManager.items.length).toBe(1);
            expect(serverValidationManager.items[0].fieldName).toBe("Name");
            expect(serverValidationManager.items[0].errorMsg).toBe("Required"); 
            expect(serverValidationManager.items[0].propertyAlias).toBe(null);
        });

        it('adds a property level server validation error when a property is invalid', function () {

            //arrange
            var content = mocksUtils.getMockContent(1234);

            //act
            contentEditingHelper.handleValidationErrors(content, { "Property.bodyText": ["Required"] });

            //assert
            expect(serverValidationManager.items.length).toBe(1);
            expect(serverValidationManager.items[0].fieldName).toBe("");
            expect(serverValidationManager.items[0].errorMsg).toBe("Required");
            expect(serverValidationManager.items[0].propertyAlias).toBe("bodyText");
        });
        
        it('adds a property level server validation error with a specific field when a property is invalid', function () {

            //arrange
            var content = mocksUtils.getMockContent(1234);

            //act
            contentEditingHelper.handleValidationErrors(content, { "Property.bodyText.value": ["Required"] });

            //assert
            expect(serverValidationManager.items.length).toBe(1);
            expect(serverValidationManager.items[0].fieldName).toBe("value");
            expect(serverValidationManager.items[0].errorMsg).toBe("Required");
            expect(serverValidationManager.items[0].propertyAlias).toBe("bodyText");
        });
        
        it('adds a multiple property and field level server validation errors when they are invalid', function () {

            //arrange
            var content = mocksUtils.getMockContent(1234);

            //act
            contentEditingHelper.handleValidationErrors(
                content,
                {
                    "Name": ["Required"],
                    "UpdateDate": ["Invalid date"],
                    "Property.bodyText.value": ["Required field"],
                    "Property.textarea": ["Invalid format"]
                });

            //assert
            expect(serverValidationManager.items.length).toBe(4);
            expect(serverValidationManager.items[0].fieldName).toBe("Name");
            expect(serverValidationManager.items[0].errorMsg).toBe("Required");
            expect(serverValidationManager.items[0].propertyAlias).toBe(null);
            expect(serverValidationManager.items[1].fieldName).toBe("UpdateDate");
            expect(serverValidationManager.items[1].errorMsg).toBe("Invalid date");
            expect(serverValidationManager.items[1].propertyAlias).toBe(null);
            expect(serverValidationManager.items[2].fieldName).toBe("value");
            expect(serverValidationManager.items[2].errorMsg).toBe("Required field");
            expect(serverValidationManager.items[2].propertyAlias).toBe("bodyText");
            expect(serverValidationManager.items[3].fieldName).toBe("");
            expect(serverValidationManager.items[3].errorMsg).toBe("Invalid format");
            expect(serverValidationManager.items[3].propertyAlias).toBe("textarea");
        });

    });

    describe('redirecting behavior after saving content', function () {

        it('does not redirect when editing existing content', function () {

            //arrange
            $routeParams.create = undefined;

            //act
            var result = contentEditingHelper.redirectToCreatedContent(1234, null);

            //assert
            expect(result).toBe(false);
        });
        
        it('does a redirect when creating content with a valid name', function () {
            
            //arrange
            $routeParams.create = true;

            //act
            var result = contentEditingHelper.redirectToCreatedContent(1234, null);

            //assert
            expect(result).toBe(true);
        });
        
        it('does not redirect when creating content with an invalid name', function () {
            
            //arrange
            $routeParams.create = true;

            //act
            var result = contentEditingHelper.redirectToCreatedContent(1234, {Name: ["Required"]});

            //assert
            expect(result).toBe(false);
            
        });

    });
});