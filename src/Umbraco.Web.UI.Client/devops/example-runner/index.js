import * as readline from 'node:readline';
import { execSync } from 'node:child_process';
import { readdir } from 'node:fs/promises';

const exampleDirectory = 'examples';
const digitTimeoutMs = 1000;

const getDirectories = async (source) =>
	(await readdir(source, { withFileTypes: true }))
		.filter((dirent) => dirent.isDirectory())
		.map((dirent) => dirent.name);

const filterNames = (names, query) => {
	const text = query.toLowerCase();
	return names.filter((name) => name.toLowerCase().includes(text));
};

const dim = (text) => `\x1b[2m${text}\x1b[0m`;
const cyan = (text) => `\x1b[36m${text}\x1b[0m`;
const hiddenRows = (arrow, count) => (count ? dim(`  ${arrow} ${count} more`) : '');

const logo = [
	'  ▄▄████▄▄  ',
	' ██████████ ',
	'███ ████ ███',
	'███ ████ ███',
	' ██▄▄▄▄▄▄██ ',
	'  ▀▀████▀▀  '
];
const title = ['Umbraco', 'Backoffice Client Examples'];
const titleRow = 2;
const banner = [
	...logo.map((line, i) => {
		const text = title[i - titleRow];
		return text ? `${line}   \x1b[1m${text}\x1b[0m` : line;
	}),
	'',
];

/**
 * Lets the user pick an example with the keyboard, filtering the list by typing.
 * @param {string[]} names The example folder names.
 * @returns {Promise<string | undefined>} The selected folder name, or undefined if cancelled.
 */
function pickInteractive(names) {
	return new Promise((resolve) => {
		const { stdin, stdout } = process;
		let filter = '';
		let matches = names;
		let index = 0;
		let top = 0;
		let digits = '';
		let digitTimer;
		let renderedHeight = 0;
		let done = false;

		// Banner, header, two indicator lines and one spare row, so the frame never scrolls the terminal.
		const viewportHeight = () => Math.max(1, Math.min(names.length, (stdout.rows ?? 24) - 4 - banner.length));

		const clear = () => {
			if (renderedHeight) stdout.write(`\x1b[${renderedHeight}A\x1b[0J`);
		};

		const render = () => {
			const height = viewportHeight();
			if (index < top) top = index;
			if (index >= top + height) top = index - height + 1;
			top = Math.max(0, Math.min(top, matches.length - height));

			const rows = Array.from({ length: height }, (_, i) => {
				const name = matches[top + i];
				if (!name) return i === 0 && !matches.length ? dim('  No matches') : '';
				const number = String(top + i + 1).padStart(2);
				return top + i === index ? cyan(` ❯ ${number}  ${name}`) : `   ${number}  ${name}`;
			});

			const header = filter
				? `${cyan('?')} Select an example: ${filter}`
				: `${cyan('?')} Select an example (↑/↓, number, type to filter, Enter)`;
			const lines = [
				...banner,
				header,
				hiddenRows('↑', top),
				...rows,
				hiddenRows('↓', Math.max(0, matches.length - top - height)),
			];

			// Overwrite the previous frame in place, in a single write, so the list is never blanked between frames.
			let moveUp = '';
			if (renderedHeight) {
				const clearBelow = lines.length === renderedHeight ? '' : '\x1b[0J';
				moveUp = `\x1b[${renderedHeight}A${clearBelow}`;
			}
			stdout.write(moveUp + lines.map((line) => `${line}\x1b[K\n`).join(''));
			renderedHeight = lines.length;
		};

		const finish = (name) => {
			done = true;
			clearTimeout(digitTimer);
			stdin.off('keypress', onKeypress);
			stdout.off('resize', render);
			stdin.setRawMode(false);
			stdin.pause();
			clear();
			stdout.write('\x1b[?25h');
			if (name) console.log(`\x1b[32m✔\x1b[0m Example: ${name}`);
			resolve(name);
		};

		const move = (to) => {
			if (!matches.length) return;
			index = to;
			digits = '';
		};

		const setFilter = (value) => {
			filter = value;
			matches = filterNames(names, value);
			index = 0;
			top = 0;
			digits = '';
		};

		const onDigit = (digit) => {
			const next = [digits + digit, digit].find((candidate) => {
				const number = Number.parseInt(candidate);
				return number >= 1 && number <= matches.length;
			});
			if (!next) return;
			digits = next;
			index = Number.parseInt(next) - 1;
			clearTimeout(digitTimer);
			digitTimer = setTimeout(() => (digits = ''), digitTimeoutMs);
		};

		const keyActions = {
			up: () => move((index - 1 + matches.length) % matches.length),
			down: () => move((index + 1) % matches.length),
			home: () => move(0),
			end: () => move(matches.length - 1),
			backspace: () => setFilter(filter.slice(0, -1)),
			escape: () => (filter ? setFilter('') : finish(undefined)),
			return: () => matches[index] && finish(matches[index]),
		};

		const onKeypress = (_, key) => {
			if (!key) return;

			if (key.ctrl && key.name === 'c') return finish(undefined);

			const action = keyActions[key.name];
			if (action) action();
			else if (/^\d$/.test(key.sequence)) onDigit(key.sequence);
			else if (/^[a-z_-]$/i.test(key.sequence)) setFilter(filter + key.sequence);
			else return;

			if (!done) render();
		};

		readline.emitKeypressEvents(stdin);
		stdin.setRawMode(true);
		stdin.resume();
		stdin.on('keypress', onKeypress);
		stdout.on('resize', render);
		stdout.write('\x1b[?25l');
		render();
	});
}

/**
 * Lets the user pick an example by entering its number, for when stdin is not a terminal.
 * @param {string[]} names The example folder names.
 * @returns {Promise<string | undefined>} The selected folder name, or undefined if the number is not valid.
 */
function pickByNumber(names) {
	return new Promise((resolve) => {
		const rl = readline.createInterface({
			input: process.stdin,
			output: process.stdout,
		});

		console.log('Please select an example by entering the corresponding number:');
		names.forEach((folder, index) => {
			console.log(`[${index + 1}]	${folder}`);
		});

		rl.question('Enter your selection: ', (answer) => {
			// Close readline before starting the dev server so Ctrl+C can terminate the process.
			rl.close();

			const selectedFolder = names[Number.parseInt(answer) - 1];
			if (selectedFolder) console.log(`You selected: ${selectedFolder}`);
			else console.log('No valid example selected.');
			resolve(selectedFolder);
		});
	});
}

/**
 * Starts the Vite dev server for an example, and returns once the server has stopped.
 * @param {string} name The example folder name.
 */
function startExample(name) {
	process.env['VITE_EXAMPLE_PATH'] = `${exampleDirectory}/${name}`;

	// Start vite server:
	try {
		execSync('npm run dev', { stdio: 'inherit' });
	} catch {
		// Nothing, cause this is most likely just the server being stopped.
	}
}

/**
 * Starts the example named on the command line, or otherwise asks the user to pick one.
 */
async function pickExampleUI() {
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
}

await pickExampleUI();
