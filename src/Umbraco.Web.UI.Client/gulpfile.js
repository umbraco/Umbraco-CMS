var gulp = require('gulp');
var watch = require('gulp-watch');
var concat = require('gulp-concat');
var rename = require('gulp-rename');
var wrap = require("gulp-wrap-js");
var sort = require('gulp-sort');
var connect = require('gulp-connect');
var open = require('gulp-open');
var babel = require("gulp-babel");
var runSequence = require('run-sequence');
var imagemin = require('gulp-imagemin');

var _ = require('lodash');
var MergeStream = require('merge-stream');

// js
var eslint = require('gulp-eslint');

//Less + css
var postcss = require('gulp-postcss');
var less = require('gulp-less');
var autoprefixer = require('autoprefixer');
var cssnano = require('cssnano');
var cleanCss = require("gulp-clean-css");

// Documentation
var gulpDocs = require('gulp-ngdocs');

// Testing
var karmaServer = require('karma').Server;

/***************************************************************
Helper functions
***************************************************************/
function processJs(files, out) {

    return gulp.src(files)
        // check for js errors
        .pipe(eslint())
        // outputs the lint results to the console
        .pipe(eslint.format())
        // sort files in stream by path or any custom sort comparator
        .pipe(babel())
        .pipe(sort())
        .pipe(concat(out))
        .pipe(wrap('(function(){\n%= body %\n})();'))
        .pipe(gulp.dest(root + targets.js));

        console.log(out + " compiled");
}

function processLess(files, out) {
    var processors = [
         autoprefixer,
         cssnano({zindex: false})
    ];

    return gulp.src(files)
        .pipe(less())
        .pipe(cleanCss())
        .pipe(postcss(processors))
        .pipe(rename(out))
        .pipe(gulp.dest(root + targets.css));

    console.log(out + " compiled");
}

/***************************************************************
Paths and destinations
Each group is iterated automatically in the setup tasks below
***************************************************************/
var sources = {

    //less files used by backoffice and preview
    //processed in the less task
    less: {
        installer: { files: ["src/less/installer.less"], out: "installer.css" },
        nonodes: { files: ["src/less/pages/nonodes.less"], out: "nonodes.style.min.css"},
        preview: { files: ["src/less/canvas-designer.less"], out: "canvasdesigner.css" },
        umbraco: { files: ["src/less/belle.less"], out: "umbraco.css" }
    },

    //js files for backoffie
    //processed in the js task
    js: {
        preview: { files: ["src/preview/**/*.js"], out: "umbraco.preview.js" },
        installer: { files: ["src/installer/**/*.js"], out: "umbraco.installer.js" },
        controllers: { files: ["src/{views,controllers}/**/*.controller.js"], out: "umbraco.controllers.js" },
        directives: { files: ["src/common/directives/**/*.js"], out: "umbraco.directives.js" },
        filters: { files: ["src/common/filters/**/*.js"], out: "umbraco.filters.js" },
        resources: { files: ["src/common/resources/**/*.js"], out: "umbraco.resources.js" },
        services: { files: ["src/common/services/**/*.js"], out: "umbraco.services.js" },
        security: { files: ["src/common/interceptors/**/*.js"], out: "umbraco.interceptors.js" }
    },

    //selectors for copying all views into the build
    //processed in the views task
    views:{
        umbraco: {files: ["src/views/**/*.html"], folder: ""},
        installer: {files: ["src/installer/steps/*.html"], folder: "install"}
    },

    //globs for file-watching
    globs:{
        views: "./src/views/**/*.html",
        less: "./src/less/**/*.less",
        js: "./src/*.js",
        lib: "./lib/**/*",
        assets: "./src/assets/**"
    }
};

var root = "../Umbraco.Web.UI/Umbraco/";
var targets = {
    js: "js/",
    lib: "lib/",
    views: "views/",
    css: "assets/css/",
    assets: "assets/"
};


/**************************
 * Main tasks for the project to prepare backoffice files
 **************************/

 // Build - build the files ready for production
gulp.task('build', function(cb) {
    runSequence(["dependencies", "js", "less", "views"], cb);
});

// Dev - build the files ready for development and start watchers
gulp.task('dev', function(cb) {
    runSequence(["dependencies", "js", "less", "views"], "watch", cb);
});

// Docserve - build and open the back office documentation
gulp.task('docserve', function(cb) {
    runSequence('docs', 'connect:docs', 'open:docs', cb);
});

