import { removeLastSlashFromPath } from './remove-last-slash-from-path.function.js';
import { expect } from '@open-wc/testing';

describe('removeLastSlashFromPath', () => {
	it('removes a single trailing slash', () => {
		expect(removeLastSlashFromPath('foo/')).to.equal('foo');
	});

	it('returns the path unchanged when there is no trailing slash', () => {
		expect(removeLastSlashFromPath('foo')).to.equal('foo');
	});

	it('only removes one slash, even when several are present', () => {
		expect(removeLastSlashFromPath('foo//')).to.equal('foo/');
	});

	it('returns the empty string when given a single slash', () => {
		expect(removeLastSlashFromPath('/')).to.equal('');
	});

	it('returns an empty string unchanged', () => {
		expect(removeLastSlashFromPath('')).to.equal('');
	});

	it('does not touch a leading slash', () => {
		expect(removeLastSlashFromPath('/foo')).to.equal('/foo');
	});
});
