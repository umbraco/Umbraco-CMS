const fs = require('fs');
const io = require("./io");

function createReports() {

    var reports = [];

    // Get all JSON output files
    const reportFiles = io.getFilesFromPath("output", "json");
    for (let i = 0; i < reportFiles.length; i++) {
        const reportFile = reportFiles[i];
        const rawdata = fs.readFileSync('output/' + reportFile);
        const reportJson = JSON.parse(rawdata);

        const fileId = io.getFileId('output/' + reportFile);

        reports.push({
            fileId: fileId.fileId,
            fileDate: fileId.fileDate,
            file: reportFile,
            // we only care about the aggregate info
            report: reportJson.aggregate
        });
    }

    return reports;
}

module.exports = { createReports };
