import { getUnsupportedBlockMessageKeys } from './get-unsupported-block-message-keys.function.js';
import { expect } from '@open-wc/testing';

describe('getUnsupportedBlockMessageKeys', () => {
	it('explains that the library element was deleted', () => {
		expect(getUnsupportedBlockMessageKeys('deleted')).to.deep.equal({
			nameKey: 'blockEditor_deletedLibraryElementName',
			descriptionKey: 'blockEditor_deletedLibraryElementDescription',
		});
	});

	it('explains that the library element is not accessible', () => {
		expect(getUnsupportedBlockMessageKeys('noAccess')).to.deep.equal({
			nameKey: 'blockEditor_noAccessLibraryElementName',
			descriptionKey: 'blockEditor_noAccessLibraryElementDescription',
		});
	});

	it('falls back to the generic unsupported message without a failure reason', () => {
		expect(getUnsupportedBlockMessageKeys(undefined)).to.deep.equal({
			nameKey: 'blockEditor_unsupportedBlockName',
			descriptionKey: 'blockEditor_unsupportedBlockDescription',
		});
	});
});
