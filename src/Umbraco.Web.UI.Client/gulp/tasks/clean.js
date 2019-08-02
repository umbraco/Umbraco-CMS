'use strict';

var config = require('../config');
var gulp = require('gulp');

var fs = require('fs');
var glob = require('glob');
var path = require('path');

const cleanEmptyFoldersRecursively = folder => {

    if (!fs.statSync(folder).isDirectory()) return;

    let files = fs.readdirSync(folder);

    if (files.length > 0) {
        files.forEach(file => cleanEmptyFoldersRecursively(path.join(folder, file)));
        files = fs.readdirSync(folder);
    }

    if (files.length === 0) {
        fs.rmdirSync(folder);
        return;
    }
};

/*
 * Delete all unnecessary files, where the template has been embedded in the respective js file
 */
gulp.task('clean.views', async () => {

    const path = config.root + config.targets.views;
    const js = config.root + config.targets.js;
    
    // get all the filepaths referenced in the js, these will only appear in an inline template, therefore the physical file is not required
    // data-filepath attribute is added in the views task
    const regex = /data-filepath="(.*?)"/gi; 

    await new Promise((resolve, reject) => {
        glob(js + '**/*.js', (err, files) => {

            let viewsToClean = [];

            files.forEach(file => {
                const contents = fs.readFileSync(file, 'utf8');
                let m;

                while ((m = regex.exec(contents)) !== null) {
                    // because the path is escaped in the directive, it's doubly escaped by the regex.
                    if (m[1]) {
                        viewsToClean.push(m[1].replace(/\\{2,}/g, '\\').substring(1));
                    }
                }
            });

            viewsToClean.forEach(v => fs.unlinkSync(path + v));            

        }).on('end', resolve);
    });
});


/*
 * Delete all empty directories
 */
gulp.task('clean.dirs', () => cleanEmptyFoldersRecursively(config.root + config.targets.views));
