import { getFileExtension } from './get-file-extension.function.js';
import { expect } from '@open-wc/testing';

describe('getFileExtension', () => {
	it('should return the extension for a standard filename', () => {
		expect(getFileExtension('photo.jpg')).to.eq('jpg');
	});

	it('should return the last extension for a multi-dot filename', () => {
		expect(getFileExtension('archive.tar.gz')).to.eq('gz');
	});

	it('should return undefined for a filename without a dot', () => {
		expect(getFileExtension('README')).to.be.undefined;
	});

	it('should return undefined for a dotfile without an extension', () => {
		expect(getFileExtension('.gitignore')).to.be.undefined;
	});

	it('should return undefined for a filename ending with a dot', () => {
		expect(getFileExtension('file.')).to.be.undefined;
	});

	it('should return the extension for a dotfile with an extension', () => {
		expect(getFileExtension('.eslintrc.json')).to.eq('json');
	});

	it('should preserve the case of the extension', () => {
		expect(getFileExtension('Document.PDF')).to.eq('PDF');
	});
});
