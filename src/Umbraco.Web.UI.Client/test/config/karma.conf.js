module.exports = function(karma) {
  karma.configure({
    // base path, that will be used to resolve files and exclude
    basePath: '../..',

    frameworks: ['jasmine'],

    // list of files / patterns to load in the browser
    files: [
        'lib/../build/belle/lib/jquery/jquery.min.js',
        'lib/angular/1.1.5/angular.js',
        'lib/angular/1.1.5/angular-cookies.min.js',
        'lib/angular/1.1.5/angular-mocks.js',
        'lib/angular/angular-ui-sortable.js',
        
        /*
        For angular 1.2:
         'lib/angular/1.2/angular.js',
         'lib/angular/1.2/angular-route.min.js',
         'lib/angular/1.2/angular-touch.min.js',
         'lib/angular/1.2/angular-cookies.min.js',
         'lib/angular/1.2/angular-animate.min.js',
         'lib/angular/1.2/angular-mocks.js',*/
         

         'lib/../build/belle/lib/underscore/underscore-min.js',
         'lib/umbraco/Extensions.js', 
         'lib/../build/belle/lib/rgrove-lazyload/lazyload.js',


         'test/config/app.unit.js',
         'src/common/mocks/umbraco.servervariables.js',

         'src/common/directives/*.js',
         'src/common/filters/*.js',
         'src/common/services/*.js',
         'src/common/security/*.js',
         'src/common/resources/*.js',
         'src/common/mocks/**/*.js',
         'src/views/**/*.controller.js',
         'test/unit/**/*.spec.js',
        {pattern: 'lib/**/*.js', watched: true, served: true, included: false}       
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
    logLevel: karma.LOG_INFO,

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

    // If browser does not capture in given timeout [ms], kill it
    // CLI --capture-timeout 5000
    captureTimeout: 5000,

    // Auto run tests on start (when browsers are captured) and exit
    // CLI --single-run --no-single-run
    singleRun: true,

    // report which specs are slower than 500ms
    // CLI --report-slower-than 500
    reportSlowerThan: 500,

    plugins: [
      'karma-jasmine',
      'karma-phantomjs-launcher'
    ]
  });
};