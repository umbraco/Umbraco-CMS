'use strict';

var config = require('../config');
var gulp = require('gulp');

var connect = require('gulp-connect');
var open = require('gulp-open');
var gulpDocs = require('gulp-ngdocs');

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
