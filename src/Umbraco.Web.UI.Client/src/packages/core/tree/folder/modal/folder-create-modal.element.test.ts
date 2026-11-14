import type { UmbFolderModel } from '../types.js';
import type { UmbFolderCreateModalData } from './folder-create-modal.token.js';
import { UmbFolderCreateModalElement } from './folder-create-modal.element.js';
import { expect, waitUntil } from '@open-wc/testing';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import type { UUIInputElement } from '@umbraco-cms/backoffice/external/uui';

const FOLDER_REPOSITORY_ALIAS = 'Umb.Test.FolderCreateModal.FolderRepository';

let releaseScaffold: () => void;
let scaffoldReady: Promise<void>;
let created: Array<{ folder: UmbFolderModel; parentUnique: string | null }>;

class UmbTestFolderRepository {
	async createScaffold() {
		await scaffoldReady;
		return { data: { entityType: 'test-folder', unique: 'new-folder', name: '' } };
	}

	async create(folder: UmbFolderModel, parentUnique: string | null) {
		created.push({ folder, parentUnique });
		return { data: folder };
	}

	destroy() {}
}

describe('UmbFolderCreateModalElement', () => {
	let element: UmbFolderCreateModalElement;
	let releaseRepository: () => void;

	beforeEach(() => {
		created = [];
		scaffoldReady = new Promise((resolve) => (releaseScaffold = resolve));
		const repositoryLoaded = new Promise<{ api: typeof UmbTestFolderRepository }>(
			(resolve) => (releaseRepository = () => resolve({ api: UmbTestFolderRepository })),
		);

		umbExtensionsRegistry.register({
			type: 'repository',
			alias: FOLDER_REPOSITORY_ALIAS,
			name: 'Test Folder Repository',
			api: () => repositoryLoaded,
		} as never);

		element = new UmbFolderCreateModalElement();
		element.data = {
			folderRepositoryAlias: FOLDER_REPOSITORY_ALIAS,
			parent: { entityType: 'test-root', unique: null },
		} as UmbFolderCreateModalData;
		document.body.appendChild(element);
	});

	afterEach(() => {
		element.remove();
		umbExtensionsRegistry.unregister(FOLDER_REPOSITORY_ALIAS);
	});

	async function submit(name: string) {
		await element.updateComplete;
		const input = element.shadowRoot!.querySelector<UUIInputElement>('#name')!;
		input.value = name;
		// uui-input hands its value to the form on update.
		await input.updateComplete;
		element.shadowRoot!.querySelector('form')!.requestSubmit();
	}

	it('creates the folder when submitted before the folder repository has loaded', async () => {
		await submit('Early folder');
		releaseRepository();
		releaseScaffold();

		await waitUntil(() => created.length > 0, 'the folder was never created');
		expect(created[0].folder.name).to.equal('Early folder');
		expect(created[0].parentUnique).to.be.null;
	});

	it('creates the folder when submitted before the folder scaffold is ready', async () => {
		releaseRepository();
		await submit('Early folder');
		releaseScaffold();

		await waitUntil(() => created.length > 0, 'the folder was never created');
		expect(created[0].folder.unique).to.equal('new-folder');
	});

	it('creates the folder once when submitted twice while it is not ready', async () => {
		await submit('Early folder');
		element.shadowRoot!.querySelector('form')!.requestSubmit();
		releaseRepository();
		releaseScaffold();

		await waitUntil(() => created.length > 0, 'the folder was never created');
		await new Promise((resolve) => setTimeout(resolve));
		expect(created).to.have.lengthOf(1);
	});
});
