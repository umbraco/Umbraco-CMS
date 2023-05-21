describe('contentEditingHelper tests', function () {
    var contentEditingHelper, $routeParams, serverValidationManager, mocksUtils, notificationsService, formHelper;

    beforeEach(module('umbraco.services'));
    beforeEach(module('umbraco.mocks'));
    beforeEach(module('ngRoute'));

    beforeEach(inject(function ($injector, localizationMocks) {
        localizationMocks.register();

        contentEditingHelper = $injector.get('contentEditingHelper');
        $routeParams = $injector.get('$routeParams');
        serverValidationManager = $injector.get('serverValidationManager');
        mocksUtils = $injector.get('mocksUtils');
        notificationsService = $injector.get('notificationsService');
        formHelper = $injector.get('formHelper');
    }));

    describe('handles http validation errors', function () {

        it('handles validation errors for 400 results', function () {

            //arrange
            var content = mocksUtils.getMockContent(1234);
            var err = {
                data: content,
                status: 400
            };
            err.data.ModelState = {};

            //act
            var handled = contentEditingHelper.handleSaveError({
                err: err,
                allNewProps: contentEditingHelper.getAllProps(content),
                allOrigProps: contentEditingHelper.getAllProps(content)
            });

            //assert
            expect(handled).toBe(true);
        });
        
        it('does not handle validation errors that are not 400', function () {

            //arrange
            var err = {
                status: 410
            };
            
            //act
            var handled = contentEditingHelper.handleSaveError({
                err: err,
                allNewProps: [],
                allOrigProps: []
            });

            //assert
            expect(handled).toBe(false);
        });
        
        it('does not handle validation errors that are 400 without model state', function () {

            //arrange
            var content = mocksUtils.getMockContent(1234);
            var err = {
                data: content,
                status: 400
            };

            //act
            var handled = contentEditingHelper.handleSaveError({
                err: err,
                allNewProps: contentEditingHelper.getAllProps(content),
                allOrigProps: contentEditingHelper.getAllProps(content)
            });

            //assert
            expect(handled).toBe(false);
        });

    });

    describe('handling validation errors', function () {

        it('adds a field level server validation error when name is invalid', function () {

            //arrange
            var content = mocksUtils.getMockContent(1234);
            var allProps = contentEditingHelper.getAllProps(content);

            //act
            formHelper.handleServerValidation({ Name: ["Required"] });

            //assert
            expect(serverValidationManager.items.length).toBe(1);
            expect(serverValidationManager.items[0].fieldName).toBe("Name");
            expect(serverValidationManager.items[0].errorMsg).toBe("Required"); 
            expect(serverValidationManager.items[0].propertyAlias).toBe(null);
        });

        it('adds a property level server validation error when a property is invalid', function () {

            //arrange
            var content = mocksUtils.getMockContent(1234);
            var allProps = contentEditingHelper.getAllProps(content);

            //act
            formHelper.handleServerValidation({ "_Properties.bodyText": ["Required"] });

            //assert
            expect(serverValidationManager.items.length).toBe(1);
            expect(serverValidationManager.items[0].fieldName).toBe("");
            expect(serverValidationManager.items[0].errorMsg).toBe("Required");
            expect(serverValidationManager.items[0].propertyAlias).toBe("bodyText");
        });
        
        it('adds a property level server validation error with a specific field when a property is invalid', function () {

            //arrange
            var content = mocksUtils.getMockContent(1234);
            var allProps = contentEditingHelper.getAllProps(content);

            //act
            //note the null, that's because culture is null
            formHelper.handleServerValidation({ "_Properties.bodyText.null.null.value": ["Required"] });

            //assert
            expect(serverValidationManager.items.length).toBe(1);
            expect(serverValidationManager.items[0].fieldName).toBe("value");
            expect(serverValidationManager.items[0].errorMsg).toBe("Required");
            expect(serverValidationManager.items[0].propertyAlias).toBe("bodyText");
            expect(serverValidationManager.items[0].culture).toBe("invariant");
            expect(serverValidationManager.items[0].segment).toBeNull();
        });
        
        it('adds a multiple property and field level server validation errors when they are invalid', function () {

            //arrange
            var content = mocksUtils.getMockContent(1234);
            var allProps = contentEditingHelper.getAllProps(content);

            //act
            formHelper.handleServerValidation(
                {
                    "Name": ["Required"],
                    "UpdateDate": ["Invalid date"],
                    //note the null, that's because culture is null
                    "_Properties.bodyText.en-US.mySegment.value": ["Required field"],
                    "_Properties.textarea": ["Invalid format"]
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
            expect(serverValidationManager.items[2].culture).toBe("en-US");
            expect(serverValidationManager.items[2].segment).toBe("mySegment");
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
        
        it('does not redirect when creating content with an invalid id', function () {
            
            //arrange
            $routeParams.create = true;

            //act
            var result = contentEditingHelper.redirectToCreatedContent(0, {Name: ["Required"]});

            //assert
            expect(result).toBe(false);
            
        });

    });
    
    describe('wires up property values after saving', function () {

        it('does not update un-changed values', function () {

            //arrange
            var origContent = mocksUtils.getMockContent(1234);
            origContent.save = true;
            origContent.tabs[0].properties[0].value = { complex1: "origValue1a", complex2: "origValue1b" };
            origContent.tabs[1].properties[0].value = "origValue2";
            origContent.tabs[1].properties[1].value = "origValue3";
            origContent.tabs[1].properties[2].value = "origValue4";

            var newContent = mocksUtils.getMockContent(1234);
            newContent.tabs[0].properties[0].value = { complex1: "origValue1a", complex2: "newValue1b" };
            newContent.tabs[1].properties[0].value = "origValue2";
            newContent.tabs[1].properties[1].value = "newValue3";
            newContent.tabs[1].properties[2].value = "origValue4";

            //act
            var changed = contentEditingHelper.reBindChangedProperties(origContent, newContent);

            
            //assert
            expect(changed.length).toBe(2);
            expect(changed[0].alias).toBe("grid");
            expect(changed[1].alias).toBe("bodyText");
        });

    });
});
