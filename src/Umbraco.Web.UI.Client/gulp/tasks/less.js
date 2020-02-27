'use strict';

var config = require('../config');
var MergeStream = require('merge-stream');
var processLess = require('../util/processLess');

function less() {

    var stream = new MergeStream();

    for(const group in config.sources.less){
        const groupItem = config.sources.less[group];
        stream.add(processLess(groupItem.files, groupItem.out));
    }

    return stream;
};

module.exports = { less: less };
