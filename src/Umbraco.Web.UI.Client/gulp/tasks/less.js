'use strict';

var config = require('../config');
var gulp = require('gulp');

var _ = require('lodash');
var MergeStream = require('merge-stream');

var processLess = require('../util/processLess');

gulp.task('less', function () {

    var stream = new MergeStream();

    _.forEach(config.sources.less, function (group) {
        stream.add( processLess(group.files, group.out) );
    });

    return stream;
});
