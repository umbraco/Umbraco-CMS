import { getMediaFileExtension } from './get-media-file-extension.function.js';
import { expect } from '@open-wc/testing';

describe('getMediaFileExtension', () => {
	const folderTypeUniques = new Set(['folder']);

	it('derives the extension from the name, lower-cased', () => {
		expect(getMediaFileExtension({ name: 'holiday-photo.JPG', mediaTypeUnique: 'image', folderTypeUniques })).to.equal(
			'jpg',
		);
	});

	it('returns nothing for a folder, whatever its name looks like', () => {
		expect(getMediaFileExtension({ name: 'Campaign 2026.Q1', mediaTypeUnique: 'folder', folderTypeUniques })).to.be
			.undefined;
	});

	it('returns nothing for a name without an extension', () => {
		expect(getMediaFileExtension({ name: 'README', mediaTypeUnique: 'image', folderTypeUniques })).to.be.undefined;
	});

	it('returns nothing while the folder types are unknown', () => {
		expect(getMediaFileExtension({ name: 'holiday-photo.jpg', mediaTypeUnique: 'image', folderTypeUniques: undefined }))
			.to.be.undefined;
	});

	it('tolerates an item whose name or media type has not arrived yet', () => {
		expect(getMediaFileExtension({ name: undefined, mediaTypeUnique: 'image', folderTypeUniques })).to.be.undefined;
		expect(getMediaFileExtension({ name: 'report.pdf', mediaTypeUnique: undefined, folderTypeUniques })).to.equal(
			'pdf',
		);
	});
});
