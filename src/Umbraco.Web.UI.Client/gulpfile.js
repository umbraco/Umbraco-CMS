
'use strict';

/*
    * gulpfile.js
    * ===========
    * This is now using Gulp 4, each child task is now a child function in its own corresponding file.
    *
    * To add a new task, simply add a new task file to gulp/tasks folder, add a require statement below to include the one or more methods 
    * and then add the exports command to add the new item into the task menu.
    */

global.isProd = true;

const { src, dest, series, parallel, lastRun } = require('gulp');

const { dependencies } = require('./gulp/tasks/dependencies');
const { docs, connectDocs, openDocs } = require('./gulp/tasks/docs');
const { js } = require('./gulp/tasks/js');
const { less } = require('./gulp/tasks/less');
const { testE2e, testUnit } = require('./gulp/tasks/test');
const { views } = require('./gulp/tasks/views');
const { watch } = require('./gulp/tasks/watch');



// ***********************************************************
// Enables users to have personal configuration without manipulating the config file.
// Usefull if you dont want to commit your personal configuration.
// ***********************************************************
var { readdirSync } = require('fs');

var onlyScripts = require('./gulp/util/scriptFilter');
var tasks = readdirSync('./gulp/extra/').filter(onlyScripts);
tasks.forEach(function(task) {
	require('./gulp/extra/' + task);
});


// ***********************************************************
// These Exports are the new way of defining Tasks in Gulp 4.x
// ***********************************************************
exports.build = series(dependencies, js, less, views, testUnit);
exports.dev = series(dependencies, js, less, views, watch);
exports.fastdev = series(dependencies, js, less, views, watch);

exports.docs = series(docs);
exports.connectDocs = series(connectDocs);
exports.openDocs = series(openDocs);
exports.docserve = series(docs, connectDocs, openDocs);

exports.js = series(js);
exports.views = series(views);
exports.watch = series(watch);

exports.runTests = series(js, testUnit);
exports.testUnit = series(testUnit);
exports.testE2e = series(testE2e);
