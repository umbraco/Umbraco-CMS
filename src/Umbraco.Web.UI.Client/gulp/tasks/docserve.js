'use strict';

var config = require('../config');
var gulp = require('gulp');
var runSequence = require('run-sequence');

// Docserve - build and open the back office documentation
gulp.task('docserve', function(cb) {
    runSequence('docs', 'connect:docs', 'open:docs', cb);
});
