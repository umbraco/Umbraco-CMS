import { readFileSync, readdirSync } from 'fs';
import { tiptapPmSubpaths, prosemirrorPackageNames } from './tiptap.js';

// Everything the backoffice promises for Tiptap/ProseMirror (see #23703) traces back to
// `@tiptap/pm`'s own `package.json`. This asserts the four places that promise depends on stay in
// sync with it, so a Tiptap/ProseMirror version bump fails loudly here instead of 404ing at
// runtime for a package developer.
const errors = [];

// 1. The checked-in entry files in src/external/tiptap/pm/ must match `@tiptap/pm`'s own subpaths
// exactly — a subpath with no entry file is a bare specifier the importmap maps to a file that
// doesn't exist.
const entryFileDir = 'src/external/tiptap/pm';
const entryFileSubpaths = readdirSync(entryFileDir)
	.filter((file) => file.endsWith('.ts'))
	.map((file) => file.replace(/\.ts$/, ''))
	.sort();
const expectedSubpaths = [...tiptapPmSubpaths].sort();

if (JSON.stringify(entryFileSubpaths) !== JSON.stringify(expectedSubpaths)) {
	errors.push(
		`"${entryFileDir}" does not match @tiptap/pm's subpath exports.\n` +
			`  Entry files: ${entryFileSubpaths.join(', ')}\n` +
			`  @tiptap/pm exports: ${expectedSubpaths.join(', ')}`,
	);
}

// 2. Every `prosemirror-*` dependency of `@tiptap/pm` must have a matching subpath (same name,
// minus the `prosemirror-` prefix) — this is what lets the importmap point a `prosemirror-*` bare
// specifier at the same file as its `@tiptap/pm/*` counterpart.
for (const packageName of prosemirrorPackageNames) {
	const subpath = packageName.replace(/^prosemirror-/, '');
	if (!tiptapPmSubpaths.includes(subpath)) {
		errors.push(`@tiptap/pm depends on "${packageName}" but has no matching "./${subpath}" export subpath.`);
	}
}

// 3. The `prosemirror-*` dependency ranges declared in src/external/tiptap/package.json (which
// become published peerDependencies) must match the ranges `@tiptap/pm` itself declares — a stale
// range would silently misrepresent what's actually shipped.
const tiptapPmPackageJson = JSON.parse(readFileSync('node_modules/@tiptap/pm/package.json', 'utf8'));
const externalTiptapPackageJson = JSON.parse(readFileSync('src/external/tiptap/package.json', 'utf8'));

for (const packageName of prosemirrorPackageNames) {
	const expectedRange = tiptapPmPackageJson.dependencies[packageName];
	const declaredRange = externalTiptapPackageJson.dependencies?.[packageName];

	if (declaredRange !== expectedRange) {
		errors.push(
			`src/external/tiptap/package.json declares "${packageName}": "${declaredRange ?? '(missing)'}", ` +
				`but @tiptap/pm depends on "${expectedRange}".`,
		);
	}
}

// 4. The ESLint `enforce-umbraco-external-imports` exceptions must still cover the bare specifier
// shapes the importmap serves — otherwise the rule's autofixer rewrites a correct bare import into
// a nonexistent `@umbraco-cms/backoffice/external/...` specifier.
const eslintConfig = readFileSync('eslint.config.js', 'utf8');
for (const exception of ["'@tiptap/core'", "'@tiptap/pm/'", "'prosemirror-'"]) {
	if (!eslintConfig.includes(exception)) {
		errors.push(`eslint.config.js is missing the ${exception} exception for "enforce-umbraco-external-imports".`);
	}
}

if (errors.length > 0) {
	console.error(`--- Tiptap/ProseMirror parity check failed ---\n\n${errors.join('\n\n')}`);
	process.exit(1);
}

console.log('--- Tiptap/ProseMirror parity check passed. ---');
