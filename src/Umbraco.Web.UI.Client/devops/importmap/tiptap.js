import { readFileSync } from 'fs';

const tiptapPmPackageJson = JSON.parse(readFileSync('node_modules/@tiptap/pm/package.json', 'utf8'));

// The `@tiptap/pm/*` subpaths, taken from its own `exports` map.
export const tiptapPmSubpaths = Object.keys(tiptapPmPackageJson.exports).map((key) => key.replace(/^\.\//, ''));

// The underlying `prosemirror-*` packages, taken from `@tiptap/pm`'s own `dependencies`. Each
// `@tiptap/pm/<subpath>` module re-exports the correspondingly-named `prosemirror-<subpath>`
// package, so the two lists above are always the same length and share the same subpath names.
export const prosemirrorPackageNames = Object.keys(tiptapPmPackageJson.dependencies || {});

// Derives the bare-specifier importmap entries for Tiptap/ProseMirror, so third-party Tiptap
// extensions (which import `@tiptap/core`/`@tiptap/pm/*`/`prosemirror-*` directly, not through
// `@umbraco-cms/backoffice/*`) resolve to the same module instances the backoffice RTE itself
// runs on. See devops/importmap/check-tiptap-parity.js, which asserts these derived lists stay in
// sync with the checked-in entry files in src/external/tiptap/pm/, that package's declared
// `prosemirror-*` dependencies, and the ESLint exceptions for these specifiers.
export const createTiptapImportMap = (rootDir) => {
	const base = `${rootDir}/external/tiptap`;

	const imports = {
		'@tiptap/core': `${base}/index.js`,
		'@tiptap/pm/': `${base}/pm/`,
	};

	for (const packageName of prosemirrorPackageNames) {
		const subpath = packageName.replace(/^prosemirror-/, '');
		imports[packageName] = `${base}/pm/${subpath}.js`;
	}

	return imports;
};
