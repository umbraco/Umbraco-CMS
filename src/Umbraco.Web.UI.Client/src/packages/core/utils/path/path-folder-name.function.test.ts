import { pathFolderName } from './path-folder-name.function.js';
import { expect } from '@open-wc/testing';

describe('pathFolderName', () => {
	it('produces a camelCase folder name from a multi-word title', () => {
		expect(pathFolderName('My Folder Name')).to.equal('myFolderName');
	});
});
