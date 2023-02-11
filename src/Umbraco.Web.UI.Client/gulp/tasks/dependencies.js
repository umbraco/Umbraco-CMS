'use strict';

var config = require('../config');
var gulp = require('gulp');

var MergeStream = require('merge-stream');

var imagemin = require('gulp-imagemin');
var _ = require('lodash');

/**************************
 * Task processes and copies all dependencies, either installed by npm or stored locally in the project
 **************************/
function dependencies() {

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
                "./node_modules/ace-builds/src-min-noconflict/snippets/json.js",
                "./node_modules/ace-builds/src-min-noconflict/snippets/xml.js",
                "./node_modules/ace-builds/src-min-noconflict/theme-chrome.js",
                "./node_modules/ace-builds/src-min-noconflict/mode-razor.js",
                "./node_modules/ace-builds/src-min-noconflict/mode-javascript.js",
                "./node_modules/ace-builds/src-min-noconflict/mode-css.js",
                "./node_modules/ace-builds/src-min-noconflict/mode-json.js",
                "./node_modules/ace-builds/src-min-noconflict/mode-xml.js",
                "./node_modules/ace-builds/src-min-noconflict/worker-javascript.js",
                "./node_modules/ace-builds/src-min-noconflict/worker-css.js",
                "./node_modules/ace-builds/src-min-noconflict/worker-json.js",
                "./node_modules/ace-builds/src-min-noconflict/worker-xml.js"
            ],
            "base": "./node_modules/ace-builds"
        },
        {
            "name": "angular",
            "src":  [
                "./node_modules/angular/angular.min.js",
                "./node_modules/angular/angular.min.js.map"
            ],
            "base": "./node_modules/angular"
        },
        {
            "name": "angular-aria",
            "src":  ["./node_modules/angular-aria/angular-aria.min.js",
                    "./node_modules/angular-aria/angular-aria.min.js.map"],
            "base": "./node_modules/angular-aria"
        },
        {
            "name": "angular-cookies",
            "src":  [
                "./node_modules/angular-cookies/angular-cookies.min.js",
                "./node_modules/angular-cookies/angular-cookies.min.js.map"
            ],
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
            "src":  [
                "./node_modules/angular-sanitize/angular-sanitize.min.js",
                "./node_modules/angular-sanitize/angular-sanitize.min.js.map"
            ],
            "base": "./node_modules/angular-sanitize"
        },
        {
            "name": "angular-touch",
            "src":  [
                "./node_modules/angular-touch/angular-touch.min.js",
                "./node_modules/angular-touch/angular-touch.min.js.map"
            ],
            "base": "./node_modules/angular-touch"
        },
        {
            "name": "angular-ui-sortable",
            "src":  ["./node_modules/angular-ui-sortable/dist/sortable.min.js"],
            "base": "./node_modules/angular-ui-sortable/dist"
        },
        {
            "name": "angular-route",
            "src":  [
                "./node_modules/angular-route/angular-route.min.js",
                "./node_modules/angular-route/angular-route.min.js.map"
            ],
            "base": "./node_modules/angular-route"
        },
        {
            "name": "angular-animate",
            "src":  [
                "./node_modules/angular-animate/angular-animate.min.js",
                "./node_modules/angular-animate/angular-animate.min.js.map"
            ],
            "base": "./node_modules/angular-animate"
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
            "src":  [
                "./node_modules/angular-messages/angular-messages.min.js",
                "./node_modules/angular-messages/angular-messages.min.js.map"
            ],
            "base": "./node_modules/angular-messages"
        },
        {
            "name": "angular-mocks",
            "src":  ["./node_modules/angular-mocks/angular-mocks.js"],
            "base": "./node_modules/angular-mocks"
        },
        {
            "name": "animejs",
            "src":  ["./node_modules/animejs/lib/anime.min.js"],
            "base": "./node_modules/animejs/lib"
        },
        {
            "name": "bootstrap-social",
            "src":  ["./node_modules/bootstrap-social/bootstrap-social.css"],
            "base": "./node_modules/bootstrap-social"
        },

        {
            "name": "angular-chart.js",
            "src":  [
                "./node_modules/angular-chart.js/dist/angular-chart.min.js",
                "./node_modules/angular-chart.js/dist/angular-chart.min.js.map"
            ],
            "base": "./node_modules/angular-chart.js/dist"
        },
        {
            "name": "chart.js",
            "src":  ["./node_modules/chart.js/dist/Chart.min.js"],
            "base": "./node_modules/chart.js/dist"
        },
        {
            "name": "clipboard",
            "src":  ["./node_modules/clipboard/dist/clipboard.min.js"],
            "base": "./node_modules/clipboard/dist"
        },
        {
            "name": "jsdiff",
            "src":  ["./node_modules/diff/dist/diff.js"],
            "base": "./node_modules/diff/dist"
        },
        {
            "name": "flatpickr",
            "src":  [
                "./node_modules/flatpickr/dist/flatpickr.min.js",
                "./node_modules/flatpickr/dist/flatpickr.min.css",
                "./node_modules/flatpickr/dist/l10n/*.js"
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
            "src":  ["./node_modules/lazyload-js/LazyLoad.min.js"],
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
                "./node_modules/nouislider/dist/nouislider.min.js",
                "./node_modules/nouislider/dist/nouislider.min.css"
            ],
            "base": "./node_modules/nouislider/dist"
        },
        {
            "name": "signalr",
            "src":  [
                "./node_modules/@microsoft/signalr/dist/browser/signalr.min.js",
            ],
            "base": "./node_modules/@microsoft/signalr/dist/browser"
        },
        {
            "name": "spectrum",
            "src":  [
                "./node_modules/spectrum-colorpicker2/dist/spectrum.js",
                "./node_modules/spectrum-colorpicker2/dist/spectrum.min.css"
            ],
            "base": "./node_modules/spectrum-colorpicker2/dist"
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
        },
        {
            "name": "wicg-inert",
            "src": [
                "./node_modules/wicg-inert/dist/inert.min.js",
                "./node_modules/wicg-inert/dist/inert.min.js.map"
            ],
            "base": "./node_modules/wicg-inert"
        },
        {
            "name": "umbraco-ui",
            "src": [
                "./node_modules/@umbraco-ui/uui/dist/uui.min.js",
                "./node_modules/@umbraco-ui/uui/dist/uui.min.js.map",
                "./node_modules/@umbraco-ui/uui-css/dist/custom-properties.css",
                "./node_modules/@umbraco-ui/uui-css/dist/uui-text.css"
            ],
            "base": "./node_modules/@umbraco-ui"
        }
    ];

    // add streams for node modules
    nodeModules.forEach(module => {
        var task = gulp.src(module.src, { base: module.base, allowEmpty: true });

        _.forEach(config.roots, function(root){
            task = task.pipe(gulp.dest(root + config.targets.lib + "/" + module.name))
        });

        stream.add(task);
    });

    //copy over libs which are not on npm (/lib)
    var libTask = gulp.src(config.sources.globs.lib, { allowEmpty: true });

    _.forEach(config.roots, function(root){
        libTask = libTask.pipe(gulp.dest(root + config.targets.lib))
    });

    stream.add(libTask);

    //Copies all static assets into /root / assets folder
    //css, fonts and image files

    var assetsTask = gulp.src(config.sources.globs.assets, { allowEmpty: true });
    assetsTask = assetsTask.pipe(imagemin([
        imagemin.gifsicle({interlaced: true}),
        imagemin.mozjpeg({progressive: true}),
        imagemin.optipng({optimizationLevel: 5}),
        imagemin.svgo({
            plugins: [
                {removeViewBox: true},
                {cleanupIDs: false}
            ]
        })
    ]));

    _.forEach(config.roots, function(root){
        assetsTask = assetsTask.pipe(gulp.dest(root + config.targets.assets));
    });


    stream.add(assetsTask);

    // Copies all the less files related to the preview into their folder
    //these are not pre-processed as preview has its own less compiler client side
    var lessTask = gulp.src("src/canvasdesigner/editors/*.less", { allowEmpty: true });

    _.forEach(config.roots, function(root){
        lessTask = lessTask.pipe(gulp.dest(root + config.targets.assets + "/less"));
    });
    stream.add(lessTask);



	// TODO: check if we need these fileSize
    var configTask = gulp.src("src/views/propertyeditors/grid/config/*.*", { allowEmpty: true });
    _.forEach(config.roots, function(root){
        configTask = configTask.pipe(gulp.dest(root + config.targets.views + "/propertyeditors/grid/config"));
    });
    stream.add(configTask);

    var dashboardTask = gulp.src("src/views/dashboard/default/*.jpg", { allowEmpty: true });
    _.forEach(config.roots, function(root){
        dashboardTask = dashboardTask .pipe(gulp.dest(root + config.targets.views + "/dashboard/default"));
    });
    stream.add(dashboardTask);

    return stream;
};

module.exports = { dependencies: dependencies };
