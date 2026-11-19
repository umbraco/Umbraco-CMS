import { UmbCollectionWorkspaceViewInteractionMemoryController } from './collection-workspace-view-interaction-memory.controller.js';
import { aTimeout, expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbEntityContext } from '@umbraco-cms/backoffice/entity';
import { UmbInteractionMemoryContext } from '@umbraco-cms/backoffice/interaction-memory';
import type { UmbInteractionMemoryModel } from '@umbraco-cms/backoffice/interaction-memory';

const FILTER_MEMORY_UNIQUE = 'UmbCollectionFilter';

@customElement('test-collection-workspace-view-interaction-memory-host')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

const WORKSPACE_VIEW_ALIAS = 'Umb.WorkspaceView.Test.Collection';
const COLLECTION_ALIAS = 'Umb.Collection.Test';
const ENTITY_UNIQUE = 'entity-1';
const OTHER_ENTITY_UNIQUE = 'entity-2';
const MEMORY_UNIQUE = `${WORKSPACE_VIEW_ALIAS}:${COLLECTION_ALIAS}:${ENTITY_UNIQUE}`;
const OTHER_MEMORY_UNIQUE = `${WORKSPACE_VIEW_ALIAS}:${COLLECTION_ALIAS}:${OTHER_ENTITY_UNIQUE}`;

const filterMemory: UmbInteractionMemoryModel = {
	unique: FILTER_MEMORY_UNIQUE,
	value: { filter: 'news' },
};

describe('UmbCollectionWorkspaceViewInteractionMemoryController', () => {
	let hostElement: UmbTestControllerHostElement;
	let globalInteractionMemoryContext: UmbInteractionMemoryContext;
	let entityContext: UmbEntityContext;
	let controller: UmbCollectionWorkspaceViewInteractionMemoryController;
	let observed: Array<Array<UmbInteractionMemoryModel> | undefined>;

	const setupController = async () => {
		controller = new UmbCollectionWorkspaceViewInteractionMemoryController(hostElement);
		controller.observe(controller.memories, (memories) => observed.push(memories), 'testMemoriesObserver');
		controller.setWorkspaceViewAlias(WORKSPACE_VIEW_ALIAS);
		controller.setCollectionAlias(COLLECTION_ALIAS);
		await aTimeout(0);
	};

	beforeEach(() => {
		hostElement = new UmbTestControllerHostElement();
		document.body.appendChild(hostElement);
		globalInteractionMemoryContext = new UmbInteractionMemoryContext(hostElement);
		entityContext = new UmbEntityContext(hostElement);
		entityContext.setEntityType('test-entity-type');
		entityContext.setUnique(ENTITY_UNIQUE);
		observed = [];
	});

	afterEach(() => {
		hostElement.remove();
	});

	describe('writeInteractionMemory()', () => {
		it('stores the memories under the workspace view, collection and entity', async () => {
			await setupController();

			controller.writeInteractionMemory([filterMemory]);

			expect(globalInteractionMemoryContext.memory.getMemory(MEMORY_UNIQUE)?.memories).to.eql([filterMemory]);
		});

		it('stores the memories of another entity on its own', async () => {
			await setupController();
			controller.writeInteractionMemory([filterMemory]);

			entityContext.setUnique(OTHER_ENTITY_UNIQUE);
			controller.writeInteractionMemory([filterMemory]);

			expect(globalInteractionMemoryContext.memory.getMemory(MEMORY_UNIQUE)?.memories).to.eql([filterMemory]);
			expect(globalInteractionMemoryContext.memory.getMemory(OTHER_MEMORY_UNIQUE)?.memories).to.eql([filterMemory]);
		});

		it('deletes the memory when there is nothing left to remember', async () => {
			await setupController();
			controller.writeInteractionMemory([filterMemory]);

			controller.writeInteractionMemory([]);

			expect(globalInteractionMemoryContext.memory.getMemory(MEMORY_UNIQUE)).to.be.undefined;
		});

		it('does not store anything before the scope is known', async () => {
			controller = new UmbCollectionWorkspaceViewInteractionMemoryController(hostElement);
			controller.setWorkspaceViewAlias(WORKSPACE_VIEW_ALIAS);
			await aTimeout(0);

			controller.writeInteractionMemory([filterMemory]);

			expect(globalInteractionMemoryContext.memory.getAllMemories()).to.eql([]);
		});
	});

	describe('memories', () => {
		it('emits undefined when there is nothing to restore', async () => {
			await setupController();

			expect(observed).to.eql([undefined]);
		});

		it('emits the memories of the current entity', async () => {
			globalInteractionMemoryContext.memory.setMemory({ unique: MEMORY_UNIQUE, memories: [filterMemory] });

			await setupController();

			expect(observed.at(-1)).to.eql([filterMemory]);
		});

		it('does not emit the memories of another entity', async () => {
			globalInteractionMemoryContext.memory.setMemory({ unique: OTHER_MEMORY_UNIQUE, memories: [filterMemory] });

			await setupController();

			expect(observed).to.eql([undefined]);
		});

		it('emits an empty set when the entity changes to one without memories', async () => {
			await setupController();
			controller.writeInteractionMemory([filterMemory]);

			entityContext.setUnique(OTHER_ENTITY_UNIQUE);
			await aTimeout(0);

			expect(observed.at(-1)).to.eql([]);
		});
	});
});
