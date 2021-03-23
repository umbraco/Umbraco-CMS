const crypto = require('crypto')
const fs = require('fs');

function getFileId(file) {
    // create a unique file id for this run
    const fileStats = fs.statSync(file);
    const shasum = crypto.createHash('sha1');
    shasum.update(file + fileStats.birthtimeMs);
    const fileId = shasum.digest('hex');
    return { fileId: fileId, fileDate: fileStats.birthtimeMs };
}

function getFilesFromPath(path, extension) {
    let files = fs.readdirSync(path);
    return files.filter(file => file.match(new RegExp(`.*\.(${extension})`, 'ig')));
}

module.exports = {
    getFilesFromPath,
    getFileId
};
