
var config = require('../config');
var gulp = require('gulp');

var postcss = require('gulp-postcss');
var less = require('gulp-less');
var autoprefixer = require('autoprefixer');
var cssnano = require('cssnano');
var cleanCss = require("gulp-clean-css");
var rename = require('gulp-rename');

module.exports = function(files, out) {
    
    var processors = [
        autoprefixer,
        cssnano({zindex: false})
    ];

    var task = gulp.src(files)
        .pipe(less());
    
    
    if (global.isProd === true) {
        task = task.pipe(cleanCss());
    }
    
    task = task.pipe(postcss(processors))
        .pipe(rename(out))
        .pipe(gulp.dest(config.root + config.targets.css));
    
    return task;
    
};