/**************************
 * Task processes and copies all dependencies, either installed by npm or stored locally in the project
 **************************/
gulp.task('dependencies', function () {

    //as we do multiple things in this task, we merge the multiple streams
    var stream = new MergeStream();

    // Pick the dependencies we need from each package
    // so we don't just ship with a lot of files that aren't needed
    const nodeModules = [
        {
            "name": "ace-builds",
            "src":  [
                "./node_modules/ace-builds/src-min-noconflict/ace.js",
                "./node_modules/ace-builds/src-min-noconflict/ext-language_tools.js",
                "./node_modules/ace-builds/src-min-noconflict/ext-searchbox.js",
                "./node_modules/ace-builds/src-min-noconflict/ext-settings_menu.js",
                "./node_modules/ace-builds/src-min-noconflict/snippets/text.js",
                "./node_modules/ace-builds/src-min-noconflict/snippets/javascript.js",
                "./node_modules/ace-builds/src-min-noconflict/snippets/css.js",
                "./node_modules/ace-builds/src-min-noconflict/theme-chrome.js",
                "./node_modules/ace-builds/src-min-noconflict/mode-razor.js",
                "./node_modules/ace-builds/src-min-noconflict/mode-javascript.js",
                "./node_modules/ace-builds/src-min-noconflict/mode-css.js",
                "./node_modules/ace-builds/src-min-noconflict/worker-javascript.js",
                "./node_modules/ace-builds/src-min-noconflict/worker-css.js"
            ],
            "base": "./node_modules/ace-builds"
        },
        {
            "name": "angular",
            "src":  ["./node_modules/angular/angular.js"],
            "base": "./node_modules/angular"
        },
        {
            "name": "angular-cookies",
            "src":  ["./node_modules/angular-cookies/angular-cookies.js"],
            "base": "./node_modules/angular-cookies"
        },
        {
            "name": "angular-dynamic-locale",
            "src":  [
                "./node_modules/angular-dynamic-locale/dist/tmhDynamicLocale.min.js",
                "./node_modules/angular-dynamic-locale/dist/tmhDynamicLocale.min.js.map"
            ],
            "base": "./node_modules/angular-dynamic-locale/dist"
        },
        {
            "name": "angular-sanitize",
            "src":  ["./node_modules/angular-sanitize/angular-sanitize.js"],
            "base": "./node_modules/angular-sanitize"
        },
        {
            "name": "angular-touch",
            "src":  ["./node_modules/angular-touch/angular-touch.js"],
            "base": "./node_modules/angular-touch"
        },
        {
            "name": "angular-ui-sortable",
            "src":  ["./node_modules/angular-ui-sortable/dist/sortable.js"],
            "base": "./node_modules/angular-ui-sortable/dist"
        },
        {
            "name": "angular-route",
            "src":  ["./node_modules/angular-route/angular-route.js"],
            "base": "./node_modules/angular-route"
        },
        {
            "name": "angular-animate",
            "src":  ["./node_modules/angular-animate/angular-animate.js"],
            "base": "./node_modules/angular-animate"
        },
        {
            "name": "angular-chart.js",
            "src": ["./node_modules/angular-chart.js/dist/angular-chart.min.js"],
            "base": "./node_modules/angular-chart.js/dist"
        },
        {
            "name": "angular-i18n",
            "src":  [
                "./node_modules/angular-i18n/angular-i18n.js",
                "./node_modules/angular-i18n/angular-locale_*.js"
            ],
            "base": "./node_modules/angular-i18n"
        },
        {
            "name": "angular-local-storage",
            "src":  [
                "./node_modules/angular-local-storage/dist/angular-local-storage.min.js",
                "./node_modules/angular-local-storage/dist/angular-local-storage.min.js.map"
            ],
            "base": "./node_modules/angular-local-storage/dist"
        },
        {
            "name": "angular-messages",
            "src":  ["./node_modules/angular-messages/angular-messages.js"],
            "base": "./node_modules/angular-messages"
        },
        {
            "name": "angular-mocks",
            "src":  ["./node_modules/angular-mocks/angular-mocks.js"],
            "base": "./node_modules/angular-mocks"
        },
        {
            "name": "animejs",
            "src":  ["./node_modules/animejs/anime.min.js"],
            "base": "./node_modules/animejs"
        },
        {
            "name": "bootstrap-social",
            "src":  ["./node_modules/bootstrap-social/bootstrap-social.css"],
            "base": "./node_modules/bootstrap-social"
        },
        {
            "name": "chart.js",
            "src": ["./node_modules/chart.js/dist/Chart.min.js"],
            "base": "./node_modules/chart.js/dist"
        },
        {
            "name": "clipboard",
            "src":  ["./node_modules/clipboard/dist/clipboard.min.js"],
            "base": "./node_modules/clipboard/dist"
        },
        {
            "name": "jsdiff",
            "src":  ["./node_modules/diff/dist/diff.min.js"],
            "base": "./node_modules/diff/dist"
        },
        {
            "name": "flatpickr",
            "src":  [
                "./node_modules/flatpickr/dist/flatpickr.js",
                "./node_modules/flatpickr/dist/flatpickr.css"
            ],
            "base": "./node_modules/flatpickr/dist"
        },
        {
            "name": "font-awesome",
            "src":  [
                "./node_modules/font-awesome/fonts/*",
                "./node_modules/font-awesome/css/font-awesome.min.css"
            ],
            "base": "./node_modules/font-awesome"
        },
        {
            "name": "jquery",
            "src":  [
                "./node_modules/jquery/dist/jquery.min.js",
                "./node_modules/jquery/dist/jquery.min.map"
            ],
            "base": "./node_modules/jquery/dist"
        },
        {
            "name": "jquery-ui",
            "src":  ["./node_modules/jquery-ui-dist/jquery-ui.min.js"],
            "base": "./node_modules/jquery-ui-dist"
        },
        {
            "name": "jquery-ui-touch-punch",
            "src":  ["./node_modules/jquery-ui-touch-punch/jquery.ui.touch-punch.min.js"],
            "base": "./node_modules/jquery-ui-touch-punch"
        },
        {
            "name": "lazyload-js",
            "src":  ["./node_modules/lazyload-js/lazyload.min.js"],
            "base": "./node_modules/lazyload-js"
        },
        {
            "name": "moment",
            "src":  ["./node_modules/moment/min/moment.min.js"],
            "base": "./node_modules/moment/min"
        },
        {
            "name": "moment",
            "src":  ["./node_modules/moment/locale/*.js"],
            "base": "./node_modules/moment/locale"
        },
        {
            "name": "ng-file-upload",
            "src":  ["./node_modules/ng-file-upload/dist/ng-file-upload.min.js"],
            "base": "./node_modules/ng-file-upload/dist"
        },
        {
            "name": "nouislider",
            "src":  [
                "./node_modules/nouislider/distribute/nouislider.min.js",
                "./node_modules/nouislider/distribute/nouislider.min.css"
            ],
            "base": "./node_modules/nouislider/distribute"
        },
        {
            "name": "signalr",
            "src":  ["./node_modules/signalr/jquery.signalR.js"],
            "base": "./node_modules/signalr"
        },
        {
            "name": "spectrum",
            "src":  [
                "./node_modules/spectrum-colorpicker/spectrum.js",
                "./node_modules/spectrum-colorpicker/spectrum.css"
            ],
            "base": "./node_modules/spectrum-colorpicker"
        },
        {
            "name": "tinymce",
            "src":  [
                "./node_modules/tinymce/tinymce.min.js",
                "./node_modules/tinymce/plugins/**",
                "./node_modules/tinymce/skins/**",
                "./node_modules/tinymce/themes/**"
            ],
            "base": "./node_modules/tinymce"
        },
        {
            "name": "typeahead.js",
            "src":  ["./node_modules/typeahead.js/dist/typeahead.bundle.min.js"],
            "base": "./node_modules/typeahead.js/dist"
        },
        {
            "name": "underscore",
            "src":  ["node_modules/underscore/underscore-min.js"],
            "base": "./node_modules/underscore"
        }
    ];

    // add streams for node modules
    nodeModules.forEach(module => {
        stream.add(
            gulp.src(module.src,
                { base: module.base })
                .pipe(gulp.dest(root + targets.lib + "/" + module.name))
        );
    });

    //copy over libs which are not on npm (/lib)
    stream.add(
         gulp.src(sources.globs.lib)
            .pipe(gulp.dest(root + targets.lib))
        );

    //Copies all static assets into /root / assets folder
    //css, fonts and image files
    stream.add(
            gulp.src(sources.globs.assets)
				.pipe(imagemin([
                    imagemin.gifsicle({interlaced: true}),
                    imagemin.jpegtran({progressive: true}),
                    imagemin.optipng({optimizationLevel: 5}),
                    imagemin.svgo({
                        plugins: [
                            {removeViewBox: true},
                            {cleanupIDs: false}
                        ]
                    })
                ]))
                .pipe(gulp.dest(root + targets.assets))
        );

    // Copies all the less files related to the preview into their folder
    //these are not pre-processed as preview has its own less combiler client side
    stream.add(
            gulp.src("src/canvasdesigner/editors/*.less")
                .pipe(gulp.dest(root + targets.assets + "/less"))
        );

	// Todo: check if we need these fileSize
    stream.add(
            gulp.src("src/views/propertyeditors/grid/config/*.*")
                .pipe(gulp.dest(root + targets.views + "/propertyeditors/grid/config"))
        );
    stream.add(
            gulp.src("src/views/dashboard/default/*.jpg")
                .pipe(gulp.dest(root + targets.views + "/dashboard/default"))
        );

    return stream;
});


