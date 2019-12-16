'use strict';

var config = require('./config');
var gulp = require('gulp');

function setDevelopmentMode(cb) {

    config.compile.current = config.compile.dev;

    return cb();
};

module.exports = { setDevelopmentMode: setDevelopmentMode };
