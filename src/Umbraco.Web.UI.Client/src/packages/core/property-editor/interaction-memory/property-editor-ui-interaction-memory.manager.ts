import type { UmbPropertyEditorConfigCollection } from '../config/index.js';
import { simpleHashCode } from '@umbraco-cms/backoffice/observable-api';
import {
	UMB_INTERACTION_MEMORY_CONTEXT,
	UmbInteractionMemoryManager,
} from '@umbraco-cms/backoffice/interaction-memory';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbInteractionMemoryModel } from '@umbraco-cms/backoffice/interaction-memory';

export interface UmbPropertyEditorUiInteractionMemoryManagerArgs {
	memoryUniquePrefix: string;
}

/**
 * An `UmbInteractionMemoryManager` that also persists its memories to the app-root interaction-memory
 * store, keyed by a prefix plus a hash of the property editor's configuration. This lets a property
 * editor's own remembered state (e.g. a picker modal's last-navigated folder) survive the property
 * editor being recreated, while still exposing the per-key manager API a picker modal expects from a
 * "nearest scope" (see `UMB_PICKER_INTERACTION_MEMORY_CONTEXT`).
 * @exports
 * @class UmbPropertyEditorUiInteractionMemoryManager
 * @augments {UmbInteractionMemoryManager}
 */
export class UmbPropertyEditorUiInteractionMemoryManager extends UmbInteractionMemoryManager {
	/** Alias of `memories`, kept for existing callers. */
	memoriesForPropertyEditor = this.memories;

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
				this.#observeInteractionMemory();
			}).asPromise(),
		]);
	}

	// These four await `#init` before touching local state — not just before writing to the store.
	// `#observeInteractionMemory` (below) only starts listening once `#init` resolves, and its first
	// emission applies whatever the store currently holds. If a local mutation applied immediately
	// (synchronously) it could land *before* that first emission is applied, which would then
	// clobber it back to empty. Waiting here guarantees the read side is already live before any
	// write can land, so the two can never race — mirroring how `saveMemoriesForPropertyEditor`
	// below has always been gated.

	override async setMemory(memory: UmbInteractionMemoryModel) {
		await this.#init;
		super.setMemory(memory);
		this.#writeToStore();
	}

	override async deleteMemory(unique: string) {
		await this.#init;
		super.deleteMemory(unique);
		this.#writeToStore();
	}

	override async setMemories(memories: Array<UmbInteractionMemoryModel>) {
		await this.#init;
		super.setMemories(memories);
		this.#writeToStore();
	}

	override async clear() {
		await this.#init;
		super.clear();
		this.#writeToStore();
	}

	/**
	 * Sets the property editor config, used to create a unique hash for the interaction memory.
	 * @param {(UmbPropertyEditorConfigCollection | undefined)} config
	 * @memberof UmbPropertyEditorUiInteractionMemoryManager
	 */
	setPropertyEditorConfig(config: UmbPropertyEditorConfigCollection | undefined) {
		this.#setConfigHash(config);
		this.#observeInteractionMemory();
	}

	/**
	 * Creates or updates an interaction memory for this property editor based on the provided memories.
	 * @param {Array<UmbInteractionMemoryModel>} memories - The memories to include for this property editor.
	 * @returns {Promise<void>}
	 * @memberof UmbPropertyEditorUiInteractionMemoryManager
	 */
	saveMemoriesForPropertyEditor(memories: Array<UmbInteractionMemoryModel>): Promise<void> {
		return this.setMemories(memories);
	}

	/**
	 * Deletes the interaction memory for this property editor.
	 * @memberof UmbPropertyEditorUiInteractionMemoryManager
	 */
	deleteMemoriesForPropertyEditor(): Promise<void> {
		return this.clear();
	}

	#getInteractionMemoryUnique() {
		return `${this.#memoryUniquePrefix}PropertyEditorUi${this.#configHashCode ? '-' + this.#configHashCode : ''}`;
	}

	#writeToStore() {
		if (!this.#interactionMemoryContext || !this.#configHashCode) return;

		const memoryUnique = this.#getInteractionMemoryUnique();
		const memories = this.getAllMemories();

		if (memories.length > 0) {
			this.#interactionMemoryContext.memory.setMemory({ unique: memoryUnique, memories });
		} else {
			this.#interactionMemoryContext.memory.deleteMemory(memoryUnique);
		}
	}

	async #observeInteractionMemory() {
		if (!this.#interactionMemoryContext || !this.#configHashCode) return;
		const memoryUnique = this.#getInteractionMemoryUnique();
		if (!memoryUnique) return;
		this.observe(
			this.#interactionMemoryContext?.memory.memory(memoryUnique),
			(memory) => {
				// Apply via the base setter so this doesn't loop back through #persist — the store is
				// already holding this exact value, there's nothing new to write.
				super.setMemories(memory?.memories ?? []);
			},
			'observeMemory',
		);
	}

	#setConfigHash(config: UmbPropertyEditorConfigCollection | undefined) {
		const configString = config ? JSON.stringify(config.toObject()) : '';
		const hashCode = simpleHashCode(configString);
		this.#configHashCode = hashCode;
	}
}
