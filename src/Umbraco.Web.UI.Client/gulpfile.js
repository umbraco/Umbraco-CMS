var gulp = require('gulp');
var watch = require("gulp-watch");
var concat = require("gulp-concat");
var rename = require("gulp-rename");
var wrap = require("gulp-wrap");

var _ = require("lodash");

//Less + css
var postcss = require('gulp-postcss');
var less = require('gulp-less');
var autoprefixer = require('autoprefixer');
var cssnano = require('cssnano');


/***************************************************************
Helper functions
***************************************************************/
function processJs(files, out) {
    console.log("Compiling JS:" + files + " to " + out);

    return gulp.src(files)
     .pipe(concat(out))
     .pipe(wrap('(function(){\n"use strict";\n<%= contents %>\n})();'))
     .pipe(gulp.dest(root + targets.js));
}

function processLess(files, out) {
    console.log("Compiling LESS:" + files + " to " + out);

    var processors = [
         autoprefixer,
         cssnano
    ];

    return gulp.src(files)
        .pipe(less())
        .pipe(postcss(processors))
        .pipe(rename(out))
        .pipe(gulp.dest(root + targets.css));
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
        preview: { files: ['src/less/canvas-designer.less', 'src/less/helveticons.less'], out: "canvasdesigner.css" },
        umbraco: { files: ["src/less/belle.less"], out: "umbraco.css" }
    },
    
    //js files for backoffie
    //processed in the js task
    js: {
        preview: { files: ["src/canvasdesigner/**/*.js"], out: "umbraco.canvasdesigner.js" },
        installer: { files: ["src/installer/**/*.js"], out: "umbraco.installer.js" },
        
        controllers: { files: ["src/views/**/*.controller.js"], out: "umbraco.controllers.js" },
        directives: { files: ["src/common/directives/**/*.js"], out: "umbraco.directives.js" },
        filters: { files: ["src/common/filters/**/*.js"], out: "umbraco.filters.js" },
        resources: { files: ["src/common/resources/**/*.js"], out: "umbraco.resources.js" },
        services: { files: ["src/common/services/**/*.js"], out: "umbraco.services.js" },
        security: { files: ["src/common/security/**/*.js"], out: "umbraco.security.js" }
    },

    //selectors for copying all views into the build
    //processed in the views task
    views:{
        umbraco: {files: ["src/views/**/*html"], folder: "/"},
        preview: { files: ["src/canvasdesigner/**/*.html"], folder: "../preview"},
        installer: {files: ["src/installer/steps/*.html"], folder: "/install"}
    },

    //globs for file-watching
    globs:{
        views: "src/views/**/*html",
        less: "src/less*.less",
        js: "src/*.js",
        lib: "lib/**/*.*, lib-bower/**/*.*",
        assets: "src/assets/**"    
    }
};

var root = "../__BUILD/umbraco/";
var targets = {
    js: "js",
    lib: "lib",
    views: "views",
    css: "assets/css",
    assets: "assets"
};


/**************************
 * Runs the default tasks for the project to prepare backoffice files
 **************************/
var defaultTasks = ["dependencies","js", "less", "views"];
gulp.task("default", defaultTasks);


/**************************
 * Task processes and copies all dependencies, either installed by bower, npm or stored locally in the project
 **************************/
gulp.task('dependencies', function () { 

    //bower component specific copy rules
    //this is to patch the sometimes wonky rules these libs are distrbuted under

    //Tinymce
    gulp.src("bower_components/tinymce/plugins/**")
        .pipe(gulp.dest(root + targets.lib + "/tinymce/plugins/"));

    //font-awesome
    gulp.src("bower_components/font-awesome/{fonts,css}/*")
        .pipe(gulp.dest(root + targets.lib + "/font-awesome"));

    //copy over libs which are not on bower (/lib) and 
    //libraries that have been managed by bower-installer (/lib-bower)
    gulp.src(sources.globs.lib)
     .pipe(gulp.dest(root + targets.lib));

    //Copies all static assets into /root / assets folder 
    //css, fonts and image files
    gulp.src(sources.globs.assets)
        .pipe(gulp.dest(root + targets.assets));

    // Copies all the less files related to the preview into their folder
    //these are not pre-processed as preview has its own less combiler client side
    gulp.src("src/canvasdesigner/editors/*.less")
        .pipe(gulp.dest(root + targets.assets + "/less"));     
});


/**************************
 * Copies all angular JS files into their seperate umbraco.*.js file
 **************************/
gulp.task('js', function () { 

  gulp.src(sources.globs.js)
     .pipe(gulp.dest(root + targets.js));

    return _.forEach(sources.js, function (group) {
        processJs(group.files, group.out);
    });
});

gulp.task('less', function () {
    
    gulp.src(sources.globs.assets)
     .pipe(gulp.dest(targets.assets));

    return _.forEach(sources.less, function (group) {
        processLess(group.files, group.out);
    });
});


gulp.task('views', function () {

    return _.forEach(sources.views, function (group) {
        gulp.src(group.files)
            .pipe(gulp.dest(root + targets.views + group.folder));
    });
   
});


gulp.task('watch', defaultTasks, function () {

    //Setup a watcher for all groups of javascript files
    _.forEach(sources.js, function (group) {
        if(group.watch !== false){
            watch(group.files, { ignoreInitial: true }, function () {
                processJs(group.files, group.out);
            });
        }
    });

    //watch all less files and trigger the less task
    watch(sources.globs.less, { ignoreInitial: true }, function () {
        gulp.run(['less']);
    });

    //watch all views - copy single file changes
    watch(sources.globs.views)
        .pipe(gulp.dest(root + targets.views));

    //watch all app js files that will not be merged - copy single file changes
    watch(sources.globs.js)
        .pipe(gulp.dest(root + targets.js));    
});