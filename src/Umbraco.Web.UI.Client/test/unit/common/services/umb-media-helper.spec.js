describe('media helper tests', function () {
    var umbMediaHelper;

    beforeEach(module('umbraco.services'));  

    beforeEach(inject(function ($injector) {
        umbMediaHelper = $injector.get('mediaHelper');
    }));

    describe('mediaHelper service tests', function () {

        it('returns file path associated with media property', function () {

            // dummy imageModel
            var imageModel =
            {
                "isChildOfListView": false,
                "treeNodeUrl": "/umbraco/backoffice/UmbracoTrees/MediaTree/GetTreeNode/2061",
                "contentTypeName": "Image",
                "isContainer": false,
                "notifications": [],
                "ModelState": {},
                "tabs": [
                  {
                      "id": 3,
                      "active": true,
                      "label": "Image",
                      "alias": "Image",
                      "properties": [
                        {
                            "label": "Upload image",
                            "description": null,
                            "view": "imagecropper",
                            "config": {
                                "focalPoint": {
                                    "left": 0.5,
                                    "top": 0.5
                                },
                                "src": ""
                            },
                            "hideLabel": false,
                            "validation": {
                                "mandatory": false,
                                "pattern": null
                            },
                            "id": 1097,
                            "value": {
                                "src": "/media/1001/TestImage123.jpg",
                                "crops": []
                            },
                            "alias": "umbracoFile",
                            "editor": "Umbraco.ImageCropper"
                        }
                      ]
                  }
                ]
            };
            
            // test returns file path associated with media property
            expect(umbMediaHelper.getMediaPropertyValue({ mediaModel: imageModel, imageOnly: true })).toBe("/media/1001/TestImage123.jpg");
        });

    });
});