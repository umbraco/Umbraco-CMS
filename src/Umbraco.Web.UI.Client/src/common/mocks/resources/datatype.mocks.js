angular.module('umbraco.mocks').
    factory('dataTypeMocks', ['$httpBackend', 'mocksUtils', function ($httpBackend, mocksUtils) {
        'use strict';

        function returnById(status, data, headers) {

            if (!mocksUtils.checkAuth()) {
                return [401, null, null];
            }

            var id = mocksUtils.getParameterByName(data, "id") || 1234;

            var selectedId = String.CreateGuid();

            var dataType = mocksUtils.getMockDataType(id, selectedId);

            return [200, dataType, null];
        }

        function returnEmpty(status, data, headers) {

            if (!mocksUtils.checkAuth()) {
                return [401, null, null];
            }

            var response = returnById(200, "", null);
            var node = response[1];

            node.name = "";
            node.selectedEditor = "";
            node.id = 0;
            node.preValues = [];

            return response;
        }

        function returnPreValues(status, data, headers) {

            if (!mocksUtils.checkAuth()) {
                return [401, null, null];
            }

            var editorId = mocksUtils.getParameterByName(data, "editorId") || "83E9AD36-51A7-4440-8C07-8A5623AC6979";

            var preValues = [
                {
                    label: "Custom pre value 1 for editor " + editorId,
                    description: "Enter a value for this pre-value",
                    key: "myPreVal",
                    view: "requiredfield",
                    validation: [
                        {
                            type: "Required"
                        }
                    ]
                },
                {
                    label: "Custom pre value 2 for editor " + editorId,
                    description: "Enter a value for this pre-value",
                    key: "myPreVal",
                    view: "requiredfield",
                    validation: [
                        {
                            type: "Required"
                        }
                    ]
                }
            ];
            return [200, preValues, null];
        }

        function returnSave(status, data, headers) {
            if (!mocksUtils.checkAuth()) {
                return [401, null, null];
            }

            var postedData = JSON.parse(headers);

            var dataType = mocksUtils.getMockDataType(postedData.id, postedData.selectedEditor);
            dataType.notifications = [{
                header: "Saved",
                message: "Data type saved",
                type: 0
            }];

            return [200, dataType, null];
        }

        return {
            register: function () {

                $httpBackend
                    .whenPOST(mocksUtils.urlRegex('/umbraco/UmbracoApi/DataType/PostSave'))
                    .respond(returnSave);

                $httpBackend
                    .whenGET(mocksUtils.urlRegex('/umbraco/UmbracoApi/DataType/GetById'))
                    .respond(returnById);

                $httpBackend
                    .whenGET(mocksUtils.urlRegex('/umbraco/UmbracoApi/DataType/GetEmpty'))
                    .respond(returnEmpty);

                $httpBackend
                    .whenGET(mocksUtils.urlRegex('/umbraco/UmbracoApi/DataType/GetPreValues'))
                    .respond(returnPreValues);
            },
            expectGetById: function () {
                $httpBackend
                    .expectGET(mocksUtils.urlRegex('/umbraco/UmbracoApi/DataType/GetById'));
            }
        };
    }]);
