import { readFileSync, readdirSync, statSync } from 'fs';
import { join } from 'path';

const distDir = 'dist-cms';
const sharedDir = join(distDir, 'external', 'tiptap');

// Matches a sourcemap `sources` entry for `@tiptap/pm` itself or any of the underlying
// `prosemirror-*` packages it re-exports (see devops/importmap/tiptap.js).
const PROSEMIRROR_SOURCE = /node_modules\/(@tiptap\/pm|prosemirror-[a-z-]+)\//;

// Sourcemap `sources` entries record each bundled chunk's *original* file paths, so — unlike
// grepping the emitted code for a symbol name — this is immune to Rollup renaming/deconflicting
// identifiers when it merges modules into a shared chunk (verified while building this check: a
// legitimately single-copied class can still show its name in more than one emitted file, once in
// the chunk that defines it and once in a re-export shim that merely aliases it).
const walkFiles = (dir, suffix) => {
	const files = [];
	for (const entry of readdirSync(dir)) {
		const fullPath = join(dir, entry);
		if (statSync(fullPath).isDirectory()) {
			files.push(...walkFiles(fullPath, suffix));
		} else if (fullPath.endsWith(suffix)) {
			files.push(fullPath);
		}
	}
	return files;
};

const mapFiles = walkFiles(distDir, '.js.map');
const errors = [];
let sawProsemirrorSource = false;

for (const mapFile of mapFiles) {
	const sources = JSON.parse(readFileSync(mapFile, 'utf8')).sources ?? [];
	if (!sources.some((source) => PROSEMIRROR_SOURCE.test(source))) {
		continue;
	}

	sawProsemirrorSource = true;
	const jsFile = mapFile.replace(/\.map$/, '');
	if (!jsFile.startsWith(sharedDir)) {
		errors.push(`"${jsFile}" bundles ProseMirror source directly, expected it only under "${sharedDir}".`);
	}
}

if (!sawProsemirrorSource) {
	errors.push(`No emitted chunk under "${distDir}" bundles any ProseMirror source. Did the build run?`);
}

// Positive-signal sanity check: the RTE package must actually consume the shared modules, not
// merely happen to avoid bundling its own copy for some unrelated reason.
const tiptapPackageDir = join(distDir, 'packages', 'tiptap');
const tiptapJsFiles = walkFiles(tiptapPackageDir, '.js');
const importsSharedPm = tiptapJsFiles.some((file) => readFileSync(file, 'utf8').includes('@tiptap/pm/'));
if (!importsSharedPm) {
	errors.push(
		`No file under "${tiptapPackageDir}" imports from "@tiptap/pm/" — expected the RTE package to consume the shared ProseMirror modules rather than its own bundled copy.`,
	);
}

if (errors.length > 0) {
	console.error(`--- ProseMirror single-copy check failed ---\n\n${errors.join('\n\n')}`);
	process.exit(1);
}

console.log(`--- ProseMirror single-copy check passed: one shared copy, under "${sharedDir}". ---`);
