'use strict';

var config = require('../config');
var gulp = require('gulp');
var runSequence = require('run-sequence');

// Build - build the files ready for production
gulp.task('build', function(cb) {
    runSequence(["typescript", "js", "dependencies", "less", "views"], "test:unit", cb);
});
