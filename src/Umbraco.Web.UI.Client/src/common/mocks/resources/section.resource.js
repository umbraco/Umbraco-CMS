/**
* @ngdoc service
* @name umbraco.mocks.sectionMocks     
* @description 
* Mocks data retrival for the sections
**/
function sectionMocks($httpBackend, mocksUtils) {

    /** internal method to mock the sections to be returned */
    function getSections() {
        
        if (!mocksUtils.checkAuth()) {
            return [401, null, null];
        }

        var sections = [
            { name: "Content", cssclass: "traycontent", alias: "content" },
            { name: "Media", cssclass: "traymedia", alias: "media" },
            { name: "Settings", cssclass: "traysettings", alias: "settings" },
            { name: "Developer", cssclass: "traydeveloper", alias: "developer" },
            { name: "Users", cssclass: "trayuser", alias: "users" }
        ];
        
        return [200, sections, null];
    }
    
    return {
        register: function () {
            $httpBackend
              .whenGET(mocksUtils.urlRegex('/umbraco/UmbracoApi/Section/GetSections'))
              .respond(getSections);
        }
    };
}

angular.module('umbraco.mocks').factory('sectionMocks', ['$httpBackend', 'mocksUtils', sectionMocks]);
