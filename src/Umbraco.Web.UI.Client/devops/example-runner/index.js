import * as globModule from 'tiny-glob';
import * as readline from 'readline';
import { readdir } from 'fs/promises'

const exampleDirectory = 'examples';

const getDirectories = async (source) =>
  (await readdir(source, { withFileTypes: true }))
    .filter(dirent => dirent.isDirectory())
    .map(dirent => dirent.name)

async function pickExampleUI(){

	// Find sub folder:
	const exampleFolderNames = await getDirectories(`${exampleDirectory}`);

	const rl = readline.createInterface({
		input: process.stdin,
		output: process.stdout
	});

	console.log('Please select a folder by entering the corresponding number:');
	exampleFolderNames.forEach((folder, index) => {
		console.log(`${index + 1}. ${folder}`);
	});

	rl.question('Enter your selection: ', (answer) => {
		const selectedFolder = exampleFolderNames[parseInt(answer) - 1];
		console.log(`You selected: ${selectedFolder}`);
		rl.close();
	});

};

pickExampleUI();
