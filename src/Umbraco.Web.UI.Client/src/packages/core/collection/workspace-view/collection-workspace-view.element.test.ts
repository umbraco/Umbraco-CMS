import { UmbCollectionWorkspaceViewElement } from './collection-workspace-view.element.js';
import type { ManifestWorkspaceViewCollectionKind } from './types.js';
import type { UmbCollectionElement } from '../collection.element.js';
import { aTimeout, expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbEntityContext } from '@umbraco-cms/backoffice/entity';
import {
	UmbInteractionMemoriesChangeEvent,
	UmbInteractionMemoryContext,
} from '@umbraco-cms/backoffice/interaction-memory';
import type { UmbInteractionMemoryModel } from '@umbraco-cms/backoffice/interaction-memory';

const CURRENT_VIEW_MEMORY_UNIQUE = 'UmbCollectionCurrentView';
const FILTER_MEMORY_UNIQUE = 'UmbCollectionFilter';

@customElement('test-collection-workspace-view-host')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

const WORKSPACE_VIEW_ALIAS = 'Umb.WorkspaceView.Test.Collection';
const COLLECTION_ALIAS = 'Umb.Collection.Test';
const ENTITY_UNIQUE = 'entity-1';
const OTHER_ENTITY_UNIQUE = 'entity-2';
const MEMORY_UNIQUE = `${WORKSPACE_VIEW_ALIAS}:${COLLECTION_ALIAS}:${ENTITY_UNIQUE}`;
const OTHER_MEMORY_UNIQUE = `${WORKSPACE_VIEW_ALIAS}:${COLLECTION_ALIAS}:${OTHER_ENTITY_UNIQUE}`;

const manifest: ManifestWorkspaceViewCollectionKind = {
	type: 'workspaceView',
	kind: 'collection',
	alias: WORKSPACE_VIEW_ALIAS,
	name: 'Test Collection Workspace View',
	meta: {
		label: 'Collection',
		pathname: 'collection',
		icon: 'icon-layers',
		collectionAlias: COLLECTION_ALIAS,
	},
};

const currentViewMemory: UmbInteractionMemoryModel = {
	unique: CURRENT_VIEW_MEMORY_UNIQUE,
	value: { alias: 'Umb.CollectionView.Test' },
};

const filterMemory: UmbInteractionMemoryModel = {
	unique: FILTER_MEMORY_UNIQUE,
	value: { filter: 'news' },
};

describe('UmbCollectionWorkspaceViewElement interaction memory', () => {
	let hostElement: UmbTestControllerHostElement;
	let interactionMemoryContext: UmbInteractionMemoryContext;
	let entityContext: UmbEntityContext;
	let element: UmbCollectionWorkspaceViewElement;

	const getStoredMemories = (unique = MEMORY_UNIQUE) => interactionMemoryContext.memory.getMemory(unique)?.memories;

	const getCollectionElement = () => element.shadowRoot?.querySelector('umb-collection') as UmbCollectionElement | null;

	const reportMemories = async (memories: Array<UmbInteractionMemoryModel>) => {
		const collection = getCollectionElement();
		if (!collection) throw new Error('The collection element was not rendered.');
		collection.getInteractionMemories = () => memories;
		collection.dispatchEvent(new UmbInteractionMemoriesChangeEvent());
		await aTimeout(0);
	};

	const setupElement = async () => {
		element = new UmbCollectionWorkspaceViewElement();
		element.manifest = manifest;
		hostElement.appendChild(element);
		await aTimeout(50);
	};

	beforeEach(() => {
		hostElement = new UmbTestControllerHostElement();
		document.body.appendChild(hostElement);
		interactionMemoryContext = new UmbInteractionMemoryContext(hostElement);
		entityContext = new UmbEntityContext(hostElement);
		entityContext.setEntityType('test-entity-type');
		entityContext.setUnique(ENTITY_UNIQUE);
	});

	afterEach(() => {
		hostElement.remove();
	});

	it('stores the memories reported by the collection under the workspace view, collection and current entity', async () => {
		await setupElement();

		await reportMemories([currentViewMemory, filterMemory]);

		expect(getStoredMemories()).to.eql([currentViewMemory, filterMemory]);
	});

	it('stores the memories of another entity on its own', async () => {
		await setupElement();
		await reportMemories([filterMemory]);

		entityContext.setUnique(OTHER_ENTITY_UNIQUE);
		await reportMemories([currentViewMemory]);

		expect(getStoredMemories()).to.eql([filterMemory]);
		expect(getStoredMemories(OTHER_MEMORY_UNIQUE)).to.eql([currentViewMemory]);
	});

	it('deletes the memory when there is nothing left to remember', async () => {
		await setupElement();
		await reportMemories([filterMemory]);

		await reportMemories([]);

		expect(interactionMemoryContext.memory.getMemory(MEMORY_UNIQUE)).to.be.undefined;
	});

	it('gives the memories of the current entity to the collection', async () => {
		interactionMemoryContext.memory.setMemory({
			unique: MEMORY_UNIQUE,
			memories: [currentViewMemory, filterMemory],
		});

		await setupElement();

		expect(getCollectionElement()?.interactionMemories).to.eql([currentViewMemory, filterMemory]);
	});

	it('does not give the memories of the other entities to the collection', async () => {
		interactionMemoryContext.memory.setMemory({
			unique: OTHER_MEMORY_UNIQUE,
			memories: [filterMemory],
		});

		await setupElement();

		expect(getCollectionElement()?.interactionMemories).to.be.undefined;
	});

	it('clears the memories of the collection when the entity changes to one without memories', async () => {
		await setupElement();
		await reportMemories([filterMemory]);

		entityContext.setUnique(OTHER_ENTITY_UNIQUE);
		await aTimeout(0);

		expect(getCollectionElement()?.interactionMemories).to.eql([]);
	});
});
