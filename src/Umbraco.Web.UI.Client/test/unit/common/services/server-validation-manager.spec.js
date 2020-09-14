describe('serverValidationManager tests', function () {
    var $rootScope, serverValidationManager, $timeout;

    beforeEach(module('umbraco.services'));
    
    beforeEach(inject(function ($injector) {
        $rootScope = $injector.get('$rootScope');
        $timeout = $injector.get('$timeout');
        serverValidationManager = $injector.get('serverValidationManager');
        serverValidationManager.clear();
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
            serverValidationManager.addPropertyError("myProperty", null, "value1", "Some value 1", null);
            serverValidationManager.addPropertyError("myProperty", null, "value2", "Another value 2");

            //act
            var err1 = serverValidationManager.getPropertyError("myProperty", null, "value1", null);
            var err2 = serverValidationManager.getPropertyError("myProperty", null, "value2", null);

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
            serverValidationManager.addPropertyError("myProperty", "en-US", "value1", "Some value 1", null);
            serverValidationManager.addPropertyError("myProperty", "fr-FR", "value2", "Another value 2", null);

            //act
            var err1 = serverValidationManager.getPropertyError("myProperty", "en-US", "value1", null);
            var err1NotFound = serverValidationManager.getPropertyError("myProperty", null, "value1", null);
            var err2 = serverValidationManager.getPropertyError("myProperty", "fr-FR", "value2", null);
            var err2NotFound = serverValidationManager.getPropertyError("myProperty", null, "value2", null);


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

        it('can retrieve property validation errors for a sub field for segments', function () {

            //arrange
            serverValidationManager.addPropertyError("myProperty", null, "value1", "Some value 1", "segment1");
            serverValidationManager.addPropertyError("myProperty", null, "value2", "Another value 2", "segment2");

            //act
            var err1 = serverValidationManager.getPropertyError("myProperty", null, "value1", "segment1");
            var err1NotFound = serverValidationManager.getPropertyError("myProperty", null, "value1", null);
            var err2 = serverValidationManager.getPropertyError("myProperty", null, "value2", "segment2");
            var err2NotFound = serverValidationManager.getPropertyError("myProperty", null, "value2", null);


            //assert            
            expect(err1NotFound).toBeUndefined();
            expect(err2NotFound).toBeUndefined();

            expect(err1).not.toBeUndefined();
            expect(err1.propertyAlias).toEqual("myProperty");
            expect(err1.fieldName).toEqual("value1");
            expect(err1.errorMsg).toEqual("Some value 1");
            expect(err1.segment).toEqual("segment1");

            expect(err2).not.toBeUndefined();
            expect(err2.propertyAlias).toEqual("myProperty");
            expect(err2.fieldName).toEqual("value2");
            expect(err2.errorMsg).toEqual("Another value 2");
            expect(err2.segment).toEqual("segment2");
        });

        
        it('can retrieve property validation errors for a sub field for culture with segments', function () {

            //arrange
            serverValidationManager.addPropertyError("myProperty", "en-US", "value1", "Some value 1", "segment1");
            serverValidationManager.addPropertyError("myProperty", "fr-FR", "value2", "Another value 2", "segment2");

            //act
            var err1 = serverValidationManager.getPropertyError("myProperty", "en-US", "value1", "segment1");
            expect(serverValidationManager.getPropertyError("myProperty", null, "value1", null)).toBeUndefined();
            expect(serverValidationManager.getPropertyError("myProperty", "en-US", "value1", null)).toBeUndefined();
            expect(serverValidationManager.getPropertyError("myProperty", null, "value1", "segment1")).toBeUndefined();
            var err2 = serverValidationManager.getPropertyError("myProperty", "fr-FR", "value2", "segment2");
            expect(serverValidationManager.getPropertyError("myProperty", null, "value2", null)).toBeUndefined();
            expect(serverValidationManager.getPropertyError("myProperty", "fr-FR", "value2", null)).toBeUndefined();
            expect(serverValidationManager.getPropertyError("myProperty", null, "value2", "segment2")).toBeUndefined();


            //assert

            expect(err1).not.toBeUndefined();
            expect(err1.propertyAlias).toEqual("myProperty");
            expect(err1.fieldName).toEqual("value1");
            expect(err1.errorMsg).toEqual("Some value 1");
            expect(err1.culture).toEqual("en-US");
            expect(err1.segment).toEqual("segment1");

            expect(err2).not.toBeUndefined();
            expect(err2.propertyAlias).toEqual("myProperty");
            expect(err2.fieldName).toEqual("value2");
            expect(err2.errorMsg).toEqual("Another value 2");
            expect(err2.culture).toEqual("fr-FR");
            expect(err2.segment).toEqual("segment2");
        });
        
        it('can add a property errors with multiple sub fields and it the first will be retreived with only the property alias', function () {

            //arrange
            serverValidationManager.addPropertyError("myProperty", null, "value1", "Some value 1", null);
            serverValidationManager.addPropertyError("myProperty", null, "value2", "Another value 2", null);

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
            serverValidationManager.addPropertyError("myProperty", null, "value", "Required", null);

            //act
            var err = serverValidationManager.getPropertyError("DoesntExist", null, "value", null);

            //assert
            expect(err).toBeUndefined();

        });

        it('detects if a property error exists', function () {

            //arrange
            serverValidationManager.addPropertyError("myProperty", null, "value1", "Some value 1", null);
            serverValidationManager.addPropertyError("myProperty", null, "value2", "Another value 2", null);

            //act
            var err1 = serverValidationManager.hasPropertyError("myProperty");
            var err2 = serverValidationManager.hasPropertyError("myProperty", null, "value1", null);
            var err3 = serverValidationManager.hasPropertyError("myProperty", null, "value2", null);
            var err4 = serverValidationManager.hasPropertyError("notFound");
            var err5 = serverValidationManager.hasPropertyError("myProperty", null, "notFound", null);

            //assert
            expect(err1).toBe(true);            
            expect(err2).toBe(true);
            expect(err3).toBe(true);
            expect(err4).toBe(false);
            expect(err5).toBe(false);

        });
        
        it('can remove a property error with a sub field specified', function () {

            //arrage
            serverValidationManager.addPropertyError("myProperty", null, "value1", "Some value 1", null);
            serverValidationManager.addPropertyError("myProperty", null, "value2", "Another value 2", null);

            //act
            serverValidationManager.removePropertyError("myProperty", null, "value1", null);

            //assert
            expect(serverValidationManager.hasPropertyError("myProperty", null, "value1", null)).toBe(false);
            expect(serverValidationManager.hasPropertyError("myProperty", null, "value2", null)).toBe(true);

        });
        
        it('can remove a property error and all sub field errors by specifying only the property', function () {

            //arrage
            serverValidationManager.addPropertyError("myProperty", null, "value1", "Some value 1");
            serverValidationManager.addPropertyError("myProperty", null, "value2", "Another value 2");

            //act
            serverValidationManager.removePropertyError("myProperty");

            //assert
            expect(serverValidationManager.hasPropertyError("myProperty", null, "value1", null)).toBe(false);
            expect(serverValidationManager.hasPropertyError("myProperty", null, "value2", null)).toBe(false);

        });

    });

    describe('managing culture validation errors', function () {

        it('can retrieve culture validation errors', function () {

            //arrange
            serverValidationManager.addPropertyError("myProperty", null, "value1", "Some value 1", null);
            serverValidationManager.addPropertyError("myProperty", "en-US", "value1", "Some value 2", null);
            serverValidationManager.addPropertyError("myProperty", null, "value2", "Another value 2", null);
            serverValidationManager.addPropertyError("myProperty", "fr-FR", "value2", "Another value 3", null);

            //assert
            expect(serverValidationManager.hasCultureError(null)).toBe(true);
            expect(serverValidationManager.hasCultureError("en-US")).toBe(true);
            expect(serverValidationManager.hasCultureError("fr-FR")).toBe(true);
            expect(serverValidationManager.hasCultureError("es-ES")).toBe(false);

        });

    });

    describe('managing variant validation errors', function () {

        it('can retrieve variant validation errors', function () {

            //arrange
            serverValidationManager.addPropertyError("myProperty", null, "value1", "Some value 1", null);
            serverValidationManager.addPropertyError("myProperty", "en-US", "value1", "Some value 2", null);
            serverValidationManager.addPropertyError("myProperty", null, "value2", "Another value 2", null);
            serverValidationManager.addPropertyError("myProperty", "fr-FR", "value2", "Another value 3", null);

            serverValidationManager.addPropertyError("myProperty", null, "value1", "Some value 1", "MySegment");
            serverValidationManager.addPropertyError("myProperty", "en-US", "value1", "Some value 2", "MySegment");
            serverValidationManager.addPropertyError("myProperty", null, "value2", "Another value 2", "MySegment");
            serverValidationManager.addPropertyError("myProperty", "fr-FR", "value2", "Another value 3", "MySegment");

            //assert
            expect(serverValidationManager.hasVariantError(null, null)).toBe(true);
            expect(serverValidationManager.hasVariantError("en-US", null)).toBe(true);
            expect(serverValidationManager.hasVariantError("fr-FR", null)).toBe(true);

            expect(serverValidationManager.hasVariantError(null, "MySegment")).toBe(true);
            expect(serverValidationManager.hasVariantError("en-US", "MySegment")).toBe(true);
            expect(serverValidationManager.hasVariantError("fr-FR", "MySegment")).toBe(true);

            expect(serverValidationManager.hasVariantError("es-ES", null)).toBe(false);
            expect(serverValidationManager.hasVariantError("es-ES", "MySegment")).toBe(false);
            expect(serverValidationManager.hasVariantError("fr-FR", "MySegmentNotRight")).toBe(false);
            expect(serverValidationManager.hasVariantError(null, "MySegmentNotRight")).toBe(false);

        });

    });

    describe('managing complex editor validation errors', function () {

        // this root element doesn't have it's own attached errors, instead it has model state just 
        // showing that it has errors within it's nested properties. that ModelState is automatically
        // added on the server side.
        var nonRootLevelComplexValidationMsg = `[
    {
        "$elementTypeAlias": "addressBook",
		"$id": "34E3A26C-103D-4A05-AB9D-7E14032309C3",
        "addresses":
        [
			{
				"$elementTypeAlias": "addressInfo",
				"$id": "FBEAEE8F-4BC9-43EE-8B81-FCA8978850F1",
				"ModelState":
                {
                    "_Properties.city.invariant.null.country": [
                        "City is not in Australia"
                    ],
                    "_Properties.city.invariant.null.capital": [
                        "Not a capital city"
                    ]
                }
			},
			{
				"$elementTypeAlias": "addressInfo",
				"$id": "7170A4DD-2441-4B1B-A8D3-437D75C4CBC9",
				"ModelState":
                {
                    "_Properties.city.invariant.null.country": [
                        "City is not in Australia"
                    ],
                    "_Properties.city.invariant.null.capital": [
                        "Not a capital city"
                    ]
                }
			}
        ],
        "ModelState":
        {
            "_Properties.addresses.invariant.null": [
                ""
            ]
        }
    }
]`;

        it('create dictionary of id to ModelState', function () {

            //arrange
            var complexValidationMsg = `[
    {
        "$elementTypeAlias": "addressBook",
		"$id": "34E3A26C-103D-4A05-AB9D-7E14032309C3",
        "addresses":
        [
			{
				"$elementTypeAlias": "addressInfo",
				"$id": "FBEAEE8F-4BC9-43EE-8B81-FCA8978850F1",
				"ModelState":
                {
                    "_Properties.city.invariant.null.country": [
                        "City is not in Australia"
                    ],
                    "_Properties.city.invariant.null.capital": [
                        "Not a capital city"
                    ]
                }
			},
			{
				"$elementTypeAlias": "addressInfo",
				"$id": "7170A4DD-2441-4B1B-A8D3-437D75C4CBC9",
				"ModelState":
                {
                    "_Properties.city.invariant.null.country": [
                        "City is not in Australia"
                    ],
                    "_Properties.city.invariant.null.capital": [
                        "Not a capital city"
                    ]
                }
			}
        ],
        "ModelState":
        {
            "_Properties.addresses.invariant.null.counter": [
                "Must have at least 3 addresses"
            ],
            "_Properties.bookName.invariant.null.book": [
                "Invalid address book name"
            ]
        }
    }
]`;
            
            //act 
            var ms = serverValidationManager.parseComplexEditorError(complexValidationMsg, "myBlockEditor");

            //assert
            expect(ms.length).toEqual(3);
            expect(ms[0].validationPath).toEqual("myBlockEditor/34E3A26C-103D-4A05-AB9D-7E14032309C3");
            expect(ms[1].validationPath).toEqual("myBlockEditor/34E3A26C-103D-4A05-AB9D-7E14032309C3/addresses/FBEAEE8F-4BC9-43EE-8B81-FCA8978850F1");
            expect(ms[2].validationPath).toEqual("myBlockEditor/34E3A26C-103D-4A05-AB9D-7E14032309C3/addresses/7170A4DD-2441-4B1B-A8D3-437D75C4CBC9");

        });

        it('create dictionary of id to ModelState with inherited errors', function () {

            //act 
            var ms = serverValidationManager.parseComplexEditorError(nonRootLevelComplexValidationMsg, "myBlockEditor");

            //assert
            expect(ms.length).toEqual(3);
            expect(ms[0].validationPath).toEqual("myBlockEditor/34E3A26C-103D-4A05-AB9D-7E14032309C3");
            var item0ModelState = ms[0].modelState;
            expect(Object.keys(item0ModelState).length).toEqual(1);
            expect(item0ModelState["_Properties.addresses.invariant.null"].length).toEqual(1);
            expect(item0ModelState["_Properties.addresses.invariant.null"][0]).toEqual("");
            expect(ms[1].validationPath).toEqual("myBlockEditor/34E3A26C-103D-4A05-AB9D-7E14032309C3/addresses/FBEAEE8F-4BC9-43EE-8B81-FCA8978850F1");
            expect(ms[2].validationPath).toEqual("myBlockEditor/34E3A26C-103D-4A05-AB9D-7E14032309C3/addresses/7170A4DD-2441-4B1B-A8D3-437D75C4CBC9");

        });

        it('add errors for ModelState with inherited errors', function () {

            //act 
            let modelState = {
                "_Properties.blockFeatures.invariant.null": [
                    nonRootLevelComplexValidationMsg
                ]
            };
            serverValidationManager.addErrorsForModelState(modelState);

            //assert
            var propertyErrors = [
                "blockFeatures",
                "blockFeatures/34E3A26C-103D-4A05-AB9D-7E14032309C3/addresses",
                "blockFeatures/34E3A26C-103D-4A05-AB9D-7E14032309C3/addresses/FBEAEE8F-4BC9-43EE-8B81-FCA8978850F1/city",
                "blockFeatures/34E3A26C-103D-4A05-AB9D-7E14032309C3/addresses/7170A4DD-2441-4B1B-A8D3-437D75C4CBC9/city"
            ]
            // These will all exist
            propertyErrors.forEach(x => expect(serverValidationManager.getPropertyError(x)).toBeDefined());

            // These field errors also exist
            expect(serverValidationManager.getPropertyError(propertyErrors[2], null, "country")).toBeDefined();
            expect(serverValidationManager.getPropertyError(propertyErrors[2], null, "capital")).toBeDefined();
            expect(serverValidationManager.getPropertyError(propertyErrors[3], null, "country")).toBeDefined();
            expect(serverValidationManager.getPropertyError(propertyErrors[3], null, "capital")).toBeDefined();
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
            }, null);

            

            //act
            serverValidationManager.addFieldError("Name", "Required");
            serverValidationManager.addPropertyError("myProperty", null, "value1", "Some value 1", null);

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
            serverValidationManager.subscribe(null, null, "Name", cb1, null);
            
            serverValidationManager.subscribe(null, null, "Title", cb2, null);
            

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
            var numCalledWithErrors = 0;

            //arrange
            serverValidationManager.subscribe("myProperty", null, "value1", function (isValid, propertyErrors, allErrors) {
                args1 = {
                    isValid: isValid,
                    propertyErrors: propertyErrors,
                    allErrors: allErrors
                };
            }, null);
            
            
            serverValidationManager.subscribe("myProperty", null, "", function (isValid, propertyErrors, allErrors) {
                numCalled++;
                if (propertyErrors.length > 0) {
                    numCalledWithErrors++;
                }
                args2 = {
                    isValid: isValid,
                    propertyErrors: propertyErrors,
                    allErrors: allErrors
                };
            }, null);
            
            //act
            serverValidationManager.addPropertyError("myProperty", null, "value1", "Some value 1", null);
            serverValidationManager.addPropertyError("myProperty", null, "value2", "Some value 2", null);
            serverValidationManager.addPropertyError("myProperty", null, "", "Some value 3", null);

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
            //3 errors are added but a call to subscribe also calls the callback
            expect(numCalled).toEqual(4);
            expect(numCalledWithErrors).toEqual(3);
        });

        it('can subscribe to a culture error for both a property and its sub field', function () {
            var args1;
            var args2;
            var numCalled = 0;
            var numCalledWithErrors = 0;

            //arrange
            serverValidationManager.subscribe(null, "en-US", null, function (isValid, propertyErrors, allErrors) {
                numCalled++;
                if (propertyErrors.length > 0) {
                    numCalledWithErrors++;
                }
                args1 = {
                    isValid: isValid,
                    propertyErrors: propertyErrors,
                    allErrors: allErrors
                };
            }, null);
            

            serverValidationManager.subscribe(null, "es-ES", null, function (isValid, propertyErrors, allErrors) {
                numCalled++;
                if (propertyErrors.length > 0) {
                    numCalledWithErrors++;
                }
                args2 = {
                    isValid: isValid,
                    propertyErrors: propertyErrors,
                    allErrors: allErrors
                };
            }, null);

            //act
            serverValidationManager.addPropertyError("myProperty", null, "value1", "Some value 1", null);       // doesn't match
            serverValidationManager.addPropertyError("myProperty", "en-US", "value1", "Some value 1", null);    // matches callback 1
            serverValidationManager.addPropertyError("myProperty", "en-US", "value2", "Some value 2", null);    // matches callback 1
            serverValidationManager.addPropertyError("myProperty", "fr-FR", "", "Some value 3", null);          // doesn't match - but all callbacks still execute

            //assert
            expect(args1.isValid).toBe(false);
            expect(args2.isValid).toBe(true); // no errors registered for this callback

            expect(numCalled).toEqual(10); // both subscriptions will be called once per addPropertyError and also called on subscribe
            expect(numCalledWithErrors).toEqual(3); // the first subscription is called 3 times with errors because the 4th time we call addPropertyError all callbacks still execute
        });

        it('can subscribe to a property validation path prefix', function () {
            var callbackA = [];
            var callbackB = [];

            //arrange
            serverValidationManager.subscribe("myProperty", null, null, function (isValid, propertyErrors, allErrors) {
                if (propertyErrors.length > 0) {
                    callbackA.push(propertyErrors);
                }
            }, null, { matchType: "prefix" });

            serverValidationManager.subscribe("myProperty/34E3A26C-103D-4A05-AB9D-7E14032309C3/addresses", null, null, function (isValid, propertyErrors, allErrors) {
                if (propertyErrors.length > 0) {
                    callbackB.push(propertyErrors);
                }
            }, null, { matchType: "prefix" });

            //act
            // will match A:
            serverValidationManager.addPropertyError("myProperty", null, null, "property error", null);
            serverValidationManager.addPropertyError("myProperty", null, "value1", "value error", null);
            // will match A + B
            serverValidationManager.addPropertyError("myProperty/34E3A26C-103D-4A05-AB9D-7E14032309C3/addresses/FBEAEE8F-4BC9-43EE-8B81-FCA8978850F1/city", null, null, "property error", null);
            serverValidationManager.addPropertyError("myProperty/34E3A26C-103D-4A05-AB9D-7E14032309C3/addresses/FBEAEE8F-4BC9-43EE-8B81-FCA8978850F1/city", null, "value1", "value error", null);
            // won't match:
            serverValidationManager.addPropertyError("myProperty", "en-US", null, "property error", null);
            serverValidationManager.addPropertyError("myProperty/34E3A26C-103D-4A05-AB9D-7E14032309C3/addresses/FBEAEE8F-4BC9-43EE-8B81-FCA8978850F1/city", "en-US", null, "property error", null);
            serverValidationManager.addPropertyError("otherProperty", null, null, "property error", null);
            serverValidationManager.addPropertyError("otherProperty", null, "value1", "value error", null);

            //assert

            // both will be called each time addPropertyError is called
            expect(callbackA.length).toEqual(8);
            expect(callbackB.length).toEqual(6); // B - will only be called 6 times with errors because the first 2 calls to addPropertyError haven't added errors for B yet
            expect(callbackA[callbackA.length - 1].length).toEqual(4); // 4 errors for A
            expect(callbackB[callbackB.length - 1].length).toEqual(2); // 2 errors for B

            // clear the data and notify
            callbackA = [];
            callbackB = [];

            serverValidationManager.notify();
            $timeout.flush();

            expect(callbackA.length).toEqual(1);
            expect(callbackB.length).toEqual(1);
            expect(callbackA[0].length).toEqual(4); // 4 errors for A
            expect(callbackB[0].length).toEqual(2); // 2 errors for B
            
        });

        it('can subscribe to a property validation path suffix', function () {
            var callbackA = [];
            var callbackB = [];

            //arrange
            serverValidationManager.subscribe("myProperty", null, null, function (isValid, propertyErrors, allErrors) {
                if (propertyErrors.length > 0) {
                    callbackA.push(propertyErrors);
                }
            }, null, { matchType: "suffix" });

            serverValidationManager.subscribe("city", null, null, function (isValid, propertyErrors, allErrors) {
                if (propertyErrors.length > 0) {
                    callbackB.push(propertyErrors);
                }
            }, null, { matchType: "suffix" });

            //act
            // will match A:
            serverValidationManager.addPropertyError("myProperty", null, null, "property error", null);
            serverValidationManager.addPropertyError("myProperty", null, "value1", "value error", null);
            // will match B
            serverValidationManager.addPropertyError("myProperty/34E3A26C-103D-4A05-AB9D-7E14032309C3/addresses/FBEAEE8F-4BC9-43EE-8B81-FCA8978850F1/city", null, null, "property error", null);
            serverValidationManager.addPropertyError("myProperty/34E3A26C-103D-4A05-AB9D-7E14032309C3/addresses/FBEAEE8F-4BC9-43EE-8B81-FCA8978850F1/city", null, "value1", "value error", null);
            // won't match:
            serverValidationManager.addPropertyError("myProperty", "en-US", null, "property error", null);
            serverValidationManager.addPropertyError("myProperty/34E3A26C-103D-4A05-AB9D-7E14032309C3/addresses/FBEAEE8F-4BC9-43EE-8B81-FCA8978850F1/city", "en-US", null, "property error", null);
            serverValidationManager.addPropertyError("otherProperty", null, null, "property error", null);
            serverValidationManager.addPropertyError("otherProperty", null, "value1", "value error", null);

            //assert

            // both will be called each time addPropertyError is called
            expect(callbackA.length).toEqual(8);
            expect(callbackB.length).toEqual(6); // B - will only be called 6 times with errors because the first 2 calls to addPropertyError haven't added errors for B yet
            expect(callbackA[callbackA.length - 1].length).toEqual(2); // 2 errors for A
            expect(callbackB[callbackB.length - 1].length).toEqual(2); // 2 errors for B

            // clear the data and notify
            callbackA = [];
            callbackB = [];

            serverValidationManager.notify();
            $timeout.flush();

            expect(callbackA.length).toEqual(1);
            expect(callbackB.length).toEqual(1);
            expect(callbackA[0].length).toEqual(2); // 2 errors for A
            expect(callbackB[0].length).toEqual(2); // 2 errors for B

        });

        it('can subscribe to a property validation path contains', function () {
            var callbackA = [];

            //arrange
            serverValidationManager.subscribe("addresses", null, null, function (isValid, propertyErrors, allErrors) {
                if (propertyErrors.length > 0) {
                    callbackA.push(propertyErrors);
                }
            }, null, { matchType: "contains" });

            //act
            // will match A:
            serverValidationManager.addPropertyError("addresses", null, null, "property error", null);
            serverValidationManager.addPropertyError("addresses", null, "value1", "value error", null);
            serverValidationManager.addPropertyError("myProperty/34E3A26C-103D-4A05-AB9D-7E14032309C3/addresses/FBEAEE8F-4BC9-43EE-8B81-FCA8978850F1/city", null, null, "property error", null);
            serverValidationManager.addPropertyError("myProperty/34E3A26C-103D-4A05-AB9D-7E14032309C3/addresses/FBEAEE8F-4BC9-43EE-8B81-FCA8978850F1/city", null, "value1", "value error", null);
            // won't match:
            serverValidationManager.addPropertyError("addresses", "en-US", null, "property error", null);
            serverValidationManager.addPropertyError("addresses/34E3A26C-103D-4A05-AB9D-7E14032309C3/addresses/FBEAEE8F-4BC9-43EE-8B81-FCA8978850F1/city", "en-US", null, "property error", null);
            serverValidationManager.addPropertyError("otherProperty", null, null, "property error", null);
            serverValidationManager.addPropertyError("otherProperty", null, "value1", "value error", null);

            //assert

            // both will be called each time addPropertyError is called
            expect(callbackA.length).toEqual(8);
            expect(callbackA[callbackA.length - 1].length).toEqual(4); // 4 errors for A

            // clear the data and notify
            callbackA = [];

            serverValidationManager.notify();
            $timeout.flush();

            expect(callbackA.length).toEqual(1);
            expect(callbackA[0].length).toEqual(4); // 4 errors for A

        });

        // TODO: Finish testing the rest!

    });

});
