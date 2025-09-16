import type { UmbPropertyEditorConfigCollection } from '../config/index.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import {
	UMB_INTERACTION_MEMORY_CONTEXT,
	type UmbInteractionMemoryModel,
} from '@umbraco-cms/backoffice/interaction-memory';
import { simpleHashCode, UmbArrayState } from '@umbraco-cms/backoffice/observable-api';

export interface UmbPropertyEditorUiInteractionMemoryManagerArgs {
	memoryUniquePrefix: string;
}

export class UmbPropertyEditorUiInteractionMemoryManager extends UmbControllerBase {
	#memories = new UmbArrayState<UmbInteractionMemoryModel>([], (x) => x.unique);
	memories = this.#memories.asObservable();

	#interactionMemoryContext?: typeof UMB_INTERACTION_MEMORY_CONTEXT.TYPE;
	#configHashCode?: number;
	#memoryUniquePrefix: string;

	constructor(host: UmbControllerHost, args: UmbPropertyEditorUiInteractionMemoryManagerArgs) {
		super(host);

		this.#memoryUniquePrefix = args.memoryUniquePrefix;

		this.consumeContext(UMB_INTERACTION_MEMORY_CONTEXT, (context) => {
			this.#interactionMemoryContext = context;
			this.#getInteractionMemory();
		});
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
	 * @returns {void}
	 * @memberof UmbPropertyEditorUiInteractionMemoryManager
	 */
	setMemory(memories: Array<UmbInteractionMemoryModel>): void {
		const memoryUnique = this.#getInteractionMemoryUnique();
		if (!memoryUnique) return;
		if (!this.#interactionMemoryContext) return;

		const propertyEditorMemory: UmbInteractionMemoryModel = {
			unique: memoryUnique,
			memories,
		};

		this.#interactionMemoryContext.memory.setMemory(propertyEditorMemory);
	}

	/**
	 * Deletes the interaction memory for this property editor.
	 * @memberof UmbPropertyEditorUiInteractionMemoryManager
	 */
	deleteMemory() {
		const unique = this.#getInteractionMemoryUnique();
		if (!unique) {
			throw new Error('Memory unique is missing');
		}
		this.#interactionMemoryContext?.memory.deleteMemory(unique);
	}

	#getInteractionMemoryUnique() {
		return `${this.#memoryUniquePrefix + this.#configHashCode ? '-' + this.#configHashCode : ''}`;
	}

	#getInteractionMemory() {
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
