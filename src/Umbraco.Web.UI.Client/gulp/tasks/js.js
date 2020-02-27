'use strict';

var config = require('../config');
var gulp = require('gulp');

var MergeStream = require('merge-stream');

var processJs = require('../util/processJs');

/**************************
 * Copies all angular JS files into their separate umbraco.*.js file
 **************************/
function js() {

    //we run multiple streams, so merge them all together
    var stream = new MergeStream();

    stream.add(
        gulp.src(config.sources.globs.js).pipe( gulp.dest(config.root + config.targets.js) )
    );

    Object.keys(config.sources.js).forEach(key => {
        const groupItem = config.sources.js[key];
        stream.add(processJs(groupItem.files, groupItem.out));
    });

    return stream;
};

module.exports = { js: js };
