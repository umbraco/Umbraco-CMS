import { UmbMediaPickerFolderPathElement } from './media-picker-folder-path.element.js';
import { aTimeout, expect, fixture, html, waitUntil } from '@open-wc/testing';
import { UmbMediaTypeStructureRepository } from '@umbraco-cms/backoffice/media-type';

describe('UmbMediaPickerFolderPathElement', () => {
	const originals = {
		requestAllAllowedChildrenOf: UmbMediaTypeStructureRepository.prototype.requestAllAllowedChildrenOf,
		requestMediaTypesOfFolders: UmbMediaTypeStructureRepository.prototype.requestMediaTypesOfFolders,
	};

	let folderTypesRequested = false;

	async function createElement(noAccess: boolean) {
		const element = await fixture<UmbMediaPickerFolderPathElement>(
			html`<umb-media-picker-folder-path .noAccess=${noAccess}></umb-media-picker-folder-path>`,
		);
		await waitUntil(() => folderTypesRequested);
		await aTimeout(0);
		await element.updateComplete;
		return element;
	}

	beforeEach(() => {
		folderTypesRequested = false;
		UmbMediaTypeStructureRepository.prototype.requestAllAllowedChildrenOf = (async () => ({
			data: { items: [{ unique: 'folder-type' }], total: 1 },
		})) as never;
		UmbMediaTypeStructureRepository.prototype.requestMediaTypesOfFolders = (async () => {
			folderTypesRequested = true;
			return [{ unique: 'folder-type', name: 'Folder' }];
		}) as never;
	});

	afterEach(() => {
		UmbMediaTypeStructureRepository.prototype.requestAllAllowedChildrenOf = originals.requestAllAllowedChildrenOf;
		UmbMediaTypeStructureRepository.prototype.requestMediaTypesOfFolders = originals.requestMediaTypesOfFolders;
	});

	it('offers to add a folder when the user has access', async () => {
		const element = await createElement(false);
		expect(element.shadowRoot!.querySelector('#add-folder') !== null).to.be.true;
	});

	it('does not offer to add a folder when the user has no access', async () => {
		const element = await createElement(true);
		expect(element.shadowRoot!.querySelector('#add-folder') !== null).to.be.false;
	});
});
