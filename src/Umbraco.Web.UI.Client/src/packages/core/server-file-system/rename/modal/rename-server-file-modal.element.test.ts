import type { UmbRenameModalData } from './rename-server-file-modal.token.js';
import { UmbRenameModalElement } from './rename-server-file-modal.element.js';
import { expect, waitUntil } from '@open-wc/testing';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import type { UUIInputElement } from '@umbraco-cms/backoffice/external/uui';

const ITEM_REPOSITORY_ALIAS = 'Umb.Test.RenameModal.ItemRepository';
const RENAME_REPOSITORY_ALIAS = 'Umb.Test.RenameModal.RenameRepository';

let renamed: Array<{ unique: string; name: string }>;
let existingUniques: Array<string>;
let itemLookups: number;

class UmbTestItemRepository {
	async requestItems(uniques: Array<string>) {
		itemLookups++;
		return {
			data: uniques.filter((unique) => existingUniques.includes(unique)).map((unique) => ({ unique, name: 'Original name' })),
		};
	}

	destroy() {}
}

class UmbTestRenameRepository {
	async rename(unique: string, name: string) {
		renamed.push({ unique, name });
		return { data: { unique, name } };
	}

	destroy() {}
}

describe('UmbRenameModalElement', () => {
	let element: UmbRenameModalElement;
	let releaseRenameRepository: () => void;

	beforeEach(() => {
		renamed = [];
		existingUniques = ['file.js'];
		itemLookups = 0;
		const renameRepositoryLoaded = new Promise<{ api: typeof UmbTestRenameRepository }>(
			(resolve) => (releaseRenameRepository = () => resolve({ api: UmbTestRenameRepository })),
		);

		umbExtensionsRegistry.registerMany([
			{ type: 'repository', alias: ITEM_REPOSITORY_ALIAS, name: 'Test Item Repository', api: UmbTestItemRepository },
			{
				type: 'repository',
				alias: RENAME_REPOSITORY_ALIAS,
				name: 'Test Rename Repository',
				api: () => renameRepositoryLoaded,
			},
		] as Array<never>);

		element = new UmbRenameModalElement();
		element.data = {
			itemRepositoryAlias: ITEM_REPOSITORY_ALIAS,
			renameRepositoryAlias: RENAME_REPOSITORY_ALIAS,
			unique: 'file.js',
		} as UmbRenameModalData;
		document.body.appendChild(element);
	});

	afterEach(() => {
		element.remove();
		umbExtensionsRegistry.unregister(ITEM_REPOSITORY_ALIAS);
		umbExtensionsRegistry.unregister(RENAME_REPOSITORY_ALIAS);
	});

	async function submit(name: string) {
		await element.updateComplete;
		const input = element.shadowRoot!.querySelector<UUIInputElement>('#name')!;
		input.value = name;
		// uui-input hands its value to the form on update.
		await input.updateComplete;
		element.shadowRoot!.querySelector('form')!.requestSubmit();
	}

	it('renames the file when submitted before the rename repository has loaded', async () => {
		await submit('renamed.js');
		releaseRenameRepository();

		await waitUntil(() => renamed.length > 0, 'the file was never renamed');
		expect(renamed[0]).to.deep.equal({ unique: 'file.js', name: 'renamed.js' });
	});

	it('opens without an error when the file no longer exists', async () => {
		const unhandled: Array<unknown> = [];
		const onUnhandled = (event: PromiseRejectionEvent) => {
			unhandled.push(event.reason);
			event.preventDefault();
		};
		window.addEventListener('unhandledrejection', onUnhandled);

		try {
			existingUniques = [];
			releaseRenameRepository();

			await waitUntil(() => itemLookups > 0, 'the file was never looked up');
			await new Promise((resolve) => setTimeout(resolve));

			expect(unhandled).to.have.lengthOf(0);
		} finally {
			window.removeEventListener('unhandledrejection', onUnhandled);
		}
	});

	it('renames the file once when submitted twice while it is not ready', async () => {
		await submit('renamed.js');
		element.shadowRoot!.querySelector('form')!.requestSubmit();
		releaseRenameRepository();

		await waitUntil(() => renamed.length > 0, 'the file was never renamed');
		await new Promise((resolve) => setTimeout(resolve));
		expect(renamed).to.have.lengthOf(1);
	});

	function getInput() {
		return element.shadowRoot!.querySelector<UUIInputElement>('#name')!;
	}

	async function itemHasLoaded() {
		await waitUntil(() => itemLookups > 0, 'the file was never looked up');
		await new Promise((resolve) => setTimeout(resolve));
		await element.updateComplete;
	}

	it('shows the current name once the file has loaded', async () => {
		releaseRenameRepository();
		await itemHasLoaded();

		expect(getInput().value).to.equal('Original name');
	});

	it('keeps a name typed before the file has loaded', async () => {
		await element.updateComplete;
		const input = getInput();
		input.value = 'typed.js';
		input.dispatchEvent(new Event('input', { bubbles: true, composed: true }));

		releaseRenameRepository();
		await itemHasLoaded();

		expect(getInput().value).to.equal('typed.js');
	});
});
