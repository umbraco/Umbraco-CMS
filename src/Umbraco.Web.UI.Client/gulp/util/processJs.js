
var config = require('../config');
var gulp = require('gulp');

var eslint = require('gulp-eslint');
var babel = require("gulp-babel");
var sort = require('gulp-sort');
var concat = require('gulp-concat');
var wrap = require("gulp-wrap-js");

module.exports = function(files, out) {

    return gulp.src(files)
        // check for js errors
        .pipe(eslint())
        // outputs the lint results to the console
        .pipe(eslint.format())
        // sort files in stream by path or any custom sort comparator
        .pipe(babel())
        .pipe(sort())
        .pipe(concat(out))
        .pipe(wrap('(function(){\n%= body %\n})();'))
        .pipe(gulp.dest(config.root + config.targets.js));

        console.log(out + " compiled");
};
