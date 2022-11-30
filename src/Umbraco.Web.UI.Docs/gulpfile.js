'use strict';


var config = require('../Umbraco.Web.UI.Client/gulp/config');
var gulp = require('gulp');

var connect = require('gulp-connect');
var open = require('gulp-open');
var gulpDocs = require('gulp-ngdocs');

var documentationFiles = ['../Umbraco.Web.UI.Client/src/common/**/*.js', './src/api/**/*.ngdoc'];

/**************************
 * Build Backoffice UI API documentation
 **************************/
gulp.task('docs', [], function (cb) {

    var options = {
        html5Mode: false,
        startPage: '/api',
        title: "Umbraco 10 Backoffice UI API Documentation",
        dest: './api',
        styles: ['./umb-docs.css'],
        image: "https://our.umbraco.com/assets/images/logo.svg"
    }

    return gulpDocs.sections({
        api: {
            glob: documentationFiles,
            api: true,
            title: 'UI API Documentation'
        }
    })
    .pipe(gulpDocs.process(options))
    .pipe(gulp.dest('./api'))
    .pipe(connect.reload());

});

gulp.task('connect:docs', function (cb) {
    connect.server({
        root: './api',
        livereload: true,
        fallback: './api/index.html',
        port: 8880
    });
    cb();
});

gulp.task('watch:docs', function (cb) {
    return gulp.watch(documentationFiles, ['docs']);
});

gulp.task('open:docs', function (cb) {

    var options = {
        uri: 'http://localhost:8880/index.html'
    };

    gulp.src(__filename)
    .pipe(open(options));
    cb();
});

gulp.task('watch', ['docs', 'connect:docs', 'open:docs', 'watch:docs']);

