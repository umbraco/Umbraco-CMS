const csv = require('csv-parser')
const fs = require('fs');
const io = require("./io");

function isNumeric(str) {
    if (typeof str !== "string") {
        return false // we only process strings!
    }

    let notANumber = isNaN(str);
    if (notANumber) {
        // use type coercion to parse the _entirety_ of the string (`parseFloat` alone does not do this)...
        return false;
    }
    let parsedNumber = parseFloat(str);
    if (isNaN(parsedNumber)) {
        // ...and ensure strings of whitespace fail
        return false;
    }
    if (parsedNumber <= 0) {
        // don't include zeros (like CPU count will be when starting a calculation)
        return false;
    }

    return true;
}

async function createReports(perfReportType) {

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

        if (perfReportType == "windows") {

            // Windows perf reports are structured with columns that represent
            // each measurement so we need to read them accordingly.
            // We extract the headers from these CSVs based on their names and those
            // Headers are used for creating the report.

            // we don't really use this but store each mapped csv value (for debugging)
            const results = [];
            // Keep the sum of each metric
            const sums = [];
            // Count how many of each metric
            const counts = [];
            // The name for each metric
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

                    // Create a report like:
                    /*
                    {
                        "# bytes in all heaps": Average_Of_Bytes_In_All_Heaps,
                        "large object heap size": Average_Of_LOH
                    }
                    */
                    let averages = {};
                    for (let s = 0; s < sums.length; s++) {
                        if (counts[s] !== 0) {
                            averages[headers[s]] = sums[s] / counts[s];
                        }
                    }

                    console.log("Created windows perf report for " + reportFile + ": ");
                    console.log(averages);

                    const fileId = io.getFileId('output/' + reportFile);

                    // we're done, resolve the report
                    promiseResolvers[index]({
                        fileId: fileId.fileId,
                        fileDate: fileId.fileDate,
                        file: reportFile,
                        report: averages
                    });
                });
        }
        else {
            // Dotnet perf reports are structured with rows that represent
            // each measurement so we need to read them accordingly.
            // We need to hard code the headers here since they are slightly different from Windows
            // which are extracted from the CSV but we need to keep them consistent.

            const valueMap = {
                "LOH Size (B)": {
                    "header": 'large object heap size',
                    "sum": 0,
                    "count": 0,
                    "managedMem": true
                },
                "Gen 2 Size (B)": {
                    "header": 'gen 2 heap size',
                    "sum": 0,
                    "count": 0,
                    "managedMem": true
                },
                "Gen 1 Size (B)": {
                    "header": 'gen 1 heap size',
                    "sum": 0,
                    "count": 0,
                    "managedMem": true
                },
                "Working Set (MB)": {
                    "header": 'working set',
                    "sum": 0,
                    "count": 0
                },
                "CPU Usage (%)": {
                    "header": '% processor time',
                    "sum": 0,
                    "count": 0
                },
                "GC Heap Size (MB)": {
                    "header": '# total committed bytes',
                    "sum": 0,
                    "count": 0
                }
            };

            // we don't really use this but store each mapped csv value (for debugging)
            const results = [];

            fs.createReadStream('output/' + reportFile)
                .pipe(csv({
                    mapHeaders: ({ header, index }) => {
                        // we only keep the 3rd (Counter Name) and 5th columns (Mean/Increment)
                        if (index !== 2 && index !== 4) {
                            return null;
                        }
                        return header;
                    }
                }))
                .on('data', (data) => {

                    results.push(data);

                    let metric = data["Counter Name"];
                    let mean = data["Mean/Increment"];

                    // if it's not a number we won't count it in the results
                    if (isNumeric(mean)) {

                        let metricValues = valueMap[metric];
                        if (!metricValues) {
                            throw "No mapped values found for metric " + metric;
                        }

                        metricValues.sum += Number(mean);
                        metricValues.count++;
                    }
                })
                .on('end', () => {

                    // console.log(valueMap);

                    const bytesInAllHeapsHeader = '# bytes in all heaps';
                    let averages = {};
                    // We need to manually calculate this since it's not included in dotnet-counters
                    // by default but it is the sum of 3 other metrics Gen1,2,LOH
                    averages[bytesInAllHeapsHeader] = 0;

                    for (const [key, value] of Object.entries(valueMap)) {

                        // We have to convert anything that is MB to B so that its consistent
                        // with the windows counters.
                        let metricValue = 0;
                        if (key.indexOf("(MB)") !== -1) {
                            metricValue = (value.sum * (1024 * 1024)) / value.count;
                        }
                        else {
                            metricValue = value.sum / value.count;
                        }

                        // This will be NaN if there is no metric (i.e. 0)
                        // so don't include it.
                        if (!isNaN(metricValue)) {
                            averages[value.header] = metricValue;
                        }

                        // if this metric is used to calc bytes in all heaps
                        if (value.managedMem) {
                            // bytes in all heaps is the sum of 3 other metrics
                            // so sum the averages of each
                            averages[bytesInAllHeapsHeader] += averages[value.header]
                        }
                    }

                    console.log("Created dotnet perf report for " + reportFile + ": ");
                    console.log(averages);

                    const fileId = io.getFileId('output/' + reportFile);

                    // we're done, resolve the report
                    promiseResolvers[index]({
                        fileId: fileId.fileId,
                        fileDate: fileId.fileDate,
                        file: reportFile,
                        report: averages
                    });
                });
        }


    });

    return Promise.all(promises);
}

module.exports = { createReports };
