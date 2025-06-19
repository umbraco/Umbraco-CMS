import { writeFile, mkdir } from 'fs';
import * as pathModule from 'path';

const path = pathModule.default;
const getDirName = path.dirname;

export const writeFileWithDir = (path, contents, cb) => {
	mkdir(getDirName(path), { recursive: true }, function (err) {
		if (err) return cb(err);

		writeFile(path, contents, cb);
	});
};
