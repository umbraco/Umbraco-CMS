import { getParentPathFromServerPath } from './parent-path-from-server-path.function.js';
import { expect } from '@open-wc/testing';

describe('parent-path-from-server-path', () => {
	it('it returns the parent path of a nested server path', () => {
		const path = 'Folder A/Folder AA/Folder AAA';
		const expectedParentPath = 'Folder A/Folder AA';
		expect(getParentPathFromServerPath(path)).to.equal(expectedParentPath);
	});

	it('it returns null of a root server path', () => {
		const path = 'Folder A';
		expect(getParentPathFromServerPath(path)).to.be.null;
	});
});
