'use strict';

var config = require('../config');
var gulp = require('gulp');

var MergeStream = require('merge-stream');

var imagemin = require('gulp-imagemin');

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
            "name": "angular-chart.js",
            "src":  ["./node_modules/angular-chart.js/dist/angular-chart.min.js"],
            "base": "./node_modules/angular-chart.js/dist"
        },
        {
            "name": "chart.js",
            "src":  ["./node_modules/chart.js/dist/chart.min.js"],
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
                .pipe(gulp.dest(config.root + config.targets.lib + "/" + module.name))
        );
    });

    //copy over libs which are not on npm (/lib)
    stream.add(
         gulp.src(config.sources.globs.lib)
            .pipe(gulp.dest(config.root + config.targets.lib))
        );

    //Copies all static assets into /root / assets folder
    //css, fonts and image files
    stream.add(
            gulp.src(config.sources.globs.assets)
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
                .pipe(gulp.dest(config.root + config.targets.assets))
        );

    // Copies all the less files related to the preview into their folder
    //these are not pre-processed as preview has its own less combiler client side
    stream.add(
            gulp.src("src/canvasdesigner/editors/*.less")
                .pipe(gulp.dest(config.root + config.targets.assets + "/less"))
        );

	// Todo: check if we need these fileSize
    stream.add(
            gulp.src("src/views/propertyeditors/grid/config/*.*")
                .pipe(gulp.dest(config.root + config.targets.views + "/propertyeditors/grid/config"))
        );
    stream.add(
            gulp.src("src/views/dashboard/default/*.jpg")
                .pipe(gulp.dest(config.root + config.targets.views + "/dashboard/default"))
        );

    return stream;
});
