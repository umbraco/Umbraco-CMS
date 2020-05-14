'use strict';

const config = require('../config');
const {watch, parallel, dest, src} = require('gulp');

var _ = require('lodash');
var MergeStream = require('merge-stream');

var processJs = require('../util/processJs');
var processLess = require('../util/processLess');

//const { less } = require('./less');
//const { views } = require('./views');


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
            viewWatcher = watch(group.files, { ignoreInitial: true, interval: watchInterval });
            viewWatcher.on('change', function(path, stats) {
                console.log("copying " + group.files + " to " + config.root + config.targets.views + group.folder);
                src(group.files).pipe( dest(config.root + config.targets.views + group.folder) );
            });
        }
    });
    
    return cb();
};

module.exports = { watchTask: watchTask };
