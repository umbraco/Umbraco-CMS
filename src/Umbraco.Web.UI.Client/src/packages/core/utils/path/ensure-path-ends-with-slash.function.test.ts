import { ensurePathEndsWithSlash } from './ensure-path-ends-with-slash.function.js';
import { expect } from '@open-wc/testing';

describe('ensurePathEndsWithSlash', () => {
	it('appends a slash when one is missing', () => {
		expect(ensurePathEndsWithSlash('foo')).to.equal('foo/');
	});

	it('returns the path unchanged when it already ends with a slash', () => {
		expect(ensurePathEndsWithSlash('foo/')).to.equal('foo/');
	});

	it('returns a single slash for empty input', () => {
		expect(ensurePathEndsWithSlash('')).to.equal('/');
	});

	it('only ensures the trailing slash, not a leading one', () => {
		expect(ensurePathEndsWithSlash('foo')).to.equal('foo/');
		expect(ensurePathEndsWithSlash('/foo')).to.equal('/foo/');
	});
});
