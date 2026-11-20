import { UmbDuplicateToEntityAction } from './duplicate-to.action.js';
import type { MetaEntityActionDuplicateToKind, UmbDuplicateToRequestArgs } from './types.js';
import type { UmbTreeItemModel } from '../../types.js';
import type { UmbTreePickerModalData, UmbTreePickerModalValue } from '../../tree-picker-modal/index.js';
import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbActionEventContext } from '@umbraco-cms/backoffice/action';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import {
	UmbRequestReloadChildrenOfEntityEvent,
	UmbRequestReloadStructureForEntityEvent,
} from '@umbraco-cms/backoffice/entity-action';
import type { UmbEntityActionEvent } from '@umbraco-cms/backoffice/entity-action';
import type { ManifestApi } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbModalManagerContext } from '@umbraco-cms/backoffice/modal';
import type { UmbModalContext } from '@umbraco-cms/backoffice/modal';

const TREE_REPOSITORY_ALIAS = 'Umb.Test.DuplicateToEntityAction.TreeRepository';
const DUPLICATE_REPOSITORY_ALIAS = 'Umb.Test.DuplicateToEntityAction.DuplicateRepository';

const ITEM: UmbEntityModel = { entityType: 'test-item', unique: 'item-unique' };
const DUPLICATE: UmbEntityModel = { entityType: 'test-item', unique: 'duplicate-unique' };
const ROOT: UmbEntityModel = { entityType: 'test-root', unique: null };
const FOLDER: UmbEntityModel = { entityType: 'test-folder', unique: 'folder-unique' };
const CURRENT_PARENT: UmbEntityModel = { entityType: 'test-folder', unique: 'current-parent-unique' };
const DESTINATION: UmbEntityModel = { entityType: 'test-folder', unique: 'destination-unique' };

let ancestorsByUnique: Record<string, Array<UmbEntityModel>>;
let duplicateResponse: { data?: string; error?: Error };
let duplicateArgs: Array<UmbDuplicateToRequestArgs>;
let calls: Array<string>;
let treeRepositories: Array<UmbTestTreeRepository>;

/**
 * Builds ancestor tree items from a path, with each entry's parent being the entry before it and the first the root.
 * @param {Array<UmbEntityModel> | undefined} path - The path from the top of the tree down to the item.
 * @returns {Array<UmbEntityModel & { parent: UmbEntityModel }> | undefined} The ancestors with parents.
 */
function withParents(path: Array<UmbEntityModel> | undefined) {
	return path?.map((entity, index) => ({ ...entity, parent: path[index - 1] ?? ROOT }));
}

class UmbTestTreeRepository extends UmbControllerBase {
	destroyed = false;

	constructor(host: UmbControllerHost) {
		super(host);
		treeRepositories.push(this);
	}

	async requestTreeItemAncestors(args: { treeItem: { unique: string } }) {
		calls.push(`requestTreeItemAncestors:${args.treeItem.unique}`);
		return { data: withParents(ancestorsByUnique[args.treeItem.unique] ?? []) };
	}
	override destroy() {
		this.destroyed = true;
		super.destroy();
	}
}

class UmbTestDuplicateRepository {
	async requestDuplicateTo(args: UmbDuplicateToRequestArgs) {
		calls.push('requestDuplicateTo');
		duplicateArgs.push(args);
		return duplicateResponse;
	}
	destroy() {}
}

@customElement('umb-test-duplicate-to-entity-action-host')
class UmbTestDuplicateToEntityActionHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

type UmbTestTreePickerModalContext = UmbModalContext<UmbTreePickerModalData<UmbTreeItemModel>, UmbTreePickerModalValue>;

