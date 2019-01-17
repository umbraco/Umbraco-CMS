'use strict';

var config = require('../config');
var gulp = require('gulp');

var _ = require('lodash');
var MergeStream = require('merge-stream');

var watch = require('gulp-watch');

gulp.task('watch', function () {

    var stream = new MergeStream();
    var watchInterval = 500;

    //Setup a watcher for all groups of javascript files
    _.forEach(config.sources.js, function (group) {

        if(group.watch !== false){

            stream.add(

                watch(group.files, { ignoreInitial: true, interval: watchInterval }, function (file) {

                    console.info(file.path + " has changed, added to:  " + group.out);
                    processJs(group.files, group.out);

                })

            );

        }

    });

    stream.add(
        //watch all less files and trigger the less task
        watch(config.sources.globs.less, { ignoreInitial: true, interval: watchInterval }, function () {
            gulp.run(['less']);
        })
    );

    //watch all views - copy single file changes
    stream.add(
        watch(config.sources.globs.views, { interval: watchInterval })
        .pipe(gulp.dest(config.root + config.targets.views))
    );

    //watch all app js files that will not be merged - copy single file changes
    stream.add(
        watch(config.sources.globs.js, { interval: watchInterval })
        .pipe(gulp.dest(config.root + config.targets.js))
    );

    return stream;
});
