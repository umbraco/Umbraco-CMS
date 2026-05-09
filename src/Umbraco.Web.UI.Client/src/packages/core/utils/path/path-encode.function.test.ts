import { aliasToPath, encodeFilePath } from './path-encode.function.js';
import { decodeFilePath } from './path-decode.function.js';
import { expect } from '@open-wc/testing';

describe('encodeFilePath', () => {
	it('encodes a dot as %2E (encodeURIComponent leaves dots untouched)', () => {
		expect(encodeFilePath('foo.bar')).to.equal('foo%2Ebar');
	});

	it('encodes a colon as %3A', () => {
		expect(encodeFilePath('a:b')).to.equal('a%3Ab');
	});

	it('encodes spaces as %20', () => {
		expect(encodeFilePath('hello world')).to.equal('hello%20world');
	});

	it('encodes unicode characters', () => {
		expect(encodeFilePath('café')).to.equal('caf%C3%A9');
	});

	it('returns an empty string for empty input', () => {
		expect(encodeFilePath('')).to.equal('');
	});

	it('encodes every dot in a path with several dots', () => {
		expect(encodeFilePath('a.b.c')).to.equal('a%2Eb%2Ec');
	});
});

describe('aliasToPath', () => {
	it('produces the same output as encodeFilePath', () => {
		expect(aliasToPath('foo.bar:baz')).to.equal(encodeFilePath('foo.bar:baz'));
	});
});

describe('encodeFilePath / decodeFilePath round-trip', () => {
	const originals = [
		'plain',
		'my.file:v2',
		'a.b.c',
		'hello world',
		'rød grød/blå bær',
		'',
	];

	originals.forEach((original) => {
		it(`encode → decode is the identity for ${JSON.stringify(original)}`, () => {
			expect(decodeFilePath(encodeFilePath(original))).to.equal(original);
		});
	});

	const encoded = [
		'plain',
		'my%2Efile%3Av2',
		'a%2Eb%2Ec',
		'hello%20world',
		'r%C3%B8d%20gr%C3%B8d%2Fbl%C3%A5%20b%C3%A6r',
		'',
	];

	encoded.forEach((value) => {
		it(`decode → encode is the identity for ${JSON.stringify(value)}`, () => {
			expect(encodeFilePath(decodeFilePath(value))).to.equal(value);
		});
	});
});
