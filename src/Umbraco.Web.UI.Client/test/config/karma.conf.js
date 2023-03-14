const jsdom = require("jsdom");
module.exports = function (config) {

  config.set({

    // base path, that will be used to resolve files and exclude
    basePath: '../..',

    frameworks: ['jasmine'],

    // list of files / patterns to load in the browser
    files: [

      // Jasmine plugins

      //libraries
      'node_modules/jquery/dist/jquery.min.js',
      'node_modules/angular/angular.min.js',
      'node_modules/angular-animate/angular-animate.min.js',
      'node_modules/angular-cookies/angular-cookies.min.js',
      'node_modules/angular-aria/angular-aria.min.js',
      'node_modules/angular-local-storage/dist/angular-local-storage.min.js',
      'node_modules/angular-route/angular-route.min.js',
      'node_modules/angular-sanitize/angular-sanitize.min.js',
      'node_modules/angular-mocks/angular-mocks.js',
      'node_modules/angular-ui-sortable/dist/sortable.min.js',
      'node_modules/underscore/underscore-min.js',
      'node_modules/moment/min/moment-with-locales.js',
      'lib/umbraco/Extensions.js',
      'node_modules/lazyload-js/LazyLoad.min.js',
      'node_modules/angular-dynamic-locale/dist/tmhDynamicLocale.min.js',

      //app bootstrap and loader
      'test/config/app.unit.js',

      //application files
      '../Umbraco.Cms.StaticAssets/wwwroot/umbraco/js/*.controllers.min.js',
      '../Umbraco.Cms.StaticAssets/wwwroot/umbraco/js/*.directives.min.js',
      '../Umbraco.Cms.StaticAssets/wwwroot/umbraco/js/*.filters.min.js',
      '../Umbraco.Cms.StaticAssets/wwwroot/umbraco/js/*.services.min.js',
      '../Umbraco.Cms.StaticAssets/wwwroot/umbraco/js/*.interceptors.min.js',
      '../Umbraco.Cms.StaticAssets/wwwroot/umbraco/js/*.resources.min.js',

      //mocked data and routing
      'src/common/mocks/umbraco.servervariables.js',
      'src/common/mocks/**/*.js',

      //tests
      'test/unit/**/*.spec.js'
    ],

    // list of files to exclude
    exclude: [],

    // use dolts reporter, as travis terminal does not support escaping sequences
    // possible values: 'dots', 'progress', 'junit', 'spec'
    // ***
    // progress: Outputs a simple list like: "Executed 128 of 144 SUCCESS (0 secs / 0.814 secs)"
    // spec: Outputs a more verbose report which is more useful for debugging if one of the tests fails.
    // ***
    // CLI --reporters progress

    reporters: ['spec', 'junit'],
    specReporter: {
      maxLogLines: 5,         // limit number of lines logged per test
      suppressErrorSummary: true,  // do not print error summary
      suppressFailed: false,  // do not print information about failed tests
      suppressPassed: false,  // do not print information about passed tests
      suppressSkipped: true,  // do not print information about skipped tests
      showSpecTiming: false // print the time elapsed for each spec
    },


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
    browsers: ['jsdom'],

    // Configure a user agent so the log file gets generated properly
    jsdomLauncher: {
      jsdom: {
        resources: new jsdom.ResourceLoader({
          userAgent: "umbraco-test-suite"
        })
      }
    },

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
      require('karma-jsdom-launcher'),
      require('karma-junit-reporter'),
      require('karma-spec-reporter')
    ],

    // the default configuration
    junitReporter: {
      outputDir: '', // results will be saved as $outputDir/$browserName.xml
      outputFile: undefined, // if included, results will be saved as $outputDir/$browserName/$outputFile
      suite: '', // suite will become the package name attribute in xml testsuite element
      useBrowserName: true, // add browser name to report and classes names
      nameFormatter: undefined, // function (browser, result) to customize the name attribute in xml testcase element
      classNameFormatter: undefined, // function (browser, result) to customize the classname attribute in xml testcase element
      properties: {} // key value pair of properties to add to the <properties> section of the report
    },

    client: {
      jasmine: {
        random: false
      }
    }
  });
};
