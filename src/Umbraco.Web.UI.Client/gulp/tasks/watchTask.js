'use strict';

const config = require('../config');
const {watch, dest, src} = require('gulp');
const processJs = require('../util/processJs');
const processLess = require('../util/processLess');


function watchTask(cb) {

    const watchInterval = 500;

    //Setup a watcher for all groups of JS files
    Object.keys(config.sources.js).forEach(key => {
        const groupItem = config.sources.js[key];
        if(groupItem.watch !== false){
            watch(groupItem.files, { ignoreInitial: true, interval: watchInterval }, function JS_Group_Compile() { return processJs(groupItem.files, groupItem.out);});
        }
    });

    //Setup a watcher for all groups of LESS files
    Object.keys(config.sources.less).forEach(key => {
        const groupItem = config.sources.less[key];
        if(groupItem.watch !== false){
            watch(groupItem.watch, { ignoreInitial: true, interval: watchInterval }, function Less_Group_Compile() { return processLess(groupItem.files, groupItem.out);});
        }
    });

    //Setup a watcher for all groups of view files
    let viewWatcher;
    Object.keys(config.sources.views).forEach(key => {
        const groupItem = config.sources.views[key];
        if(groupItem.watch !== false){
            viewWatcher = watch(groupItem.files, { ignoreInitial: true, interval: watchInterval });
            viewWatcher.on("change", function(path, stats) {
                console.log(`Copying ${groupItem.files} to ${config.root}${config.targets.views}${groupItem.folder}`);
                src(groupItem.files).pipe( dest(config.root + config.targets.views + groupItem.folder) );
            });
        }
    });

    return cb();
};

module.exports = { watchTask: watchTask };
