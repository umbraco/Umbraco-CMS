import { getMediaFileExtension } from './get-media-file-extension.function.js';
import { expect } from '@open-wc/testing';

describe('getMediaFileExtension', () => {
	const folderTypeUniques = new Set(['folder']);
	const derive = (name: string | undefined, mediaTypeUnique: string | undefined = 'image') =>
		getMediaFileExtension({ name, mediaTypeUnique, folderTypeUniques });

	it('derives the extension from the name, lower-cased', () => {
		expect(derive('holiday-photo.JPG')).to.equal('jpg');
	});

	it('takes only the last segment of a double extension', () => {
		expect(derive('archive.tar.gz')).to.equal('gz');
	});

	it('returns nothing for a folder, whatever its name looks like', () => {
		expect(derive('Campaign 2026.Q1', 'folder')).to.be.undefined;
	});

	it('returns nothing for a name without an extension', () => {
		expect(derive('README')).to.be.undefined;
	});

	it('returns nothing when the dot separates prose rather than a suffix', () => {
		expect(derive('Version 2.0 mockup')).to.be.undefined;
		expect(derive('My holiday. Best trip ever')).to.be.undefined;
	});

	it('returns nothing while the folder types are unknown', () => {
		expect(getMediaFileExtension({ name: 'holiday-photo.jpg', mediaTypeUnique: 'image', folderTypeUniques: undefined }))
			.to.be.undefined;
	});

	it('tolerates an item whose name or media type has not arrived yet', () => {
		expect(derive(undefined)).to.be.undefined;
		expect(derive('report.pdf', undefined)).to.equal('pdf');
	});
});
