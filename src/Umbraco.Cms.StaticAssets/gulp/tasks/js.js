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
    // NOTE: if you change something here, you probably also need to change it in the processJs util
    if (config.compile.current.minify === true) {
      task = task.pipe(
        minify({
          noSource: true,
          ext: { min: '.min.js' },
          mangle: false,
          compress: {
            keep_classnames: true,
            keep_fnames: true
          }
        })
      );
    } else {
      // rename the un-minified file so the client can reference it as '.min.js'
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
