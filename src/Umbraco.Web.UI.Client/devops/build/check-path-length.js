import { readdirSync, statSync } from 'fs';
import { join } from 'path';

const PROJECT_DIR = process.argv[2] ?? '.';
const MAX_PATH_LENGTH = process.argv[3] ?? 140;
const IS_CI = process.env.CI === 'true';
const IS_AZURE_PIPELINES = process.env.TF_BUILD === 'true';
const IS_GITHUB_ACTIONS = process.env.GITHUB_ACTIONS === 'true';
const FILE_PATH_COLOR = '\x1b[36m%s\x1b[0m';
const ERROR_COLOR = '\x1b[31m%s\x1b[0m';
const SUCCESS_COLOR = '\x1b[32m%s\x1b[0m';
const processExitCode = 1; // Default to 1 to fail the build, 0 to just log the issues

console.log(`Checking path length in ${PROJECT_DIR} for paths exceeding ${MAX_PATH_LENGTH}...`);
console.log('CI detected:', IS_CI);

console.log('\n-----------------------------------');
console.log('Results:');
console.log('-----------------------------------\n');

const hasError = checkPathLength(PROJECT_DIR);

if (hasError) {
	console.log('\n-----------------------------------');
	console.log(ERROR_COLOR, 'Path length check failed');
	console.log('-----------------------------------\n');
	if (IS_CI && processExitCode) {
		process.exit(processExitCode);
	}
} else {
	console.log('\n-----------------------------------');
	console.log(SUCCESS_COLOR, 'Path length check passed');
	console.log('-----------------------------------\n');
}

// Functions

/**
 * Recursively check the path length of all files in a directory.
 * @param {string} dir - The directory to check for path lengths
 * @returns {boolean}
 */
function checkPathLength(dir) {
	const files = readdirSync(dir);
	let hasError = false;

	files.forEach(file => {
		const filePath = join(dir, file);
		if (filePath.length > MAX_PATH_LENGTH) {
			hasError = true;

			if (IS_AZURE_PIPELINES) {
				console.error(`##vso[task.logissue type=error;sourcepath=${mapFileToSourcePath(filePath)};]Path exceeds maximum length of ${MAX_PATH_LENGTH} characters: ${filePath} with ${filePath.length} characters`);
			} else if (IS_GITHUB_ACTIONS) {
				console.error(`::error file=${mapFileToSourcePath(filePath)},title=Path exceeds ${MAX_PATH_LENGTH} characters::Paths should not be longer than ${MAX_PATH_LENGTH} characters to support WIN32 systems. The file ${filePath} exceeds that with ${filePath.length - MAX_PATH_LENGTH} characters.`);
			} else {
				console.error(FILE_PATH_COLOR, mapFileToSourcePath(filePath), '(exceeds by', filePath.length - MAX_PATH_LENGTH, 'chars)');
			}
		}

		if (statSync(filePath).isDirectory()) {
			const subHasError = checkPathLength(filePath);
			if (subHasError) {
				hasError = true;
			}
		}
	});

	return hasError;
}

/**
 * Maps a file path to a source path for CI logs.
 * @remark This might not always work as expected, especially on bundled files, but it's a best effort to map the file path to a source path.
 * @param {string} file - The file path to map to a source path
 * @returns {string}
 */
function mapFileToSourcePath(file) {
	return file.replace(PROJECT_DIR, 'src').replace('.js', '.ts');
}
