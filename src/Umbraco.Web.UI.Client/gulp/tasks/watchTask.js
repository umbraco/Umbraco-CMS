'use strict';

const config = require('../config');
const {watch, series, parallel, dest, src} = require('gulp');

var _ = require('lodash');
var MergeStream = require('merge-stream');

var processJs = require('../util/processJs');
var processLess = require('../util/processLess');

var {js} = require('./js');

function watchTask(cb) {

    var watchInterval = 500;

    //Setup a watcher for all groups of JS files
    _.forEach(config.sources.js, function (group) {
        if(group.watch !== false) {
            watch(group.files, { ignoreInitial: true, interval: watchInterval }, function JS_Group_Compile() { return processJs(group.files, group.out);});
        }
    });

    //Setup a watcher for all groups of LESS files
    _.forEach(config.sources.less, function (group) {
        if(group.watch !== false) {
            watch(group.watch, { ignoreInitial: true, interval: watchInterval }, function Less_Group_Compile() { return processLess(group.files, group.out); });
        }
    });

    //Setup a watcher for all groups of view files
    var viewWatcher;
    _.forEach(config.sources.views, function (group) {
        if(group.watch !== false) {
            viewWatcher = watch(group.files, { ignoreInitial: true, interval: watchInterval },
                parallel(
                    function MoveViewsAndRegenerateJS() {
                        var task = src(group.files);

                        _.forEach(config.roots, function(root){
                            var destPath = root + config.targets.views + group.folder;
                            console.log("copying " + group.files + " to " + destPath);
                            task = task.pipe( dest(destPath) );
                        });
                        return task;
                    },
                    js
                )
            );
        }
    });

    return cb();
};

module.exports = { watchTask: watchTask };
