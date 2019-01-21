'use strict';

var config = require('../config');
var gulp = require('gulp');

var _ = require('lodash');
var MergeStream = require('merge-stream');

gulp.task('views', function () {

    var stream = new MergeStream();

    _.forEach(config.sources.views, function (group) {

        console.log("copying " + group.files + " to " + config.root + config.targets.views + group.folder)

        stream.add (
            gulp.src(group.files)
                .pipe( gulp.dest(config.root + config.targets.views + group.folder) )
        );

    });

    return stream;
});
