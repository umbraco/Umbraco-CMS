import { UmbMoveToEntityAction } from './move-to.action.js';
import type { MetaEntityActionMoveToKind } from './types.js';
import type { UmbMoveToRequestArgs } from './types.js';
import type { UmbTreeItemModel } from '../../types.js';
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
import type { UmbTreePickerModalData, UmbTreePickerModalValue } from '../../tree-picker-modal/index.js';

const TREE_REPOSITORY_ALIAS = 'Umb.Test.MoveToEntityAction.TreeRepository';
const MOVE_REPOSITORY_ALIAS = 'Umb.Test.MoveToEntityAction.MoveRepository';

const ITEM: UmbEntityModel = { entityType: 'test-item', unique: 'item-unique' };
const ROOT: UmbEntityModel = { entityType: 'test-root', unique: null };
const FOLDER: UmbEntityModel = { entityType: 'test-folder', unique: 'folder-unique' };
const CURRENT_PARENT: UmbEntityModel = { entityType: 'test-folder', unique: 'current-parent-unique' };
const DESTINATION: UmbEntityModel = { entityType: 'test-folder', unique: 'destination-unique' };

let currentAncestors: Array<UmbEntityModel> | undefined;
let newAncestors: Array<UmbEntityModel> | undefined;
let ancestorsError: Error | undefined;
let moveError: Error | undefined;
let moved: boolean;
let moveArgs: Array<UmbMoveToRequestArgs>;
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

	async requestTreeItemAncestors() {
		calls.push('requestTreeItemAncestors');
		return { data: withParents(moved ? newAncestors : currentAncestors), error: ancestorsError };
	}
	override destroy() {
		this.destroyed = true;
		super.destroy();
	}
}

class UmbTestMoveRepository {
	async requestMoveTo(args: UmbMoveToRequestArgs) {
		calls.push('requestMoveTo');
		moveArgs.push(args);
		if (moveError) return { error: moveError };
		moved = true;
		return {};
	}
	destroy() {}
}

@customElement('umb-test-move-to-entity-action-host')
class UmbTestMoveToEntityActionHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

type UmbTestTreePickerModalContext = UmbModalContext<UmbTreePickerModalData<UmbTreeItemModel>, UmbTreePickerModalValue>;

