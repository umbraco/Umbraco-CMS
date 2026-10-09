import { removeInitialSlashFromPath } from './remove-initial-slash-from-path.function.js';
import { expect } from '@open-wc/testing';

describe('removeInitialSlashFromPath', () => {
	it('removes a single leading slash', () => {
		expect(removeInitialSlashFromPath('/foo')).to.equal('foo');
	});

	it('returns the path unchanged when there is no leading slash', () => {
		expect(removeInitialSlashFromPath('foo')).to.equal('foo');
	});

	it('only removes one slash, even when several are present', () => {
		expect(removeInitialSlashFromPath('//foo')).to.equal('/foo');
	});

	it('returns the empty string when given a single slash', () => {
		expect(removeInitialSlashFromPath('/')).to.equal('');
	});

	it('returns an empty string unchanged', () => {
		expect(removeInitialSlashFromPath('')).to.equal('');
	});

	it('does not touch a trailing slash', () => {
		expect(removeInitialSlashFromPath('foo/')).to.equal('foo/');
	});
});
