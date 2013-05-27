{   
    waitSeconds: 120,
    paths: {
        jquery: '../lib/jquery/jquery-1.8.2.min',
        underscore: '../lib/underscore/underscore',
        angular: '../lib/angular/angular',
        angularResource: '../lib/angular/angular-resource',
        statemanager: '../lib/angular/statemanager',
        text: '../lib/require/text',
        async: '../lib/require/async',
        namespaceMgr: '../lib/Umbraco/NamespaceManager',
        myApp: '../js/myApp'
    },
    shim: {
        'angular' : {'exports' : 'angular'},
        'angular-resource': { deps: ['angular'] },
        'statemanager': { deps: ['angular'] },
        'bootstrap': { deps: ['jquery'] },
        'jqueryCookie': { deps: ['jquery'] },
        'angular-statemanager' : {deps:['angular']},
        'underscore': {exports: '_'}
    },
    priority: [
        "angular"
    ],
    urlArgs: 'v=1.1'
}