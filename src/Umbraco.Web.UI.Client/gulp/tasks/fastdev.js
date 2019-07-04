'use strict';

var config = require('../config');
var gulp = require('gulp');
var runSequence = require('run-sequence');

// Dev - build the files ready for development and start watchers
gulp.task('fastdev', function(cb) {
    
    global.isProd = false;
    
    runSequence("views", ["dependencies", "js", "less"], "watch", cb);
});