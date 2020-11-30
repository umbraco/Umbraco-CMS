'use strict';

var config = require('../config');
var gulp = require('gulp');
var rename = require('gulp-rename');

var _ = require('lodash');
var MergeStream = require('merge-stream');

function views() {

    var stream = new MergeStream();

    _.forEach(config.sources.views, function (group) {

        var task = gulp.src(group.files)
            .pipe(rename(function (path) {
                path.dirname = path.dirname.toLowerCase();
                path.basename = path.basename.toLowerCase();
                path.extname = path.extname.toLowerCase();
            }));

        var destPath = config.root + config.targets.views + group.folder;
        console.log("copying " + group.files + " to " + destPath)
        task = task.pipe(gulp.dest(destPath));

        stream.add(task);

    });

    return stream;
};

module.exports = { views: views };

