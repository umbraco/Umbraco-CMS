import type { UmbPropertyEditorConfigCollection } from '../config/index.js';
import { simpleHashCode, UmbArrayState } from '@umbraco-cms/backoffice/observable-api';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UMB_INTERACTION_MEMORY_CONTEXT } from '@umbraco-cms/backoffice/interaction-memory';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbInteractionMemoryModel } from '@umbraco-cms/backoffice/interaction-memory';

export interface UmbPropertyEditorUiInteractionMemoryManagerArgs {
	memoryUniquePrefix: string;
}

/**
 * Persists a property editor's interaction memories to the app-root interaction-memory store, keyed by
 * a prefix plus a hash of the property editor's configuration, so they survive the property editor
 * being recreated.
 *
 * This is the only layer that talks to that store. It is deliberately not an
 * `UmbInteractionMemoryManager`: a modal's memories reach it by being relayed up from the scope the
 * opener provides, never by a modal writing into it directly.
 * @exports
 * @class UmbPropertyEditorUiInteractionMemoryManager
 * @augments {UmbControllerBase}
 */
export class UmbPropertyEditorUiInteractionMemoryManager extends UmbControllerBase {
	#memories = new UmbArrayState<UmbInteractionMemoryModel>([], (x) => x.unique);
	memoriesForPropertyEditor = this.#memories.asObservable();

	#interactionMemoryContext?: typeof UMB_INTERACTION_MEMORY_CONTEXT.TYPE;
	#configHashCode?: number;
	#memoryUniquePrefix: string;
	#init?: Promise<unknown>;
	#warnedAboutMissingConfig = false;

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

	/**
	 * Sets the property editor config, used to create a unique hash for the interaction memory.
	 * @param {(UmbPropertyEditorConfigCollection | undefined)} config - The property editor configuration.
	 * @memberof UmbPropertyEditorUiInteractionMemoryManager
	 */
	setPropertyEditorConfig(config: UmbPropertyEditorConfigCollection | undefined) {
		this.#setConfigHash(config);
		this.#observeInteractionMemory();
	}

	/**
	 * Creates or updates an interaction memory for this property editor based on the provided memories.
	 * @param {Array<UmbInteractionMemoryModel>} memories - The memories to include for this property editor.
	 * @returns {Promise<void>} Resolves once the memories have been applied.
	 * @memberof UmbPropertyEditorUiInteractionMemoryManager
	 */
	async saveMemoriesForPropertyEditor(memories: Array<UmbInteractionMemoryModel>): Promise<void> {
		await this.#init;
		if (!this.#hasConfigHash()) return;

		this.#memories.setValue(memories);
		this.#interactionMemoryContext?.memory.setMemory({ unique: this.#getInteractionMemoryUnique(), memories });
	}

	/**
	 * Deletes the interaction memory for this property editor.
	 * @returns {Promise<void>} Resolves once the memories have been removed.
	 * @memberof UmbPropertyEditorUiInteractionMemoryManager
	 */
	async deleteMemoriesForPropertyEditor(): Promise<void> {
		await this.#init;
		if (!this.#hasConfigHash()) return;

		this.#memories.setValue([]);
		this.#interactionMemoryContext?.memory.deleteMemory(this.#getInteractionMemoryUnique());
	}

	#hasConfigHash(): boolean {
		if (this.#configHashCode !== undefined) return true;

		// Without a hash there is no key to store under, so the memories would be silently dropped. It
		// means `setPropertyEditorConfig` was never called — it has to be called unconditionally, before
		// any `if (!config) return` guard in the property editor.
		if (!this.#warnedAboutMissingConfig) {
			this.#warnedAboutMissingConfig = true;
			console.warn(
				`[${this.#memoryUniquePrefix}] Interaction memories cannot be persisted because no property editor configuration has been set. Call setPropertyEditorConfig() before any early return in the property editor's config setter.`,
			);
		}
		return false;
	}

	#getInteractionMemoryUnique() {
		return `${this.#memoryUniquePrefix}PropertyEditorUi${this.#configHashCode !== undefined ? '-' + this.#configHashCode : ''}`;
	}

	#observeInteractionMemory() {
		if (!this.#interactionMemoryContext || this.#configHashCode === undefined) return;
		this.observe(
			this.#interactionMemoryContext.memory.memory(this.#getInteractionMemoryUnique()),
			(memory) => {
				this.#memories.setValue(memory?.memories ?? []);
			},
			'observeMemory',
		);
	}

	#setConfigHash(config: UmbPropertyEditorConfigCollection | undefined) {
		const configString = config ? JSON.stringify(config.toObject()) : '';
		this.#configHashCode = simpleHashCode(configString);
	}
}
