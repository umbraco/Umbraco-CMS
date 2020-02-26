'use strict';

var karmaServer = require('karma').Server;

/**************************
 * Build tests
 **************************/

// Karma test
function testUnit() {

    return new karmaServer({
        configFile: __dirname + "/../../test/config/karma.conf.js"
    })
    .start();
};

// Run karma test server
function runUnitTestServer() {

    return new karmaServer({
        configFile: __dirname + "/../../test/config/karma.conf.js",
        autoWatch: true,
        port: 9999,
        singleRun: false,
        keepalive: true
    })
    .start();
};

function testE2e() {
    return new karmaServer({
        configFile: __dirname + "/../../test/config/e2e.js",
        keepalive: true
    })
    .start();
};

module.exports = { testUnit: testUnit, testE2e: testE2e, runUnitTestServer: runUnitTestServer };
