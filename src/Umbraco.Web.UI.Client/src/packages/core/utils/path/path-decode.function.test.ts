import { decodeFilePath } from './path-decode.function.js';
import { expect } from '@open-wc/testing';

describe('decodeFilePath', () => {
	it('decodes %2E back to a dot', () => {
		expect(decodeFilePath('foo%2Ebar')).to.equal('foo.bar');
	});

	it('decodes %3A back to a colon', () => {
		expect(decodeFilePath('a%3Ab')).to.equal('a:b');
	});

	it('decodes %20 to a space', () => {
		expect(decodeFilePath('hello%20world')).to.equal('hello world');
	});

	it('decodes unicode percent-encoded sequences', () => {
		expect(decodeFilePath('caf%C3%A9')).to.equal('café');
	});

	it('returns an empty string for empty input', () => {
		expect(decodeFilePath('')).to.equal('');
	});

	it('passes already-decoded ASCII through unchanged', () => {
		expect(decodeFilePath('plain-text')).to.equal('plain-text');
	});
});
