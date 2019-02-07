'use strict';

var config = require('../config');
var gulp = require('gulp');
var karmaServer = require('karma').Server;
var runSequence = require('run-sequence');

/**************************
 * Build tests
 **************************/

 // Karma test
gulp.task('runTests', function(cb) {
    runSequence(["js"], "test:unit", cb);
});

gulp.task('test:unit', function () {

    new karmaServer({
        configFile: __dirname + "/../../test/config/karma.conf.js",
        keepalive: true
    })
    .start();
});

gulp.task('test:e2e', function() {
    new karmaServer({
        configFile: __dirname + "/../../test/config/e2e.js",
        keepalive: true
    })
    .start();
});