/**************************
 * Copies all angular JS files into their seperate umbraco.*.js file
 **************************/
gulp.task('js', function () {

    //we run multiple streams, so merge them all together
    var stream = new MergeStream();

    stream.add(
        gulp.src(sources.globs.js)
            .pipe(gulp.dest(root + targets.js))
        );

     _.forEach(sources.js, function (group) {
        stream.add (processJs(group.files, group.out) );
     });

     return stream;
});

gulp.task('less', function () {

    var stream = new MergeStream();

    _.forEach(sources.less, function (group) {
        stream.add( processLess(group.files, group.out) );
    });

    return stream;
});


gulp.task('views', function () {

    var stream = new MergeStream();

    _.forEach(sources.views, function (group) {

        console.log("copying " + group.files + " to " + root + targets.views + group.folder)

        stream.add (
            gulp.src(group.files)
                .pipe( gulp.dest(root + targets.views + group.folder) )
        );

    });

    return stream;
});


gulp.task('watch', function () {

    var stream = new MergeStream();
    var watchInterval = 500;

    //Setup a watcher for all groups of javascript files
    _.forEach(sources.js, function (group) {

        if(group.watch !== false){

            stream.add(

                watch(group.files, { ignoreInitial: true, interval: watchInterval }, function (file) {

                    console.info(file.path + " has changed, added to:  " + group.out);
                    processJs(group.files, group.out);

                })

            );

        }

    });

    stream.add(
        //watch all less files and trigger the less task
        watch(sources.globs.less, { ignoreInitial: true, interval: watchInterval }, function () {
            gulp.run(['less']);
        })
    );

    //watch all views - copy single file changes
    stream.add(
        watch(sources.globs.views, { interval: watchInterval })
        .pipe(gulp.dest(root + targets.views))
    );

    //watch all app js files that will not be merged - copy single file changes
    stream.add(
        watch(sources.globs.js, { interval: watchInterval })
        .pipe(gulp.dest(root + targets.js))
    );

    return stream;
});

