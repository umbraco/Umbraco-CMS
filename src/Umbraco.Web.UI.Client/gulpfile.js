﻿var gulp = require('gulp');
var watch = require('gulp-watch');
var concat = require('gulp-concat');
var rename = require('gulp-rename');
var wrap = require("gulp-wrap-js");
var sort = require('gulp-sort');
var connect = require('gulp-connect');
var open = require('gulp-open');
var runSequence = require('run-sequence');
const imagemin = require('gulp-imagemin');

var _ = require('lodash');
var MergeStream = require('merge-stream');

//Less + css
var postcss = require('gulp-postcss');
var less = require('gulp-less');
var autoprefixer = require('autoprefixer');
var cssnano = require('cssnano');

// Documentation
var gulpDocs = require('gulp-ngdocs');

// Testing
var karmaServer = require('karma').Server;

/***************************************************************
Helper functions
***************************************************************/
function processJs(files, out) {
    
    return gulp.src(files)
     .pipe(sort())
     .pipe(concat(out))
     .pipe(wrap('(function(){\n%= body %\n})();'))
     .pipe(gulp.dest(root + targets.js));

     console.log(out + " compiled");
}

function processLess(files, out) {

    var processors = [
         autoprefixer,
         cssnano({zindex: false}),
    ];

    return gulp.src(files)
        .pipe(less())
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
        preview: { files: ["src/canvasdesigner/**/*.js"], out: "umbraco.canvasdesigner.js" },
        installer: { files: ["src/installer/**/*.js"], out: "umbraco.installer.js" },
        controllers: { files: ["src/{views,controllers}/**/*.controller.js"], out: "umbraco.controllers.js" },
        directives: { files: ["src/common/directives/**/*.js"], out: "umbraco.directives.js" },
        filters: { files: ["src/common/filters/**/*.js"], out: "umbraco.filters.js" },
        resources: { files: ["src/common/resources/**/*.js"], out: "umbraco.resources.js" },
        services: { files: ["src/common/services/**/*.js"], out: "umbraco.services.js" },
        security: { files: ["src/common/security/**/*.js"], out: "umbraco.security.js" }
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
        bower: "./lib-bower/**/*",
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
    runSequence(["dependencies", "js", "less", "views"], "test:unit", cb);
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
 * Task processes and copies all dependencies, either installed by bower, npm or stored locally in the project
 **************************/
gulp.task('dependencies', function () { 

    //bower component specific copy rules
    //this is to patch the sometimes wonky rules these libs are distrbuted under

    //as we do multiple things in this task, we merge the multiple streams
    var stream = new MergeStream();

    //Tinymce
    stream.add(
        gulp.src(["./bower_components/tinymce/plugins/**",
            "./bower_components/tinymce/themes/**"],
            { base: "./bower_components/tinymce/" })
            .pipe(gulp.dest(root + targets.lib + "/tinymce"))
    );

    //font-awesome
    stream.add(
        gulp.src(["./bower_components/font-awesome/fonts/*",
            "./bower_components/font-awesome/css/font-awesome.min.css"],
            { base: "./bower_components/font-awesome/" })
            .pipe(gulp.dest(root + targets.lib + "/font-awesome"))
    );
    
    // ace Editor
    stream.add(
        gulp.src(["bower_components/ace-builds/src-min-noconflict/ace.js",
            "bower_components/ace-builds/src-min-noconflict/ext-language_tools.js",
            "bower_components/ace-builds/src-min-noconflict/ext-searchbox.js",
            "bower_components/ace-builds/src-min-noconflict/ext-settings_menu.js",
            "bower_components/ace-builds/src-min-noconflict/snippets/text.js",
            "bower_components/ace-builds/src-min-noconflict/snippets/javascript.js",
            "bower_components/ace-builds/src-min-noconflict/theme-chrome.js",
            "bower_components/ace-builds/src-min-noconflict/mode-razor.js",
            "bower_components/ace-builds/src-min-noconflict/mode-javascript.js",
            "bower_components/ace-builds/src-min-noconflict/worker-javascript.js"],
            { base: "./bower_components/ace-builds/" })
            .pipe(gulp.dest(root + targets.lib + "/ace-builds"))
    );

    // code mirror
    stream.add(
        gulp.src([
            "bower_components/codemirror/lib/codemirror.js",
            "bower_components/codemirror/lib/codemirror.css",

            "bower_components/codemirror/mode/css/*",
            "bower_components/codemirror/mode/javascript/*",
            "bower_components/codemirror/mode/xml/*",
            "bower_components/codemirror/mode/htmlmixed/*",

            "bower_components/codemirror/addon/search/*",
            "bower_components/codemirror/addon/edit/*",
            "bower_components/codemirror/addon/selection/*",
            "bower_components/codemirror/addon/dialog/*"],
            { base: "./bower_components/codemirror/" })
            .pipe(gulp.dest(root + targets.lib + "/codemirror"))
    );

    //copy over libs which are not on bower (/lib) and 
    //libraries that have been managed by bower-installer (/lib-bower)
    stream.add(
         gulp.src(sources.globs.lib)
            .pipe(gulp.dest(root + targets.lib))
        );

    stream.add(
         gulp.src(sources.globs.bower)
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