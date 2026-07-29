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

	// This guard is NOT a redundant re-emit check (states already dedupe that on their own) — it's a
	// reentrancy breaker. `#memoryManager`'s save/delete methods cross an `await` boundary and echo
	// back through a third, external store, so a mutation that lands mid-round-trip (e.g. from a
	// sibling picker modal) sees a genuinely different value at each hop even though the two
	// observers below are chasing the same logical update. Without this, that produces an
	// oscillating loop rather than a converging one.
	#snapshot: Array<UmbInteractionMemoryModel> = [];

	constructor(host: UmbControllerHost, args: { memoryUniquePrefix: string }) {
		super(host);

		this.#memoryManager = new UmbPropertyEditorUiInteractionMemoryManager(this, args);

		this.observe(this.#memoryManager.memoriesForPropertyEditor, (memories) => {
			if (jsonStringComparison(memories, this.#snapshot)) return;
			this.#snapshot = memories;
			this.interactionMemory.setMemories(memories);
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
}
