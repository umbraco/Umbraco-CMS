import type { UmbPropertyEditorConfigCollection } from '../config/index.js';
import { simpleHashCode, UmbArrayState } from '@umbraco-cms/backoffice/observable-api';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UMB_INTERACTION_MEMORY_CONTEXT } from '@umbraco-cms/backoffice/interaction-memory';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbInteractionMemoryModel } from '@umbraco-cms/backoffice/interaction-memory';

export interface UmbPropertyEditorUiInteractionMemoryManagerArgs {
	memoryUniquePrefix: string;
}

export class UmbPropertyEditorUiInteractionMemoryManager extends UmbControllerBase {
	#memories = new UmbArrayState<UmbInteractionMemoryModel>([], (x) => x.unique);
	memoriesForPropertyEditor = this.#memories.asObservable();

	#interactionMemoryContext?: typeof UMB_INTERACTION_MEMORY_CONTEXT.TYPE;
	#configHashCode?: number;
	#memoryUniquePrefix: string;
	#init?: Promise<unknown>;

	constructor(host: UmbControllerHost, args: UmbPropertyEditorUiInteractionMemoryManagerArgs) {
		super(host);

		this.#memoryUniquePrefix = args.memoryUniquePrefix;

		this.#init = Promise.all([
			this.consumeContext(UMB_INTERACTION_MEMORY_CONTEXT, (context) => {
				this.#interactionMemoryContext = context;
			}).asPromise(),
		]);
	}

	/**
	 * Sets the property editor config, used to create a unique hash for the interaction memory.
	 * @param {(UmbPropertyEditorConfigCollection | undefined)} config
	 * @memberof UmbPropertyEditorUiInteractionMemoryManager
	 */
	setPropertyEditorConfig(config: UmbPropertyEditorConfigCollection | undefined) {
		this.#setConfigHash(config);
		this.#getInteractionMemory();
	}

	/**
	 * Creates or updates an interaction memory for this property editor based on the provided memories.
	 * @param {Array<UmbInteractionMemoryModel>} memories - The memories to include for this property editor.
	 * @returns {Promise<void>}
	 * @memberof UmbPropertyEditorUiInteractionMemoryManager
	 */
	async saveMemoriesForPropertyEditor(memories: Array<UmbInteractionMemoryModel>): Promise<void> {
		await this.#init;
		const memoryUnique = this.#getInteractionMemoryUnique();
		if (!this.#interactionMemoryContext) return;

		const propertyEditorMemory: UmbInteractionMemoryModel = {
			unique: memoryUnique,
			memories,
		};

		this.#interactionMemoryContext.memory.setMemory(propertyEditorMemory);
		this.#memories.setValue(memories);
	}

	/**
	 * Deletes the interaction memory for this property editor.
	 * @memberof UmbPropertyEditorUiInteractionMemoryManager
	 */
	async deleteMemoriesForPropertyEditor(): Promise<void> {
		await this.#init;
		const unique = this.#getInteractionMemoryUnique();
		this.#interactionMemoryContext?.memory.deleteMemory(unique);
		this.#memories.setValue([]);
	}

	#getInteractionMemoryUnique() {
		return `${this.#memoryUniquePrefix}PropertyEditorUi${this.#configHashCode ? '-' + this.#configHashCode : ''}`;
	}

	async #getInteractionMemory() {
		await this.#init;
		const memoryUnique = this.#getInteractionMemoryUnique();
		if (!memoryUnique) return;
		if (!this.#interactionMemoryContext) return;

		const memory = this.#interactionMemoryContext.memory.getMemory(memoryUnique);
		this.#memories.setValue(memory?.memories ?? []);
	}

	#setConfigHash(config: UmbPropertyEditorConfigCollection | undefined) {
		const configString = config ? JSON.stringify(config.toObject()) : '';
		const hashCode = simpleHashCode(configString);
		this.#configHashCode = hashCode;
	}
}
