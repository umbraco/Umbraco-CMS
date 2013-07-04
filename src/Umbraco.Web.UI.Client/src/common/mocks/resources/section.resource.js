/**
    * @ngdoc factory 
    * @name umbraco.mocks.sectionMocks     
    * @description Mocks data retreival for the sections
    **/
function sectionMocks($httpBackend, mocksUtills) {

    /** internal method to mock the sections to be returned */
    function getSections() {
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
              .whenGET(mocksUtills.urlRegex('/umbraco/UmbracoApi/Section/GetSections'))
              .respond(getSections);
        }
    };
}

angular.module('umbraco.mocks').factory('sectionMocks', ['$httpBackend', 'mocksUtills', sectionMocks]);