/**************************
 * Build Backoffice UI API documentation
 **************************/
gulp.task('docs', [], function (cb) {

    var options = {
        html5Mode: false,
        startPage: '/api',
        title: "Umbraco Backoffice UI API Documentation",
        dest: 'docs/api',
        styles: ['docs/umb-docs.css'],
        image: "https://our.umbraco.com/assets/images/logo.svg"
    }

    return gulpDocs.sections({
        api: {
            glob: ['src/common/**/*.js', 'docs/src/api/**/*.ngdoc'],
            api: true,
            title: 'API Documentation'
        }
    })
    .pipe(gulpDocs.process(options))
    .pipe(gulp.dest('docs/api'));
    cb();
});

gulp.task('connect:docs', function (cb) {
    connect.server({
        root: 'docs/api',
        livereload: true,
        fallback: 'docs/api/index.html',
        port: 8880
    });
    cb();
});

gulp.task('open:docs', function (cb) {

    var options = {
        uri: 'http://localhost:8880/index.html'
    };

    gulp.src(__filename)
    .pipe(open(options));
    cb();
});

/**************************
 * Build tests
 **************************/

 // Karma test
gulp.task('test:unit', function() {
    new karmaServer({
        configFile: __dirname + "/test/config/karma.conf.js",
        keepalive: true
    })
    .start();
});

gulp.task('test:e2e', function() {
    new karmaServer({
        configFile: __dirname + "/test/config/e2e.js",
        keepalive: true
    })
    .start();
});
