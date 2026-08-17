import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UMB_ENTITY_CONTEXT } from '@umbraco-cms/backoffice/entity';
import { UMB_INTERACTION_MEMORY_CONTEXT } from '@umbraco-cms/backoffice/interaction-memory';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbInteractionMemoryModel } from '@umbraco-cms/backoffice/interaction-memory';

/**
 * Reads and writes the interaction memories of a collection hosted by a workspace view, storing them per workspace
 * view, collection and entity.
 * @exports
 * @class UmbCollectionWorkspaceViewInteractionMemoryController
 * @augments {UmbControllerBase}
 */
export class UmbCollectionWorkspaceViewInteractionMemoryController extends UmbControllerBase {
	readonly #memories = new UmbArrayState<UmbInteractionMemoryModel, string, undefined>(
		undefined,
		(memory) => memory.unique,
	);
	public readonly memories = this.#memories.asObservable();

	#globalInteractionMemoryContext?: typeof UMB_INTERACTION_MEMORY_CONTEXT.TYPE;
	#workspaceViewAlias?: string;
	#collectionAlias?: string;
	#entityKey?: string;
	#collectionHasMemories = false;

	constructor(host: UmbControllerHost) {
		super(host);

		this.consumeContext(UMB_INTERACTION_MEMORY_CONTEXT, (context) => {
			this.#globalInteractionMemoryContext = context;
			this.#readInteractionMemory();
		});

		this.consumeContext(UMB_ENTITY_CONTEXT, (context) => {
			this.observe(
				context?.unique,
				(unique) => {
					const entityKey = unique === undefined ? undefined : String(unique);
					if (this.#entityKey === entityKey) return;
					this.#entityKey = entityKey;
					this.#readInteractionMemory();
				},
				'umbCollectionInteractionMemoryEntityObserver',
			);
		});
	}

	/**
	 * Sets the alias of the workspace view hosting the collection.
	 * @param {(string | undefined)} alias - The alias of the workspace view.
	 * @memberof UmbCollectionWorkspaceViewInteractionMemoryController
	 */
	setWorkspaceViewAlias(alias: string | undefined) {
		if (this.#workspaceViewAlias === alias) return;
		this.#workspaceViewAlias = alias;
		this.#readInteractionMemory();
	}

	/**
	 * Sets the alias of the collection the memories belong to.
	 * @param {(string | undefined)} alias - The alias of the collection.
	 * @memberof UmbCollectionWorkspaceViewInteractionMemoryController
	 */
	setCollectionAlias(alias: string | undefined) {
		if (this.#collectionAlias === alias) return;
		this.#collectionAlias = alias;
		this.#readInteractionMemory();
	}

	/**
	 * Stores the memories reported by the collection.
	 * @param {Array<UmbInteractionMemoryModel>} memories - The memories of the collection.
	 * @memberof UmbCollectionWorkspaceViewInteractionMemoryController
	 */
	writeInteractionMemory(memories: Array<UmbInteractionMemoryModel>) {
		const unique = this.#getInteractionMemoryUnique();
		if (!unique || !this.#globalInteractionMemoryContext) return;

		this.#collectionHasMemories = memories.length > 0;

		if (memories.length > 0) {
			this.#globalInteractionMemoryContext.memory.setMemory({ unique, memories });
		} else {
			this.#globalInteractionMemoryContext.memory.deleteMemory(unique);
		}
	}

	#getInteractionMemoryUnique(): string | undefined {
		if (!this.#workspaceViewAlias || !this.#collectionAlias || !this.#entityKey) return undefined;
		return `${this.#workspaceViewAlias}:${this.#collectionAlias}:${this.#entityKey}`;
	}

	#readInteractionMemory() {
		const unique = this.#getInteractionMemoryUnique();
		if (!unique || !this.#globalInteractionMemoryContext) return;

		const memories = this.#globalInteractionMemoryContext.memory.getMemory(unique)?.memories ?? [];

		// Nothing to restore and nothing to clear.
		if (memories.length === 0 && !this.#collectionHasMemories) return;

		this.#collectionHasMemories = memories.length > 0;
		this.#memories.setValue(memories);
	}
}
