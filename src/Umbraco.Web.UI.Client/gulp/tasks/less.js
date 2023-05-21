'use strict';

var config = require('../config');

var _ = require('lodash');
var MergeStream = require('merge-stream');

var processLess = require('../util/processLess');

function less() {

    var stream = new MergeStream();

    _.forEach(config.sources.less, function (group) {
        stream.add( processLess(group.files, group.out) );
    });

    return stream;
};

module.exports = { less: less };
