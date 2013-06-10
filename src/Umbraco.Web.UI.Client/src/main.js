require.config({
  waitSeconds: 120,
  paths: {
    jquery: '../lib/jquery/jquery-1.8.2.min',
    jqueryCookie: '../lib/jquery/jquery.cookie',
    umbracoExtensions: "../lib/umbraco/extensions",
    bootstrap: '../lib/bootstrap/js/bootstrap',
    underscore: '../lib/underscore/underscore',
    angular: '../lib/angular/angular.min',
    angularResource: '../lib/angular/angular-resource',
    
    codemirror: '../lib/codemirror/js/lib/codemirror',
    codemirrorJs: '../lib/codemirror/js/mode/javascript/javascript',
    codemirrorCss: '../lib/codemirror/js/mode/css/css',
    codemirrorXml: '../lib/codemirror/js/mode/xml/xml',
    codemirrorHtml: '../lib/codemirror/js/mode/htmlmixed/htmlmixed',

    tinymce: '../lib/tinymce/tinymce.min',
    text: '../lib/require/text',
    async: '../lib/require/async',
    css: '../lib/require/css'
  },
  shim: {
    'angular' : {'exports' : 'angular'},
    'angular-resource': { deps: ['angular'] },
    'bootstrap': { deps: ['jquery'] },
    'jqueryCookie': { deps: ['jquery'] },
    'underscore': {exports: '_'},
    'codemirror': {exports: 'CodeMirror'},  
    'codemirrorJs':{deps:['codemirror']},
    'codemirrorCss':{deps:['codemirror']},
    'codemirrorXml':{deps:['codemirror']},
    'codemirrorHtml':{deps:['codemirrorXml','codemirrorCss','codemirrorJs'], exports: 'mixedMode'},
    'tinymce': {
      exports: 'tinyMCE',
      init: function () {
        this.tinymce.DOM.events.domLoaded = true;
        return this.tinymce;
      }
    }
  },
  priority: [
  "angular"
  ],
  urlArgs: 'v=1.1'
});

require( [
  'angular',
  'app_dev',
  'jquery',
  'jqueryCookie',
  'bootstrap',
  'umbracoExtensions',
  'umbraco.mocks',
  'umbraco.directives',
  'umbraco.filters',
  'umbraco.services',
  'umbraco.controllers',
  'routes'
  ], function(angular, app, jQuery) {
  //This function will be called when all the dependencies
  //listed above are loaded. Note that this function could
  //be called before the page is loaded.
  //This callback is optional.

  jQuery(document).ready(function () {
    angular.bootstrap(document, ['umbraco']);
  });
});
