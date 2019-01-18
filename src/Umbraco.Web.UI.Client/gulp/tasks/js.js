'use strict';

var config = require('../config');
var gulp = require('gulp');

var _ = require('lodash');
var MergeStream = require('merge-stream');

var processJs = require('../util/processJs');

/**************************
 * Copies all angular JS files into their seperate umbraco.*.js file
 **************************/
gulp.task('js', function () {

    //we run multiple streams, so merge them all together
    var stream = new MergeStream();

    stream.add(
        gulp.src(config.sources.globs.js)
            .pipe(gulp.dest(config.root + config.targets.js))
        );

     _.forEach(config.sources.js, function (group) {
        stream.add (processJs(group.files, group.out) );
     });

     return stream;
});
