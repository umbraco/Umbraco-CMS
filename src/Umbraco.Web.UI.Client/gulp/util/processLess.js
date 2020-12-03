
var config = require('../config');
var gulp = require('gulp');

var postcss = require('gulp-postcss');
var less = require('gulp-less');
var autoprefixer = require('autoprefixer');
var cssnano = require('cssnano');
var cleanCss = require('gulp-clean-css');
var rename = require('gulp-rename');
var sourcemaps = require('gulp-sourcemaps');
var _ = require('lodash');

module.exports = function(files, out) {

    var processors = [
        autoprefixer,
        cssnano({zindex: false})
    ];
    _.forEach(config.roots, function(root){
        console.log("LESS: ", files, " -> ", root + config.targets.css + out);
    })
    

    var task = gulp.src(files);

    if(config.compile.current.sourcemaps === true) {
        task = task.pipe(sourcemaps.init());
    }

    task = task.pipe(less());
    task = task.pipe(cleanCss());
    task = task.pipe(postcss(processors));
    task = task.pipe(rename(out));

    if(config.compile.current.sourcemaps === true) {
        task = task.pipe(sourcemaps.write('./maps'));
    }

    _.forEach(config.roots, function(root){
        task = task.pipe(gulp.dest(root + config.targets.css));
    })

    
   

    return task;

};
