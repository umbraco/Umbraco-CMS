import { readdirSync, statSync } from 'fs';
import { join } from 'path';

const PROJECT_DIR = process.argv[2] ?? '.';
const MAX_PATH_LENGTH = process.argv[3] ?? 140;
const IS_CI = process.env.CI === 'true';
const IS_AZURE_PIPELINES = process.env.TF_BUILD === 'true';
const IS_GITHUB_ACTIONS = process.env.GITHUB_ACTIONS === 'true';
const FILE_PATH_COLOR = '\x1b[36m%s\x1b[0m';

console.log(`Checking path length in ${PROJECT_DIR} for paths exceeding ${MAX_PATH_LENGTH}...`);
console.log('CI detected:', IS_CI);

console.log('\n-----------------------------------');
console.log('Results:');
console.log('-----------------------------------\n');

function checkPathLength(dir) {
	const files = readdirSync(dir);

	files.forEach(file => {
		const filePath = join(dir, file);
		if (filePath.length > MAX_PATH_LENGTH) {

			if (IS_CI) {
				//process.exitCode = 1; // TODO: Uncomment this line to fail the build
			}

			if (IS_AZURE_PIPELINES) {
				console.error(`##vso[task.logissue type=warning;sourcepath=${filePath};]Path exceeds maximum length of ${MAX_PATH_LENGTH} characters: ${filePath} with ${filePath.length} characters`);
			} else if (IS_GITHUB_ACTIONS) {
				console.error(`::warning file=${filePath},title=Path exceeds ${MAX_PATH_LENGTH} characters::Paths should not be longer than ${MAX_PATH_LENGTH} characters to support WIN32 systems. The file ${filePath} exceeds that with ${filePath.length - MAX_PATH_LENGTH} characters.`);
			} else {
				console.error(`Path exceeds maximum length of ${MAX_PATH_LENGTH} characters: ${FILE_PATH_COLOR}`, filePath, filePath.length - MAX_PATH_LENGTH);
			}
		}

		if (statSync(filePath).isDirectory()) {
			checkPathLength(filePath, MAX_PATH_LENGTH);
		}
	});
}

checkPathLength(PROJECT_DIR, MAX_PATH_LENGTH);
