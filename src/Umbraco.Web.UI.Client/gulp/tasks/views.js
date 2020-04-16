'use strict';

var config = require('../config');
var gulp = require('gulp');

var _ = require('lodash');
var MergeStream = require('merge-stream');

function views() {

    var stream = new MergeStream();

    _.forEach(config.sources.views, function (group) {

      

        var task = gulp.src(group.files);
        
        _.forEach(config.roots, function(root){
            console.log("copying " + group.files + " to " + root + config.targets.views + group.folder)
            task = task.pipe( gulp.dest(root + config.targets.views + group.folder));    
        })
        
        stream.add (task);

    });

    return stream;
};

module.exports = { views: views };

