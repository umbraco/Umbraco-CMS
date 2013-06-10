angular.module('umbraco.mocks.resources')
.factory('localizationResource', function () {
  var localizationArray = [];
  var labels = {};

  var factory = {
    _cachedItems: localizationArray,
    getLabels: function (language) {
      /* 
        Fetch from JSON object according to users language settings
        $http.get('model.:language.json') ish solution
       */
      labels = {
        language: 'en-UK',
        app: {
          search: {
            typeToSearch: "Type to search",
            searchResult: "Search result"
          },
          help: "Help" 
        },
        content: {
          modelName: "Content",
          contextMenu: {
            createPageLabel: "Create a page under %name"
          }
        }
      };



      return labels;
    },
    getLanguage: function() {
      return labels.language;
    }
  };
  return factory;
}); 