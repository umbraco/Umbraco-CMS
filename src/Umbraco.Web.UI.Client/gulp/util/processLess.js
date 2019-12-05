
var config = require('../config');
var gulp = require('gulp');

var postcss = require('gulp-postcss');
var less = require('gulp-less');
var autoprefixer = require('autoprefixer');
var cssnano = require('cssnano');
var cleanCss = require('gulp-clean-css');
var rename = require('gulp-rename');
var sourcemaps = require('gulp-sourcemaps');

module.exports = function(files, out) {

    var processors = [
        autoprefixer,
        cssnano({zindex: false})
    ];

    console.log("LESS: ", files, " -> ", config.root + config.targets.css + out)

    var task = gulp.src(files)
        .pipe(sourcemaps.init())
        .pipe(less())
        .pipe(cleanCss())
        .pipe(postcss(processors))
        .pipe(rename(out))
        .pipe(sourcemaps.write('./maps'))
        .pipe(gulp.dest(config.root + config.targets.css));

    return task;

};
