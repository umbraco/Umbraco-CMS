import * as readline from 'readline';
import { execSync } from 'child_process';
import { readdir } from 'fs/promises';

const exampleDirectory = 'examples';
const digitTimeoutMs = 1000;

const getDirectories = async (source) =>
  (await readdir(source, { withFileTypes: true }))
    .filter(dirent => dirent.isDirectory())
    .map(dirent => dirent.name)

function pickInteractive(names) {
	return new Promise((resolve) => {
		const { stdin, stdout } = process;
		let index = 0;
		let digits = '';
		let digitTimer;
		let hasRendered = false;

		const clear = () => {
			if (hasRendered) stdout.write(`\x1b[${names.length + 1}A\x1b[0J`);
		};

		const render = () => {
			const rows = names.map((name, i) => {
				const number = String(i + 1).padStart(2);
				return i === index
					? `\x1b[36m ❯ ${number}  ${name}\x1b[0m`
					: `   ${number}  ${name}`;
			});
			const lines = [`\x1b[36m?\x1b[0m Select an example (↑/↓, number, Enter)`, ...rows];

			// Overwrite the previous frame in place, in a single write, so the list is never blanked between frames.
			const moveUp = hasRendered ? `\x1b[${lines.length}A` : '';
			stdout.write(moveUp + lines.map(line => `${line}\x1b[K\n`).join(''));
			hasRendered = true;
		};

		const finish = (name) => {
			clearTimeout(digitTimer);
			stdin.off('keypress', onKeypress);
			stdin.setRawMode(false);
			stdin.pause();
			clear();
			stdout.write('\x1b[?25h');
			if (name) console.log(`\x1b[32m✔\x1b[0m Example: ${name}`);
			resolve(name);
		};

		const onDigit = (digit) => {
			const next = [digits + digit, digit].find(candidate => {
				const number = parseInt(candidate);
				return number >= 1 && number <= names.length;
			});
			if (!next) return;
			digits = next;
			index = parseInt(next) - 1;
			clearTimeout(digitTimer);
			digitTimer = setTimeout(() => (digits = ''), digitTimeoutMs);
		};

		const onKeypress = (_, key) => {
			if (!key) return;

			if (key.ctrl && key.name === 'c') return finish(undefined);

			switch (key.name) {
				case 'escape':
					return finish(undefined);
				case 'return':
					return finish(names[index]);
				case 'up':
					index = (index - 1 + names.length) % names.length;
					digits = '';
					break;
				case 'down':
					index = (index + 1) % names.length;
					digits = '';
					break;
				case 'home':
					index = 0;
					digits = '';
					break;
				case 'end':
					index = names.length - 1;
					digits = '';
					break;
				default:
					if (/^[0-9]$/.test(key.sequence)) onDigit(key.sequence);
					else return;
			}

			render();
		};

		readline.emitKeypressEvents(stdin);
		stdin.setRawMode(true);
		stdin.resume();
		stdin.on('keypress', onKeypress);
		stdout.write('\x1b[?25l');
		render();
	});
}

function pickByNumber(names) {
	return new Promise((resolve) => {
		const rl = readline.createInterface({
			input: process.stdin,
			output: process.stdout
		});

		console.log('Please select an example by entering the corresponding number:');
		names.forEach((folder, index) => {
			console.log(`[${index + 1}]	${folder}`);
		});

		rl.question('Enter your selection: ', (answer) => {
			// Close readline before starting the dev server so Ctrl+C can terminate the process.
			rl.close();

			const selectedFolder = names[parseInt(answer) - 1];
			if (selectedFolder) console.log(`You selected: ${selectedFolder}`);
			else console.log('No valid example selected.');
			resolve(selectedFolder);
		});
	});
}

function startExample(name) {
	process.env['VITE_EXAMPLE_PATH'] = `${exampleDirectory}/${name}`;

	// Start vite server:
	try {
		execSync('npm run dev', {stdio: 'inherit'});
	} catch (error) {
		// Nothing, cause this is most likely just the server being stopped.
		//console.log(error);
	}
}

async function pickExampleUI(){

	// Find sub folder:
	const exampleFolderNames = await getDirectories(`${exampleDirectory}`);

	// An example name passed on the command line skips the picker:
	const requested = process.argv[2];
	if (requested) {
		if (exampleFolderNames.includes(requested)) {
			startExample(requested);
			return;
		}
		console.log(`No example named "${requested}".`);
	}

	const pick = process.stdin.isTTY ? pickInteractive : pickByNumber;
	const selectedFolder = await pick(exampleFolderNames);

	if (selectedFolder) startExample(selectedFolder);
};

pickExampleUI();
