
var config = require('../config');
var gulp = require('gulp');

var ts = require('gulp-typescript');

module.exports = function(files, out) {
    
    var task = gulp.src(files);

    var tsProject = ts.createProject('tsconfig.json');
    
    
    // sort files in stream by path or any custom sort comparator
    task = task.pipe(tsProject())
        .pipe(gulp.dest(out));
    
    
    return task;
    
};