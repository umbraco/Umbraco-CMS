angular.module('umbraco.mocks').
    factory('variantContentMocks', ['$httpBackend', 'mocksUtils', function ($httpBackend, mocksUtils) {
    'use strict';

    function returnEmptyVariantNode(status, data, headers) {

        if (!mocksUtils.checkAuth()) {
            return [401, null, null];
        }

        var response = returnVariantNodebyId(200, "", null);
        var node = response[1];
        var parentId = mocksUtils.getParameterByName(data, "parentId") || 1234;

        node.name = "";
        node.id = 0;
        node.parentId = parentId;

        node.tabs.forEach(function(tab) {
            tab.properties.forEach(function(property) {
                property.value = "";
            });
        });

        return response;
    }

    function returnVariantNodebyId(status, data, headers) {

        if (!mocksUtils.checkAuth()) {
            return [401, null, null];
        }

        var id = mocksUtils.getParameterByName(data, "id") || "1234";
        id = parseInt(id, 10);

        var node = mocksUtils.getMockVariantContent(id);

        return [200, node, null];
    }


    return {
        register: function () {

            $httpBackend
                .whenGET(mocksUtils.urlRegex('/umbraco/UmbracoApi/Content/GetById?'))
                .respond(returnVariantNodebyId);

            $httpBackend
                .whenGET(mocksUtils.urlRegex('/umbraco/UmbracoApi/Content/GetEmpty'))
                .respond(returnEmptyVariantNode);

        }
    };
}]);
