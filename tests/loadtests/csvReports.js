const csv = require('csv-parser')
const fs = require('fs');
const io = require("./io");

function isNumeric(str) {
    if (typeof str != "string") return false // we only process strings!
    return !isNaN(str) && // use type coercion to parse the _entirety_ of the string (`parseFloat` alone does not do this)...
        !isNaN(parseFloat(str)) // ...and ensure strings of whitespace fail
}

async function createReports() {

    // Get all CSV output files
    let reportFiles = io.getFilesFromPath("output", "csv");

    // Since we are going to stream the file contents we need to
    // create a promise for each and resolve each when we're done.
    const promises = [];
    const promiseResolvers = [];
    for (var i = 0; i < reportFiles.length; i++) {
        promises.push(new Promise((resolve, reject) => {
            promiseResolvers.push(resolve);
        }));
    }

    reportFiles.forEach((reportFile, index) => {

        const results = [];
        const sums = [];
        const counts = [];
        const headers = [];

        fs.createReadStream('output/' + reportFile)
            .pipe(csv({
                mapHeaders: ({ header, index }) => {
                    // ignore the first column
                    if (index === 0) {
                        return null;
                    }

                    sums.push(0);
                    counts.push(0);
                    const parts = header.split("\\");
                    const h = parts[parts.length - 1].toLowerCase();
                    headers.push(h);
                    return h;
                },
                mapValues: ({ header, index, value }) => {
                    if (index !== 0) {
                        // if it's not a number we won't count it in the results
                        if (isNumeric(value)) {
                            var i = index - 1; // because we've ignored the first column
                            sums[i] += Number(value);
                            counts[i]++;
                        }
                    }
                    return value;
                }
            }))
            .on('data', (data) => {
                results.push(data);
            })
            .on('end', () => {

                // console.log(sums);
                // console.log(counts);

                let averages = {};
                for (let s = 0; s < sums.length; s++) {
                    if (counts[s] !== 0) {
                        averages[headers[s]] = sums[s] / counts[s];
                    }
                }

                const fileId = io.getFileId('output/' + reportFile);

                // we're done, resolve the report
                promiseResolvers[index]({
                    fileId: fileId.fileId,
                    fileDate: fileId.fileDate,
                    file: reportFile,
                    report: averages
                });
            });
    });

    return Promise.all(promises);
}

module.exports = { createReports };