describe('UmbMoveToEntityAction', () => {
	let host: UmbTestMoveToEntityActionHostElement;
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

		const moveRepositoryManifest: ManifestApi = {
			type: 'repository',
			alias: MOVE_REPOSITORY_ALIAS,
			name: 'Test Move Repository',
			api: UmbTestMoveRepository,
		};
		umbExtensionsRegistry.register(moveRepositoryManifest);
	});

	after(() => {
		umbExtensionsRegistry.unregister(TREE_REPOSITORY_ALIAS);
		umbExtensionsRegistry.unregister(MOVE_REPOSITORY_ALIAS);
	});

	beforeEach(() => {
		currentAncestors = [];
		newAncestors = [];
		ancestorsError = undefined;
		moveError = undefined;
		moved = false;
		moveArgs = [];
		calls = [];
		treeRepositories = [];
		events = [];
		pickerData = undefined;

		host = new UmbTestMoveToEntityActionHostElement();
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
		meta: Partial<MetaEntityActionMoveToKind> = {},
	): UmbMoveToEntityAction {
		return new UmbMoveToEntityAction(host, {
			unique: entity.unique,
			entityType: entity.entityType,
			meta: {
				icon: 'icon-enter',
				label: 'Move to',
				moveRepositoryAlias: MOVE_REPOSITORY_ALIAS,
				treeRepositoryAlias: TREE_REPOSITORY_ALIAS,
				treeAlias: 'Umb.Test.Tree',
				...meta,
			},
		});
	}

	async function executeMove(meta: Partial<MetaEntityActionMoveToKind> = {}) {
		await createAction(ITEM, meta).execute();
	}

	describe('destination picker', () => {
		it('opens the picker for the configured tree', async () => {
			answerPicker(undefined);

			await executeMove({ treeAlias: 'Umb.Test.OtherTree', foldersOnly: true, searchProviderAlias: 'Umb.Test.Search' });

			expect(pickerData?.treeAlias).to.equal('Umb.Test.OtherTree');
			expect(pickerData?.foldersOnly).to.be.true;
			expect(pickerData?.search).to.deep.equal({ providerAlias: 'Umb.Test.Search' });
		});

		it('does not enable search when no search provider is configured', async () => {
			answerPicker(undefined);

			await executeMove();

			expect(pickerData?.search).to.be.undefined;
		});

		it('does not allow the moved item to be picked as its own destination', async () => {
			answerPicker(undefined);

			await executeMove();

			const pickableFilter = pickerData?.pickableFilter;
			expect(pickableFilter?.({ ...ITEM } as UmbTreeItemModel)).to.be.false;
			expect(pickableFilter?.({ ...DESTINATION } as UmbTreeItemModel)).to.be.true;
		});

		it('expands the tree to the current location of the moved item', async () => {
			answerPicker(undefined);
			currentAncestors = [FOLDER, CURRENT_PARENT, ITEM];

			await executeMove();

			expect(pickerData?.treeExpansion).to.deep.equal([{ ...FOLDER, target: CURRENT_PARENT }, { ...CURRENT_PARENT }]);
		});

		it('opens the picker without expansion when the current location cannot be resolved', async () => {
			answerPicker(undefined);
			currentAncestors = undefined;
			ancestorsError = new Error('Ancestors could not be loaded');

			await executeMove();

			expect(pickerData).to.exist;
			expect(pickerData?.treeExpansion).to.be.undefined;
		});
	});

	describe('move', () => {
		it('moves the item to the picked destination', async () => {
			answerPicker(DESTINATION.unique);

			await executeMove();

			expect(moveArgs).to.deep.equal([{ unique: ITEM.unique, destination: { unique: DESTINATION.unique } }]);
		});

		it('moves the item to the root when the root is picked', async () => {
			answerPicker(null);

			await executeMove();

			expect(moveArgs).to.deep.equal([{ unique: ITEM.unique, destination: { unique: null } }]);
		});

		it('does not move or reload anything when the picker is cancelled', async () => {
			answerPicker(undefined);

			await executeMove();

			expect(calls).to.not.include('requestMoveTo');
			expect(events).to.have.lengthOf(0);
		});
	});

	describe('errors', () => {
		it('throws the move error and reloads nothing when the move fails', async () => {
			answerPicker(DESTINATION.unique);
			moveError = new Error('Move failed');

			let thrown: unknown;
			try {
				await createAction().execute();
			} catch (error) {
				thrown = error;
			}

			expect(thrown).to.equal(moveError);
			expect(events).to.have.lengthOf(0);
		});

		it('throws when the unique is missing', async () => {
			let thrown: unknown;
			try {
				await createAction({ entityType: ITEM.entityType, unique: null }).execute();
			} catch (error) {
				thrown = error;
			}

			expect(thrown).to.be.instanceOf(Error);
			expect(calls).to.have.lengthOf(0);
		});

		it('throws when the entity type is missing', async () => {
			let thrown: unknown;
			try {
				await createAction({ entityType: '', unique: ITEM.unique }).execute();
			} catch (error) {
				thrown = error;
			}

			expect(thrown).to.be.instanceOf(Error);
			expect(calls).to.have.lengthOf(0);
		});

		it('throws when the move repository is not available', async () => {
			answerPicker(DESTINATION.unique);

			let thrown: unknown;
			try {
				await createAction(ITEM, { moveRepositoryAlias: 'Umb.Test.MissingRepository' }).execute();
			} catch (error) {
				thrown = error;
			}

			expect(thrown).to.be.instanceOf(Error);
			expect(calls).to.not.include('requestMoveTo');
		});
	});

	describe('reload after move', () => {
		it('reloads the structure of the moved item', async () => {
			answerPicker(DESTINATION.unique);
			newAncestors = [FOLDER, DESTINATION, ITEM];

			await executeMove();

			expect(events).to.deep.include({ type: UmbRequestReloadStructureForEntityEvent.TYPE, entity: ITEM });
		});

		it('reloads the children and structure of the destination, taken from the parent of the moved item', async () => {
			answerPicker(DESTINATION.unique);
			newAncestors = [FOLDER, DESTINATION, ITEM];

			await executeMove();

			expect(events).to.deep.include({ type: UmbRequestReloadChildrenOfEntityEvent.TYPE, entity: DESTINATION });
			expect(events).to.deep.include({ type: UmbRequestReloadStructureForEntityEvent.TYPE, entity: DESTINATION });
		});

		it('reloads only the children of the tree root when moved to the root', async () => {
			answerPicker(null);
			newAncestors = [ITEM];

			await executeMove();

			expect(events).to.deep.equal([
				{ type: UmbRequestReloadStructureForEntityEvent.TYPE, entity: ITEM },
				{ type: UmbRequestReloadChildrenOfEntityEvent.TYPE, entity: ROOT },
			]);
		});

		it('resolves the destination before dispatching any reload', async () => {
			answerPicker(DESTINATION.unique);
			newAncestors = [FOLDER, DESTINATION, ITEM];

			await executeMove();

			const moveIndex = calls.indexOf('requestMoveTo');
			const lookupIndex = calls.indexOf('requestTreeItemAncestors', moveIndex);
			const firstEventIndex = calls.findIndex((call) => call.startsWith('request-reload'));
			expect(lookupIndex).to.be.greaterThan(moveIndex);
			expect(lookupIndex).to.be.lessThan(firstEventIndex);
		});

		it('uses a single tree repository and destroys it once done', async () => {
			answerPicker(DESTINATION.unique);
			newAncestors = [FOLDER, DESTINATION, ITEM];

			await executeMove();

			expect(treeRepositories).to.have.length(1);
			expect(treeRepositories[0].destroyed).to.be.true;
		});

		it('destroys the tree repository when the picker is cancelled', async () => {
			answerPicker(undefined);

			await executeMove();

			expect(treeRepositories).to.have.length(1);
			expect(treeRepositories[0].destroyed).to.be.true;
		});

		it('still reloads the moved item when the destination cannot be resolved', async () => {
			answerPicker(DESTINATION.unique);
			newAncestors = undefined;
			ancestorsError = new Error('Ancestors could not be loaded');

			await executeMove();

			expect(events).to.deep.equal([{ type: UmbRequestReloadStructureForEntityEvent.TYPE, entity: ITEM }]);
		});
	});
});
