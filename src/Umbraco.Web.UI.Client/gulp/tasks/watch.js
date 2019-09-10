'use strict';

var config = require('../config');
var { watch, dest, src } = require('gulp');

var _ = require('lodash');

var processJs = require('../util/processJs');
var processLess = require('../util/processLess');

function watching() {

    var watchInterval = 500;

    //Setup a watcher for all groups of javascript files
    _.forEach(config.sources.js, function (group) {

            watch(group.files, { ignoreInitial: true, interval: watchInterval }, function (file) {

                console.info(file.path + " has changed, added to:  " + group.out);
                processJs(group.files, group.out);

            })

    });

        //watch all less files and trigger the less task
    watch(config.sources.globs.less, { ignoreInitial: true, interval: watchInterval }, function () {
        console.log('Reprocessing the LESS files');
        processLess();
    })

    // watch all views - copy single file changes
    const viewWatcher = watch(config.sources.globs.views, { interval: watchInterval });

    viewWatcher.on('change', function (path) {
        console.log(`File ${path} was changed`);
        src(path).pipe(dest(config.root + config.targets.views))
    });

    // watch all app js files that will not be merged - copy single file changes
    const jsAppWatcher = watch(config.sources.globs.js, { interval: watchInterval });

    jsAppWatcher.on('change', function (path) {
        console.log(`File ${path} was changed`);
        src(path).pipe(dest(config.root + config.targets.js))
    });

};

module.exports = { watch: watching };
