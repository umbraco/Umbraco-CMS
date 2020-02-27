'use strict';

var config = require('../config');
var gulp = require('gulp');
var MergeStream = require('merge-stream');

function views() {

    var stream = new MergeStream();

    for(const group in config.sources.views){
        const groupItem = config.sources.views[group];

        console.log(`Copying ${groupItem.files} to ${config.root}${config.targets.views}${groupItem.folder}`);
        stream.add (
            gulp.src(groupItem.files)
                .pipe( gulp.dest(config.root + config.targets.views + groupItem.folder) )
        );
    }

    return stream;
};

module.exports = { views: views };

