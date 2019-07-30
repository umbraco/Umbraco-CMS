﻿describe('serverValidationManager tests', function () {
    var serverValidationManager;

    beforeEach(module('umbraco.services'));
    
    beforeEach(inject(function ($injector) {
        serverValidationManager = $injector.get('serverValidationManager');        
    }));

    describe('managing field validation errors', function () {

        it('can add and retrieve field validation errors', function () {

            //arrange
            serverValidationManager.addFieldError("Name", "Required");

            //act
            var err = serverValidationManager.getFieldError("Name");

            //assert
            expect(err).not.toBeUndefined();
            expect(err.propertyAlias).toBeNull();
            expect(err.fieldName).toEqual("Name");
            expect(err.errorMsg).toEqual("Required");
            expect(err.culture).toEqual("invariant");
        });
        
        it('will return null for a non-existing field error', function () {

            //arrange
            serverValidationManager.addFieldError("Name", "Required");

            //act
            var err = serverValidationManager.getFieldError("DoesntExist");            

            //assert
            expect(err).toBeUndefined();
            
        });
        
        it('detects if a field error exists', function () {

            //arrange
            serverValidationManager.addFieldError("Name", "Required");

            //act            
            var err1 = serverValidationManager.hasFieldError("Name");
            var err2 = serverValidationManager.hasFieldError("DoesntExist");

            //assert
            expect(err1).toBe(true);
            expect(err2).toBe(false);

        });
        

    });
    
    describe('managing property validation errors', function () {

        it('can retrieve property validation errors for a sub field', function () {

            //arrange
            serverValidationManager.addPropertyError("myProperty", null, "value1", "Some value 1");
            serverValidationManager.addPropertyError("myProperty", null, "value2", "Another value 2");

            //act
            var err1 = serverValidationManager.getPropertyError("myProperty", null, "value1");
            var err2 = serverValidationManager.getPropertyError("myProperty", null, "value2");

            //assert            
            expect(err1).not.toBeUndefined();
            expect(err1.propertyAlias).toEqual("myProperty");
            expect(err1.fieldName).toEqual("value1");
            expect(err1.errorMsg).toEqual("Some value 1");
            expect(err1.culture).toEqual("invariant");
            
            expect(err2).not.toBeUndefined();
            expect(err2.propertyAlias).toEqual("myProperty");
            expect(err2.fieldName).toEqual("value2");
            expect(err2.errorMsg).toEqual("Another value 2");
            expect(err2.culture).toEqual("invariant");
        });

        it('can retrieve property validation errors for a sub field for culture', function () {

            //arrange
            serverValidationManager.addPropertyError("myProperty", "en-US", "value1", "Some value 1");
            serverValidationManager.addPropertyError("myProperty", "fr-FR", "value2", "Another value 2");

            //act
            var err1 = serverValidationManager.getPropertyError("myProperty", "en-US", "value1");
            var err1NotFound = serverValidationManager.getPropertyError("myProperty", null, "value2");
            var err2 = serverValidationManager.getPropertyError("myProperty", "fr-FR", "value2");
            var err2NotFound = serverValidationManager.getPropertyError("myProperty", null, "value2");


            //assert            
            expect(err1NotFound).toBeUndefined();
            expect(err2NotFound).toBeUndefined();

            expect(err1).not.toBeUndefined();
            expect(err1.propertyAlias).toEqual("myProperty");
            expect(err1.fieldName).toEqual("value1");
            expect(err1.errorMsg).toEqual("Some value 1");
            expect(err1.culture).toEqual("en-US");

            expect(err2).not.toBeUndefined();
            expect(err2.propertyAlias).toEqual("myProperty");
            expect(err2.fieldName).toEqual("value2");
            expect(err2.errorMsg).toEqual("Another value 2");
            expect(err2.culture).toEqual("fr-FR");
        });
        
        it('can add a property errors with multiple sub fields and it the first will be retreived with only the property alias', function () {

            //arrange
            serverValidationManager.addPropertyError("myProperty", null, "value1", "Some value 1");
            serverValidationManager.addPropertyError("myProperty", null, "value2", "Another value 2");

            //act
            var err = serverValidationManager.getPropertyError("myProperty");

            //assert
            expect(err).not.toBeUndefined();
            expect(err.propertyAlias).toEqual("myProperty");
            expect(err.fieldName).toEqual("value1");
            expect(err.errorMsg).toEqual("Some value 1");
            expect(err.culture).toEqual("invariant");
        });

        it('will return null for a non-existing property error', function () {

            //arrage
            serverValidationManager.addPropertyError("myProperty", null, "value", "Required");

            //act
            var err = serverValidationManager.getPropertyError("DoesntExist", null, "value");

            //assert
            expect(err).toBeUndefined();

        });

        it('detects if a property error exists', function () {

            //arrange
            serverValidationManager.addPropertyError("myProperty", null, "value1", "Some value 1");
            serverValidationManager.addPropertyError("myProperty", null, "value2", "Another value 2");

            //act
            var err1 = serverValidationManager.hasPropertyError("myProperty");
            var err2 = serverValidationManager.hasPropertyError("myProperty", null, "value1");
            var err3 = serverValidationManager.hasPropertyError("myProperty", null, "value2");
            var err4 = serverValidationManager.hasPropertyError("notFound");
            var err5 = serverValidationManager.hasPropertyError("myProperty", null, "notFound");

            //assert
            expect(err1).toBe(true);            
            expect(err2).toBe(true);
            expect(err3).toBe(true);
            expect(err4).toBe(false);
            expect(err5).toBe(false);

        });
        
        it('can remove a property error with a sub field specified', function () {

            //arrage
            serverValidationManager.addPropertyError("myProperty", null, "value1", "Some value 1");
            serverValidationManager.addPropertyError("myProperty", null, "value2", "Another value 2");

            //act
            serverValidationManager.removePropertyError("myProperty", null, "value1");

            //assert
            expect(serverValidationManager.hasPropertyError("myProperty", null, "value1")).toBe(false);
            expect(serverValidationManager.hasPropertyError("myProperty", null, "value2")).toBe(true);

        });
        
        it('can remove a property error and all sub field errors by specifying only the property', function () {

            //arrage
            serverValidationManager.addPropertyError("myProperty", null, "value1", "Some value 1");
            serverValidationManager.addPropertyError("myProperty", null, "value2", "Another value 2");

            //act
            serverValidationManager.removePropertyError("myProperty");

            //assert
            expect(serverValidationManager.hasPropertyError("myProperty", null, "value1")).toBe(false);
            expect(serverValidationManager.hasPropertyError("myProperty", null, "value2")).toBe(false);

        });

    });

    describe('managing culture validation errors', function () {

        it('can retrieve culture validation errors', function () {

            //arrange
            serverValidationManager.addPropertyError("myProperty", null, "value1", "Some value 1");
            serverValidationManager.addPropertyError("myProperty", "en-US", "value1", "Some value 2");
            serverValidationManager.addPropertyError("myProperty", null, "value2", "Another value 2");
            serverValidationManager.addPropertyError("myProperty", "fr-FR", "value2", "Another value 3");

            //assert
            expect(serverValidationManager.hasCultureError(null)).toBe(true);
            expect(serverValidationManager.hasCultureError("en-US")).toBe(true);
            expect(serverValidationManager.hasCultureError("fr-FR")).toBe(true);
            expect(serverValidationManager.hasCultureError("es-ES")).toBe(false);

        });

    });

    describe('validation error subscriptions', function() {

        it('can subscribe to a field error', function() {
            var args;

            //arrange
            serverValidationManager.subscribe(null, null, "Name", function (isValid, propertyErrors, allErrors) {
                args = {
                    isValid: isValid,
                    propertyErrors: propertyErrors,
                    allErrors: allErrors
                };
            });
            
            //act
            serverValidationManager.addFieldError("Name", "Required");
            serverValidationManager.addPropertyError("myProperty", null, "value1", "Some value 1");

            //assert
            expect(args).not.toBeUndefined();
            expect(args.isValid).toBe(false);
            expect(args.propertyErrors.length).toEqual(1);
            expect(args.propertyErrors[0].errorMsg).toEqual("Required");
            expect(args.allErrors.length).toEqual(2);
        });
        
        it('can get the field subscription callbacks', function () {
            
            //arrange
            var cb1 = function() {
            };
            var cb2 = function () {
            };
            serverValidationManager.subscribe(null, null, "Name", cb1);
            serverValidationManager.subscribe(null, null, "Title", cb2);

            //act
            serverValidationManager.addFieldError("Name", "Required");
            serverValidationManager.addFieldError("Title", "Invalid");

            //assert
            var nameCb = serverValidationManager.getFieldCallbacks("Name");
            expect(nameCb).not.toBeUndefined();
            expect(nameCb.length).toEqual(1);
            expect(nameCb[0].propertyAlias).toBeNull();
            expect(nameCb[0].fieldName).toEqual("Name");
            expect(nameCb[0].callback).toEqual(cb1);

            var titleCb = serverValidationManager.getFieldCallbacks("Title");
            expect(titleCb).not.toBeUndefined();
            expect(titleCb.length).toEqual(1);
            expect(titleCb[0].propertyAlias).toBeNull();
            expect(titleCb[0].fieldName).toEqual("Title");
            expect(titleCb[0].callback).toEqual(cb2);
        });
        
        it('can subscribe to a property error for both a property and its sub field', function () {
            var args1;
            var args2;
            var numCalled = 0;

            //arrange
            serverValidationManager.subscribe("myProperty", null, "value1", function (isValid, propertyErrors, allErrors) {
                args1 = {
                    isValid: isValid,
                    propertyErrors: propertyErrors,
                    allErrors: allErrors
                };
            });
            
            serverValidationManager.subscribe("myProperty", null, "", function (isValid, propertyErrors, allErrors) {
                numCalled++;
                args2 = {
                    isValid: isValid,
                    propertyErrors: propertyErrors,
                    allErrors: allErrors
                };
            });

            //act
            serverValidationManager.addPropertyError("myProperty", null, "value1", "Some value 1");
            serverValidationManager.addPropertyError("myProperty", null, "value2", "Some value 2");
            serverValidationManager.addPropertyError("myProperty", null, "", "Some value 3");

            //assert
            expect(args1).not.toBeUndefined();
            expect(args1.isValid).toBe(false);
            //this is only one because this subscription is targetting only a sub field
            expect(args1.propertyErrors.length).toEqual(1);
            expect(args1.propertyErrors[0].errorMsg).toEqual("Some value 1");
            //NOTE: though you might think this should be 3, it's actually correct at 2 since the last error that doesn't have a sub field
            // specified doesn't actually get added because it already detects that a property error exists for the alias.
            expect(args1.allErrors.length).toEqual(2);
            
            expect(args2).not.toBeUndefined();
            expect(args2.isValid).toBe(false);
            //This is 2 because we are looking for all property errors including sub fields
            expect(args2.propertyErrors.length).toEqual(2);
            expect(args2.propertyErrors[0].errorMsg).toEqual("Some value 1");
            expect(args2.propertyErrors[1].errorMsg).toEqual("Some value 2");
            expect(args2.allErrors.length).toEqual(2);
            //Even though only 2 errors are added, the callback is called 3 times because any call to addPropertyError will invoke the callback
            // if the property has errors existing.
            expect(numCalled).toEqual(3);
        });

        it('can subscribe to a culture error for both a property and its sub field', function () {
            var args1;
            var args2;
            var numCalled = 0;

            //arrange
            serverValidationManager.subscribe(null, "en-US", null, function (isValid, propertyErrors, allErrors) {
                numCalled++;
                args1 = {
                    isValid: isValid,
                    propertyErrors: propertyErrors,
                    allErrors: allErrors
                };
            });

            serverValidationManager.subscribe(null, "es-ES", null, function (isValid, propertyErrors, allErrors) {
                numCalled++;
                args2 = {
                    isValid: isValid,
                    propertyErrors: propertyErrors,
                    allErrors: allErrors
                };
            });

            //act
            serverValidationManager.addPropertyError("myProperty", null, "value1", "Some value 1");
            serverValidationManager.addPropertyError("myProperty", "en-US", "value1", "Some value 1");
            serverValidationManager.addPropertyError("myProperty", "en-US", "value2", "Some value 2");
            serverValidationManager.addPropertyError("myProperty", "fr-FR", "", "Some value 3");

            //assert
            expect(args1).not.toBeUndefined();
            expect(args1.isValid).toBe(false);

            expect(args2).toBeUndefined();

            expect(numCalled).toEqual(2);
        });
        
        // TODO: Finish testing the rest!

    });

});
