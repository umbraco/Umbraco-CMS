import { transformServerPathToClientPath } from './transform-server-path-to-client-path.function.js';
import { expect } from '@open-wc/testing';

describe('transformServerPathToClientPath', () => {
	it('strips a leading ~ so ~/foo becomes /foo', () => {
		expect(transformServerPathToClientPath('~/foo')).to.equal('/foo');
	});

	it('strips a leading /wwwroot so /wwwroot/foo becomes /foo', () => {
		expect(transformServerPathToClientPath('/wwwroot/foo')).to.equal('/foo');
	});

	it('chains the two transforms when both prefixes are present', () => {
		expect(transformServerPathToClientPath('~/wwwroot/foo')).to.equal('/foo');
	});

	it('returns a path without either prefix unchanged', () => {
		expect(transformServerPathToClientPath('/foo/bar')).to.equal('/foo/bar');
	});

	it('does not strip ~/ when it is not at the start', () => {
		expect(transformServerPathToClientPath('/foo/~/bar')).to.equal('/foo/~/bar');
	});

	it('does not strip /wwwroot/ when it is not at the start', () => {
		expect(transformServerPathToClientPath('/foo/wwwroot/bar')).to.equal('/foo/wwwroot/bar');
	});

	it('returns undefined for undefined input', () => {
		expect(transformServerPathToClientPath(undefined)).to.equal(undefined);
	});

	it('returns an empty string unchanged', () => {
		expect(transformServerPathToClientPath('')).to.equal('');
	});

	it('strips ~/ alone to /', () => {
		expect(transformServerPathToClientPath('~/')).to.equal('/');
	});
});
