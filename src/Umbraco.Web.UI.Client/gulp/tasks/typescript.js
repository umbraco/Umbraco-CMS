'use strict';

var config = require('../config');
var gulp = require('gulp');

var _ = require('lodash');
var MergeStream = require('merge-stream');

var processTs = require('../util/processTypescript');

/**************************
 * Copies all angular JS files into their seperate umbraco.*.js file
 **************************/
gulp.task('typescript', function () {

    //we run multiple streams, so merge them all together
    var stream = new MergeStream();

    stream.add(
        gulp.src(config.sources.globs.js)
            .pipe(gulp.dest(config.root + config.targets.js))
        );

      // compile TS
      _.forEach(config.sources.ts, function (group) {
         if(group.files.length > 0)
            stream.add (processTs(group.files, group.out));
      });

      return stream;
});
