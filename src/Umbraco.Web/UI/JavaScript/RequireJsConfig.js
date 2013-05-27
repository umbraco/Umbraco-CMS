{
    
    /*NOTE: This is actually /Belle/js because we are loading in requireJs from /Belle already*/
    baseUrl: 'js',
    
    waitSeconds: 120,
    paths: {
        jquery: '../lib/jquery/jquery-1.8.2.min',
        jqueryCookie: '../lib/jquery/jquery.cookie',
        bootstrap: '../lib/bootstrap/js/bootstrap',
        underscore: '../lib/underscore/underscore',
        angular: '../lib/angular/angular',
        angularResource: '../lib/angular/angular-resource',
        statemanager: '../lib/angular/statemanager',
        text: '../lib/require/text',
        async: '../lib/require/async',
        namespaceMgr: '../lib/Umbraco/NamespaceManager',
        myApp: '../../../Content/JavaScript/myApp'
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