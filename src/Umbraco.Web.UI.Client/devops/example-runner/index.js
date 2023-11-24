import * as readline from 'readline';
import { execSync } from 'child_process';
import { readdir } from 'fs/promises';

const exampleDirectory = 'examples';

const getDirectories = async (source) =>
  (await readdir(source, { withFileTypes: true }))
    .filter(dirent => dirent.isDirectory())
    .map(dirent => dirent.name)

async function pickExampleUI(){

	// Find sub folder:
	const exampleFolderNames = await getDirectories(`${exampleDirectory}`);

	// Create UI:
	const rl = readline.createInterface({
		input: process.stdin,
		output: process.stdout
	});

	// List examples:
	console.log('Please select an example by entering the corresponding number:');
	exampleFolderNames.forEach((folder, index) => {
		console.log(`[${index + 1}]	${folder}`);
	});

	// Ask user to select an example:
	rl.question('Enter your selection: ', (answer) => {

		// User picked an example:
		const selectedFolder = exampleFolderNames[parseInt(answer) - 1];
		console.log(`You selected: ${selectedFolder}`);

		process.env['VITE_EXAMPLE_PATH'] = `${exampleDirectory}/${selectedFolder}`;

		// Start vite server:
		try {
			execSync('npm run dev', {stdio: 'inherit'});
		} catch (error) {
			// Nothing, cause this is most likely just the server begin stopped.
			//console.log(error);
		}
	});

};

pickExampleUI();
