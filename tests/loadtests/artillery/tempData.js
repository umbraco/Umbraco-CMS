const fs = require('fs');

module.exports = {
    getTempData,
    saveTempData
};

const tempDataPath = "output/run.tmp";

function saveTempData(t) {
    let tempData = getTempData();
    tempData = Object.assign(tempData, t);
    fs.writeFileSync(tempDataPath, JSON.stringify(tempData, null, 2));
}

function getTempData() {
    if (fs.existsSync(tempDataPath)) {
        // we use a persisted file to store/share information between
        // this artillery process (node) and the parent powershell process.
        const fileData = fs.readFileSync(tempDataPath);
        return JSON.parse(fileData);
    }
    else {
        return {};
    }
}
