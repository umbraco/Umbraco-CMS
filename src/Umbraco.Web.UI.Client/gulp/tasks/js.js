'use strict';

var config = require('../config');
var gulp = require('gulp');

var minify = require('gulp-minify');
var rename = require('gulp-rename');
var _ = require('lodash');
var MergeStream = require('merge-stream');

var processJs = require('../util/processJs');

/**************************
 * Copies all angular JS files into their separate umbraco.*.js file
 **************************/
function js() {

    //we run multiple streams, so merge them all together
    var stream = new MergeStream();

    var task = gulp.src(config.sources.globs.js);
    if (config.compile.current.minify === true) {
      task = task.pipe(
        minify({
          noSource: true,
          ext: { min: '.min.js' },
          mangle: false
        })
      );
    } else {
      task = task.pipe(rename(function(path) {
        path.basename += '.min';
      }));
    }
    _.forEach(config.roots, function(root){
        task = task.pipe( gulp.dest(root + config.targets.js) )
    })
    stream.add(task);
  
    _.forEach(config.sources.js, function (group) {
        stream.add(
            processJs(group.files, group.out)
        );
    });

     return stream;
};

module.exports = { js: js };
