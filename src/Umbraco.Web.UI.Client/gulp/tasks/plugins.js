'use strict';

var config = require('../config');
var gulp = require('gulp');

var _ = require('lodash');
var MergeStream = require('merge-stream');

gulp.task('plugins', function () {

    var stream = new MergeStream();

    _.forEach(config.sources.plugins, function (group) {

        console.log("copying " + group.files + " to " + config.root + config.targets.plugins + group.folder)

        stream.add (
            gulp.src(group.files)
                .pipe( gulp.dest(config.root + config.targets.plugins + group.folder) )
        );

    });

    return stream;
});
