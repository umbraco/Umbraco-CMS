import { UmbTrashEntityAction } from './trash.action.js';
import { UmbEntityTrashedEvent } from './trash.event.js';
import { UmbTestRecycleBinControllerHostElement } from '../../workspace-context/trashable-entity-workspace-context.test-utils.js';
import type { MetaEntityActionTrashKind } from './types.js';
import { aTimeout, expect } from '@open-wc/testing';
import { UmbActionEventContext } from '@umbraco-cms/backoffice/action';
import type { ManifestApi } from '@umbraco-cms/backoffice/extension-api';
import { UmbRequestReloadStructureForEntityEvent } from '@umbraco-cms/backoffice/entity-action';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';

const ITEM_REPOSITORY_ALIAS = 'Umb.Test.TrashEntityAction.ItemRepository';
const RECYCLE_BIN_REPOSITORY_ALIAS = 'Umb.Test.TrashEntityAction.RecycleBinRepository';

class UmbTestItemRepository {
	async requestItems(uniques: Array<string>) {
		return { data: uniques.map((unique) => ({ unique, name: 'Test Item' })) };
	}
	destroy() {}
}

class UmbTestRecycleBinRepository {
	async requestTrash() {
		return {};
	}
	destroy() {}
}

/** Skips the confirm modal, which has nothing to interact with in a headless test. */
class UmbTestTrashEntityAction extends UmbTrashEntityAction {
	protected override async _confirmTrash() {}
}

describe('UmbTrashEntityAction', () => {
	let host: UmbTestRecycleBinControllerHostElement;
	let actionEventContext: UmbActionEventContext;

	before(() => {
		const itemRepositoryManifest: ManifestApi = {
			type: 'repository',
			alias: ITEM_REPOSITORY_ALIAS,
			name: 'Test Item Repository',
			api: UmbTestItemRepository,
		};
		umbExtensionsRegistry.register(itemRepositoryManifest);

		const recycleBinRepositoryManifest: ManifestApi = {
			type: 'repository',
			alias: RECYCLE_BIN_REPOSITORY_ALIAS,
			name: 'Test Recycle Bin Repository',
			api: UmbTestRecycleBinRepository,
		};
		umbExtensionsRegistry.register(recycleBinRepositoryManifest);
	});

	after(() => {
		umbExtensionsRegistry.unregister(ITEM_REPOSITORY_ALIAS);
		umbExtensionsRegistry.unregister(RECYCLE_BIN_REPOSITORY_ALIAS);
	});

	beforeEach(() => {
		host = new UmbTestRecycleBinControllerHostElement();
		document.body.appendChild(host);
		actionEventContext = new UmbActionEventContext(host);
	});

	afterEach(() => {
		document.body.removeChild(host);
	});

	it('dispatches the trashed event before the structure-reload event', async () => {
		const dispatchedEventTypes: Array<string> = [];
		actionEventContext.addEventListener(UmbEntityTrashedEvent.TYPE, () =>
			dispatchedEventTypes.push(UmbEntityTrashedEvent.TYPE),
		);
		actionEventContext.addEventListener(UmbRequestReloadStructureForEntityEvent.TYPE, () =>
			dispatchedEventTypes.push(UmbRequestReloadStructureForEntityEvent.TYPE),
		);

		const action = new UmbTestTrashEntityAction(host, {
			unique: 'test-unique',
			entityType: 'test-entity-type',
			meta: {
				icon: 'icon-trash',
				label: 'Trash',
				itemRepositoryAlias: ITEM_REPOSITORY_ALIAS,
				recycleBinRepositoryAlias: RECYCLE_BIN_REPOSITORY_ALIAS,
			} satisfies MetaEntityActionTrashKind,
		});

		await action.execute();
		// #notify() isn't awaited by execute(), so give its event dispatch a tick to run.
		await aTimeout(50);

		expect(dispatchedEventTypes).to.deep.equal([
			UmbEntityTrashedEvent.TYPE,
			UmbRequestReloadStructureForEntityEvent.TYPE,
		]);
	});
});
