'use strict';

var config = require('./config');
var gulp = require('gulp');

function setDevelopmentMode(cb) {

    config.compile.current = config.compile.dev;

    return cb();
};

function setTestMode(cb) {

    config.compile.current = config.compile.test;

    return cb();
};

module.exports = { 
    setDevelopmentMode: setDevelopmentMode,
    setTestMode: setTestMode
 };
