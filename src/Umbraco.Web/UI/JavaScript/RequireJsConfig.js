{   
    baseUrl: "js",
    waitSeconds: 120,
    paths: {
        jquery: '../lib/jquery/jquery-1.8.2.min',
        jqueryCookie: '../lib/jquery/jquery.cookie',
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
        /* THIS IS SPECIAL SYNTAX BECAUSE JS functions ARE NOT STANDARD JSON SO THEY CANNOT BE SERIALIZED */
        /* SO BEFORE WE RENDER WE'LL ENSURE THAT IT'S FORM */
        'tinymce': "@@@@{exports: 'tinyMCE', init: function () { this.tinymce.DOM.events.domLoaded = true; return this.tinymce; } }"
    },
    priority: [
        "angular"
    ],
    urlArgs: 'v=1.1'
}