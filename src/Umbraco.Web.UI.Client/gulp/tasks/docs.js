'use strict';

var config = require('../config');
var { src, dest } = require('gulp');

var connect = require('gulp-connect');
var open = require('gulp-open');
var gulpDocs = require('gulp-ngdocs');

/**************************
 * Build Backoffice UI API documentation
 **************************/
function docs(cb) {

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
    .pipe(dest('docs/api'));
    cb();
};

function connectDocs(cb) {
    connect.server({
        root: 'docs/api',
        livereload: true,
        fallback: 'docs/api/index.html',
        port: 8880
    });
    cb();
};

function openDocs(cb) {

    var options = {
        uri: 'http://localhost:8880/index.html'
    };

    src(__filename)
    .pipe(open(options));
    cb();
};

module.exports = { docs: docs, connectDocs: connectDocs, openDocs: openDocs };
