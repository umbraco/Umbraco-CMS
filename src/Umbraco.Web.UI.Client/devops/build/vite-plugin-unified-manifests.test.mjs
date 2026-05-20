import { test } from 'node:test';
import assert from 'node:assert/strict';
import { buildEntrySource } from './vite-plugin-unified-manifests.ts';

test('emits import + spread for each package', () => {
	const src = buildEntrySource([
		{ name: 'block', path: '../../src/packages/block/manifests.ts' },
		{ name: 'documents', path: '../../src/packages/documents/manifests.ts' },
	]);
	assert.match(src, /import \{ manifests as block \} from "\.\.\/\.\.\/src\/packages\/block\/manifests\.ts"/);
	assert.match(src, /import \{ manifests as documents \} from "\.\.\/\.\.\/src\/packages\/documents\/manifests\.ts"/);
	assert.match(src, /export const allManifests = \[\s*\.\.\.block,\s*\.\.\.documents,\s*\]/);
});

test('sanitises package names with hyphens into valid identifiers', () => {
	const src = buildEntrySource([{ name: 'code-editor', path: '/abs/code-editor/manifests.ts' }]);
	assert.match(src, /import \{ manifests as codeEditor \} from /);
	assert.match(src, /\.\.\.codeEditor/);
});

test('returns empty allManifests for empty input', () => {
	const src = buildEntrySource([]);
	assert.match(src, /export const allManifests = \[\s*\]/);
});

test('produces deterministic output (stable order)', () => {
	const a = buildEntrySource([
		{ name: 'a', path: '/a' },
		{ name: 'b', path: '/b' },
	]);
	const b = buildEntrySource([
		{ name: 'a', path: '/a' },
		{ name: 'b', path: '/b' },
	]);
	assert.equal(a, b);
});

test('encodes Windows-style backslash paths so Rollup sees them as plain strings', () => {
	const src = buildEntrySource([{ name: 'block', path: 'D:\\a\\1\\s\\block\\manifests.ts' }]);
	// JSON.stringify escapes each backslash: D:\a\1\s\block\manifests.ts → "D:\\a\\1\\s\\block\\manifests.ts"
	assert.ok(src.includes('from "D:\\\\a\\\\1\\\\s\\\\block\\\\manifests.ts"'));
	assert.ok(!src.includes('"D:\\a\\1'));
});
