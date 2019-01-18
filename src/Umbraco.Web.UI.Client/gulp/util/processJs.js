
var config = require('../config');
var gulp = require('gulp');

var eslint = require('gulp-eslint');
var babel = require("gulp-babel");
var sort = require('gulp-sort');
var concat = require('gulp-concat');
var wrap = require("gulp-wrap-js");

module.exports = function(files, out) {
    
    var task = gulp.src(files);
    
    if (global.isProd === true) {
        // check for js errors
        task = task.pipe(eslint());
        // outputs the lint results to the console
        task = task.pipe(eslint.format());
    }
    
    // sort files in stream by path or any custom sort comparator
    task = task.pipe(babel())
        .pipe(sort())
        .pipe(concat(out))
        .pipe(wrap('(function(){\n%= body %\n})();'))
        .pipe(gulp.dest(config.root + config.targets.js));
    
    
    return task;
    
};