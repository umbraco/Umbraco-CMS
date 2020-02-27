'use strict';

var config = require('../config');
var MergeStream = require('merge-stream');
var processLess = require('../util/processLess');

function less() {

    var stream = new MergeStream();

    Object.keys(config.sources.less).forEach(key => {
        const groupItem = config.sources.less[key];
        stream.add(processLess(groupItem.files, groupItem.out));
    });

    return stream;
};

module.exports = { less: less };