describe('UmbDuplicateToEntityAction', () => {
	let host: UmbTestDuplicateToEntityActionHostElement;
	let actionEventContext: UmbActionEventContext;
	let modalManager: UmbModalManagerContext;
	let events: Array<{ type: string; entity: UmbEntityModel }>;
	let pickerData: UmbTreePickerModalData<UmbTreeItemModel> | undefined;

	before(() => {
		const treeRepositoryManifest: ManifestApi = {
			type: 'repository',
			alias: TREE_REPOSITORY_ALIAS,
			name: 'Test Tree Repository',
			api: UmbTestTreeRepository,
		};
		umbExtensionsRegistry.register(treeRepositoryManifest);

		const duplicateRepositoryManifest: ManifestApi = {
			type: 'repository',
			alias: DUPLICATE_REPOSITORY_ALIAS,
			name: 'Test Duplicate Repository',
			api: UmbTestDuplicateRepository,
		};
		umbExtensionsRegistry.register(duplicateRepositoryManifest);
	});

	after(() => {
		umbExtensionsRegistry.unregister(TREE_REPOSITORY_ALIAS);
		umbExtensionsRegistry.unregister(DUPLICATE_REPOSITORY_ALIAS);
	});

	beforeEach(() => {
		ancestorsByUnique = {};
		duplicateResponse = { data: DUPLICATE.unique! };
		duplicateArgs = [];
		calls = [];
		treeRepositories = [];
		events = [];
		pickerData = undefined;

		host = new UmbTestDuplicateToEntityActionHostElement();
		document.body.appendChild(host);
		actionEventContext = new UmbActionEventContext(host);
		modalManager = new UmbModalManagerContext(host);

		const recordEvent = (event: Event) => {
			const entityActionEvent = event as UmbEntityActionEvent;
			calls.push(event.type);
			events.push({
				type: event.type,
				entity: { entityType: entityActionEvent.getEntityType(), unique: entityActionEvent.getUnique() },
			});
		};
		actionEventContext.addEventListener(UmbRequestReloadChildrenOfEntityEvent.TYPE, recordEvent);
		actionEventContext.addEventListener(UmbRequestReloadStructureForEntityEvent.TYPE, recordEvent);
	});

	afterEach(() => {
		document.body.removeChild(host);
	});

	/**
	 * Answers the destination picker as soon as it opens: picks the given destination, or cancels when it is `undefined`.
	 * @param {string | null | undefined} destinationUnique - The unique to pick, `null` for the root.
	 */
	function answerPicker(destinationUnique: string | null | undefined) {
		const subscription = modalManager.modals.subscribe((modals) => {
			const modal = modals[0] as UmbTestTreePickerModalContext | undefined;
			if (!modal) return;
			subscription.unsubscribe();
			pickerData = modal.data;
			if (destinationUnique === undefined) {
				modal.reject();
			} else {
				modal.setValue({ selection: [destinationUnique] });
				modal.submit();
			}
		});
	}

	function createAction(
		entity: UmbEntityModel = ITEM,
		meta: Partial<MetaEntityActionDuplicateToKind> = {},
	): UmbDuplicateToEntityAction {
		return new UmbDuplicateToEntityAction(host, {
			unique: entity.unique,
			entityType: entity.entityType,
			meta: {
				icon: 'icon-documents',
				label: 'Duplicate to',
				duplicateRepositoryAlias: DUPLICATE_REPOSITORY_ALIAS,
				treeRepositoryAlias: TREE_REPOSITORY_ALIAS,
				treeAlias: 'Umb.Test.Tree',
				...meta,
			},
		});
	}

	async function executeDuplicate(meta: Partial<MetaEntityActionDuplicateToKind> = {}) {
		await createAction(ITEM, meta).execute();
	}

	async function executeExpectingError(action: UmbDuplicateToEntityAction): Promise<unknown> {
		try {
			await action.execute();
		} catch (error) {
			return error;
		}
		return undefined;
	}

	describe('destination picker', () => {
		it('opens the picker for the configured tree', async () => {
			answerPicker(undefined);

			await executeDuplicate({ treeAlias: 'Umb.Test.OtherTree', foldersOnly: true });

			expect(pickerData?.treeAlias).to.equal('Umb.Test.OtherTree');
			expect(pickerData?.foldersOnly).to.be.true;
		});

		it('expands the tree to the current location of the item', async () => {
			answerPicker(undefined);
			ancestorsByUnique[ITEM.unique!] = [FOLDER, CURRENT_PARENT, ITEM];

			await executeDuplicate();

			expect(pickerData?.treeExpansion).to.deep.equal([{ ...FOLDER, target: CURRENT_PARENT }, { ...CURRENT_PARENT }]);
		});
	});

	describe('duplicate', () => {
		it('duplicates the item to the picked destination', async () => {
			answerPicker(DESTINATION.unique);

			await executeDuplicate();

			expect(duplicateArgs).to.deep.equal([{ unique: ITEM.unique, destination: { unique: DESTINATION.unique } }]);
		});

		it('duplicates the item to the root when the root is picked', async () => {
			answerPicker(null);

			await executeDuplicate();

			expect(duplicateArgs).to.deep.equal([{ unique: ITEM.unique, destination: { unique: null } }]);
		});

		it('does not duplicate or reload anything when the picker is cancelled', async () => {
			answerPicker(undefined);

			await executeDuplicate();

			expect(calls).to.not.include('requestDuplicateTo');
			expect(events).to.have.lengthOf(0);
		});
	});

	describe('errors', () => {
		it('throws the duplicate error and reloads nothing when the duplicate fails', async () => {
			answerPicker(DESTINATION.unique);
			duplicateResponse = { error: new Error('Duplicate failed') };

			const thrown = await executeExpectingError(createAction());

			expect(thrown).to.equal(duplicateResponse.error);
			expect(events).to.have.lengthOf(0);
		});

		it('throws when the unique is missing', async () => {
			const thrown = await executeExpectingError(createAction({ entityType: ITEM.entityType, unique: null }));

			expect(thrown).to.be.instanceOf(Error);
			expect(calls).to.have.lengthOf(0);
		});

		it('throws when the entity type is missing', async () => {
			const thrown = await executeExpectingError(createAction({ entityType: '', unique: ITEM.unique }));

			expect(thrown).to.be.instanceOf(Error);
			expect(calls).to.have.lengthOf(0);
		});

		it('throws when the duplicate repository is not available', async () => {
			answerPicker(DESTINATION.unique);

			const thrown = await executeExpectingError(
				createAction(ITEM, { duplicateRepositoryAlias: 'Umb.Test.MissingRepository' }),
			);

			expect(thrown).to.be.instanceOf(Error);
			expect(calls).to.not.include('requestDuplicateTo');
		});
	});

	describe('reload after duplicate', () => {
		it('reloads the children and structure of the destination, taken from the parent of the duplicate', async () => {
			answerPicker(DESTINATION.unique);
			ancestorsByUnique[ITEM.unique!] = [FOLDER, CURRENT_PARENT, ITEM];
			ancestorsByUnique[DUPLICATE.unique!] = [FOLDER, DESTINATION, DUPLICATE];

			await executeDuplicate();

			expect(calls).to.include(`requestTreeItemAncestors:${DUPLICATE.unique}`);
			expect(events).to.deep.include({ type: UmbRequestReloadChildrenOfEntityEvent.TYPE, entity: DESTINATION });
			expect(events).to.deep.include({ type: UmbRequestReloadStructureForEntityEvent.TYPE, entity: DESTINATION });
		});

		it('reloads only the children of the tree root when duplicated to the root', async () => {
			answerPicker(null);
			ancestorsByUnique[DUPLICATE.unique!] = [DUPLICATE];

			await executeDuplicate();

			expect(events).to.deep.equal([
				{ type: UmbRequestReloadStructureForEntityEvent.TYPE, entity: ITEM },
				{ type: UmbRequestReloadChildrenOfEntityEvent.TYPE, entity: ROOT },
			]);
		});

		it('resolves the destination before dispatching any reload', async () => {
			answerPicker(DESTINATION.unique);
			ancestorsByUnique[DUPLICATE.unique!] = [FOLDER, DESTINATION, DUPLICATE];

			await executeDuplicate();

			const lookupIndex = calls.indexOf(`requestTreeItemAncestors:${DUPLICATE.unique}`);
			const firstEventIndex = calls.findIndex((call) => call.startsWith('request-reload'));
			expect(lookupIndex).to.be.greaterThan(calls.indexOf('requestDuplicateTo'));
			expect(lookupIndex).to.be.lessThan(firstEventIndex);
		});

		it('uses a single tree repository and destroys it once done', async () => {
			answerPicker(DESTINATION.unique);
			ancestorsByUnique[DUPLICATE.unique!] = [FOLDER, DESTINATION, DUPLICATE];

			await executeDuplicate();

			expect(treeRepositories).to.have.length(1);
			expect(treeRepositories[0].destroyed).to.be.true;
		});

		it('destroys the tree repository when the picker is cancelled', async () => {
			answerPicker(undefined);

			await executeDuplicate();

			expect(treeRepositories).to.have.length(1);
			expect(treeRepositories[0].destroyed).to.be.true;
		});

		it('reloads only the original item when the repository does not return the duplicate', async () => {
			answerPicker(DESTINATION.unique);
			duplicateResponse = {};

			await executeDuplicate();

			expect(events).to.deep.equal([{ type: UmbRequestReloadStructureForEntityEvent.TYPE, entity: ITEM }]);
		});
	});
});
