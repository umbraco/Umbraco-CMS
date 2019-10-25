'use strict';

var gulp = require('gulp');
var through2 = require('through2');

function createEmptyStream() {
    var pass = through2.obj();
    process.nextTick(pass.end.bind(pass));
    return pass;
}

/**************************
 * Copies all angular JS files into their separate umbraco.*.js file
 **************************/
function removeProductionMode() {
    
    global.isProd = false;
    
	return createEmptyStream();
};

module.exports = { removeProductionMode: removeProductionMode };
