import type { UmbPropertyEditorConfigCollection } from '../config/index.js';
import { UmbPropertyEditorUiInteractionMemoryManager } from './property-editor-ui-interaction-memory.manager.js';
import { jsonStringComparison } from '@umbraco-cms/backoffice/observable-api';
import {
	UmbInteractionMemoryScopeContext,
	type UmbInteractionMemoryModel,
} from '@umbraco-cms/backoffice/interaction-memory';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 * A picker-memory scope for a property editor, backed by the property editor's own
 * config-hashed interaction memory (see `UmbPropertyEditorUiInteractionMemoryManager`) rather than
 * a picker input's selection. Lets a property editor that opens picker modals without a picker
 * input in between — e.g. an RTE opening its media/link pickers — remember each picker's state
 * across re-opens, scoped to this property editor and its configuration.
 * @exports
 * @class UmbPropertyEditorUiPickerMemoryContext
 * @augments {UmbInteractionMemoryScopeContext}
 */
export class UmbPropertyEditorUiPickerMemoryContext extends UmbInteractionMemoryScopeContext {
	#memoryManager: UmbPropertyEditorUiInteractionMemoryManager;
	#snapshot: Array<UmbInteractionMemoryModel> = [];

	constructor(host: UmbControllerHost, args: { memoryUniquePrefix: string }) {
		super(host);

		this.#memoryManager = new UmbPropertyEditorUiInteractionMemoryManager(host, args);

		this.observe(this.#memoryManager.memoriesForPropertyEditor, (memories) => {
			if (jsonStringComparison(memories, this.#snapshot)) return;
			this.#snapshot = memories;
			this.#applyMemories(memories);
		});

		this.observe(this.interactionMemory.memories, (memories) => {
			if (jsonStringComparison(memories, this.#snapshot)) return;
			this.#snapshot = memories;

			if (memories.length > 0) {
				this.#memoryManager.saveMemoriesForPropertyEditor(memories);
			} else {
				this.#memoryManager.deleteMemoriesForPropertyEditor();
			}
		});
	}

	/**
	 * Sets the property editor config, used to create a unique hash for the interaction memory.
	 * @param {(UmbPropertyEditorConfigCollection | undefined)} config - The property editor's configuration.
	 * @memberof UmbPropertyEditorUiPickerMemoryContext
	 */
	setPropertyEditorConfig(config: UmbPropertyEditorConfigCollection | undefined) {
		this.#memoryManager.setPropertyEditorConfig(config);
	}

	#applyMemories(next: Array<UmbInteractionMemoryModel>) {
		const current = this.interactionMemory.getAllMemories();
		const nextUniques = new Set(next.map((memory) => memory.unique));

		current
			.filter((memory) => !nextUniques.has(memory.unique))
			.forEach((memory) => this.interactionMemory.deleteMemory(memory.unique));

		next.forEach((memory) => this.interactionMemory.setMemory(memory));
	}
}
