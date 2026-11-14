import type { UmbFolderModel } from '../types.js';
import type { UmbFolderUpdateModalData, UmbFolderUpdateModalValue } from './folder-update-modal.token.js';
import { UmbFolderModalElement } from './folder-update-modal.element.js';
import { expect, waitUntil } from '@open-wc/testing';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import type { UmbModalContext } from '@umbraco-cms/backoffice/modal';
import type { UUIInputElement } from '@umbraco-cms/backoffice/external/uui';

const FOLDER_REPOSITORY_ALIAS = 'Umb.Test.FolderUpdateModal.FolderRepository';

let releaseFolder: () => void;
let folderReady: Promise<void>;
let folderLoaded: boolean;

class UmbTestFolderRepository {
	async requestByUnique(unique: string) {
		await folderReady;
		folderLoaded = true;
		return { data: { entityType: 'test-folder', unique, name: 'Stored name' } as UmbFolderModel };
	}

	destroy() {}
}

describe('UmbFolderModalElement', () => {
	let element: UmbFolderModalElement;

	beforeEach(() => {
		folderLoaded = false;
		folderReady = new Promise((resolve) => (releaseFolder = resolve));

		umbExtensionsRegistry.register({
			type: 'repository',
			alias: FOLDER_REPOSITORY_ALIAS,
			name: 'Test Folder Repository',
			api: UmbTestFolderRepository,
		} as never);

		const value = new UmbObjectState<UmbFolderUpdateModalValue | undefined>(undefined);
		element = new UmbFolderModalElement();
		element.modalContext = {
			value: value.asObservable(),
			setValue: (newValue: UmbFolderUpdateModalValue) => value.setValue(newValue),
		} as unknown as UmbModalContext<UmbFolderUpdateModalData, UmbFolderUpdateModalValue>;
		element.data = { folderRepositoryAlias: FOLDER_REPOSITORY_ALIAS, unique: 'folder-1' };
		document.body.appendChild(element);
	});

	afterEach(() => {
		element.remove();
		umbExtensionsRegistry.unregister(FOLDER_REPOSITORY_ALIAS);
	});

	function getInput() {
		return element.shadowRoot!.querySelector<UUIInputElement>('#name')!;
	}

	async function folderHasLoaded() {
		await waitUntil(() => folderLoaded, 'the folder was never loaded');
		await new Promise((resolve) => setTimeout(resolve));
		await element.updateComplete;
	}

	it('shows the stored name once the folder has loaded', async () => {
		releaseFolder();
		await folderHasLoaded();

		expect(getInput().value).to.equal('Stored name');
	});

	it('keeps a name typed before the folder has loaded', async () => {
		await element.updateComplete;
		const input = getInput();
		input.value = 'Typed name';
		input.dispatchEvent(new Event('input', { bubbles: true, composed: true }));

		releaseFolder();
		await folderHasLoaded();

		expect(getInput().value).to.equal('Typed name');
	});
});
