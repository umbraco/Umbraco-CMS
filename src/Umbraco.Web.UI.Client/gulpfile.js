
'use strict';

/*
    * gulpfile.js
    * ===========
    * This is now using Gulp 4, each child task is now a child function in its own corresponding file.
    *
    * To add a new task, simply add a new task file to gulp/tasks folder, add a require statement below to include the one or more methods 
    * and then add the exports command to add the new item into the task menu.
    */

const { src, dest, series, parallel, lastRun } = require('gulp');

const config = require('./gulp/config');
const { setDevelopmentMode, setTestMode } = require('./gulp/modes');
const { dependencies } = require('./gulp/tasks/dependencies');
const { js } = require('./gulp/tasks/js');
const { less } = require('./gulp/tasks/less');
const { testE2e, testUnit, runUnitTestServer } = require('./gulp/tasks/test');
const { views } = require('./gulp/tasks/views');
const { watchTask } = require('./gulp/tasks/watchTask');

// set default current compile mode:
config.compile.current = config.compile.build;

// ***********************************************************
// These Exports are the new way of defining Tasks in Gulp 4.x
// ***********************************************************
exports.build = series(parallel(dependencies, js, less, views), testUnit);
exports.dev = series(setDevelopmentMode, parallel(dependencies, js, less, views), watchTask);
exports.watch = series(watchTask);
// 
exports.runTests = series(setTestMode, parallel(js, testUnit));
exports.runUnit = series(setTestMode, parallel(js, runUnitTestServer), watchTask);
exports.testE2e = series(setTestMode, parallel(testE2e));
