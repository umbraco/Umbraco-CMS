'use strict';

var config = require('../config');

var _ = require('lodash');
var MergeStream = require('merge-stream');

var processCss = require('../util/processCss');

function css() {

    var stream = new MergeStream();

    _.forEach(config.sources.css, function (group) {
        stream.add( processCss(group.files, group.out) );
    });

    return stream;
};

module.exports = { css: css };
