'use strict';

var config = require('../config');
var gulp = require('gulp');
var runSequence = require('run-sequence');

// Dev - build the files ready for development and start watchers
gulp.task('dev', function(cb) {
    runSequence("views", ["dependencies", "less", "js"], "watch", cb);
});
