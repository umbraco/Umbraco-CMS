module.exports = function (config) {

    config.set({
                
        // base path, that will be used to resolve files and exclude
        basePath: '../..',

        frameworks: ['jasmine'],

        // list of files / patterns to load in the browser
        files: [

            //libraries
            'node_modules/jquery/dist/jquery.min.js',
            'node_modules/angular/angular.js',
            'node_modules/angular-animate/angular-animate.js',
            'node_modules/angular-cookies/angular-cookies.js',
            'node_modules/angular-local-storage/dist/angular-local-storage.min.js',
            'node_modules/angular-route/angular-route.js',
            'node_modules/angular-sanitize/angular-sanitize.js',
            'node_modules/angular-mocks/angular-mocks.js',
            'node_modules/angular-ui-sortable/dist/sortable.js',
            'node_modules/underscore/underscore-min.js',
            'node_modules/moment/min/moment-with-locales.js',
            'lib/umbraco/Extensions.js',
            'node_modules/lazyload-js/lazyload.min.js',

            //app bootstrap and loader
            'test/config/app.unit.js',

            //application files
            'src/common/directives/*.js',
            'src/common/filters/*.js',
            'src/common/services/*.js',
            'src/common/interceptors/*.js',
            'src/common/resources/*.js',
            'src/views/**/*.controller.js',

            //mocked data and routing
            'src/common/mocks/umbraco.servervariables.js',
            'src/common/mocks/**/*.js',

            //tests
            'test/unit/**/*.spec.js'
        ],

        // list of files to exclude
        exclude: [],

        // use dolts reporter, as travis terminal does not support escaping sequences
        // possible values: 'dots', 'progress', 'junit', 'teamcity'
        // CLI --reporters progress
        reporters: ['progress'],

        // web server port
        // CLI --port 9876
        port: 9876,

        // cli runner port
        // CLI --runner-port 9100
    runnerPort: 9100,

        // enable / disable colors in the output (reporters and logs)
        // CLI --colors --no-colors
        colors: true,

        // level of logging
        // possible values: karma.LOG_DISABLE || karma.LOG_ERROR || karma.LOG_WARN || karma.LOG_INFO || karma.LOG_DEBUG
        // CLI --log-level debug
        logLevel: config.LOG_INFO,

        // enable / disable watching file and executing tests whenever any file changes
        // CLI --auto-watch --no-auto-watch
        autoWatch: false,

        // Start these browsers, currently available:
        // - Chrome
        // - ChromeCanary
        // - Firefox
        // - Opera
        // - Safari (only Mac)
        // - PhantomJS
        // - IE (only Windows)
        // CLI --browsers Chrome,Firefox,Safari
        browsers: ['PhantomJS'],

        // allow waiting a bit longer, some machines require this

        browserNoActivityTimeout: 100000,     // default 10,000ms
        
        // Auto run tests on start (when browsers are captured) and exit
        // CLI --single-run --no-single-run
        singleRun: true,

        // report which specs are slower than 500ms
        // CLI --report-slower-than 500
        reportSlowerThan: 500,

        plugins: [
            require('karma-jasmine'),
            require('karma-phantomjs-launcher')
        ]
    });
};
