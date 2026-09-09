import { getMediaFileExtension } from './get-media-file-extension.function.js';
import { expect } from '@open-wc/testing';

describe('getMediaFileExtension', () => {
	const folders = new Set(['folder']);

	it('derives the extension from the name, lower-cased', () => {
		expect(getMediaFileExtension('holiday-photo.JPG', 'image', folders)).to.equal('jpg');
	});

	it('returns nothing for a container, whatever its name looks like', () => {
		expect(getMediaFileExtension('Campaign 2026.Q1', 'folder', folders)).to.be.undefined;
	});

	it('returns nothing for a name without an extension', () => {
		expect(getMediaFileExtension('README', 'image', folders)).to.be.undefined;
	});

	it('returns nothing while the folder types are unknown', () => {
		expect(getMediaFileExtension('holiday-photo.jpg', 'image', undefined)).to.be.undefined;
	});

	it('tolerates a missing name or media type', () => {
		expect(getMediaFileExtension(null, 'image', folders)).to.be.undefined;
		expect(getMediaFileExtension('report.pdf', null, folders)).to.equal('pdf');
	});
});
