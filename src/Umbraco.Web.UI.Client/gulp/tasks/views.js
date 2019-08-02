'use strict';

var config = require('../config');
var gulp = require('gulp');

var modifyFile = require('gulp-modify-file');

var jsdom = require('jsdom');
var { JSDOM } = jsdom;

var _ = require('lodash');
var MergeStream = require('merge-stream');

var srcProject = 'Umbraco.Web.UI.Client\\src\\views';

/*
 * Views task adds the filepath to the root element in each view, then copies to the output folder
 */
gulp.task('views', function () {

    var stream = new MergeStream();

    _.forEach(config.sources.views, group => {

        console.log("copying " + group.files + " to " + config.root + config.targets.views + group.folder);

        stream.add(
            gulp.src(group.files)
            .pipe(modifyFile((content, path, file) => {
                // insert path substring as an attribute on the first element
                var subPath = path.substring(path.indexOf(srcProject) + srcProject.length);
                var dom = new JSDOM(content);

                // if the view contains an element, add the data attribute
                // otherwise return the path as a comment
                var firstChild = dom.window.document.body.firstElementChild;
                if (firstChild) {
                    firstChild.setAttribute('data-filepath', subPath);                    
                    return firstChild.outerHTML;
                } else {
                    return `<!-- filepath: ${subPath} -->\n${content}`;
                }
            }))
            .pipe(gulp.dest(config.root + config.targets.views + group.folder))
        );
    });

    return stream;
});