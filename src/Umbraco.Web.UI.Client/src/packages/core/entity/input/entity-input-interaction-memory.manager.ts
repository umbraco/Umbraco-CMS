import { jsonStringComparison } from '@umbraco-cms/backoffice/observable-api';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import {
	UmbInteractionMemoriesChangeEvent,
	type UmbInteractionMemoryManager,
	type UmbInteractionMemoryModel,
} from '@umbraco-cms/backoffice/interaction-memory';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 * Bridges a picker input's interaction-memory manager to its host element's `interactionMemories`
 * property and `interaction-memories-change` event, keeping the two in sync.
 *
 * Used by entity input elements (e.g. `umb-input-document`, `umb-input-media`, `umb-input-entity-data`)
 * to avoid duplicating the snapshot-sync and change-event logic.
 * @exports
 * @class UmbEntityInputInteractionMemoryManager
 * @augments {UmbControllerBase}
 */
export class UmbEntityInputInteractionMemoryManager extends UmbControllerBase {
	#interactionMemory: UmbInteractionMemoryManager;
	#snapshot: Array<UmbInteractionMemoryModel> = [];

	/**
	 * Creates an instance of UmbEntityInputInteractionMemoryManager.
	 * @param {UmbControllerHost} host - The host element; the change event is dispatched from it.
	 * @param {UmbInteractionMemoryManager} interactionMemory - The picker input context's interaction-memory manager to bridge.
	 * @memberof UmbEntityInputInteractionMemoryManager
	 */
	constructor(host: UmbControllerHost, interactionMemory: UmbInteractionMemoryManager) {
		super(host);
		this.#interactionMemory = interactionMemory;

		this.observe(
			this.#interactionMemory.memories,
			(memories) => {
				// only dispatch the event if the interaction memories have actually changed
				if (jsonStringComparison(memories, this.#snapshot)) return;
				this.#snapshot = memories;
				this.getHostElement().dispatchEvent(new UmbInteractionMemoriesChangeEvent());
			},
			'_observeMemories',
		);
	}

	/**
	 * Gets all interaction memories currently held by the picker input context.
	 * @returns {Array<UmbInteractionMemoryModel>} The current interaction memories.
	 * @memberof UmbEntityInputInteractionMemoryManager
	 */
	getMemories(): Array<UmbInteractionMemoryModel> {
		return this.#interactionMemory.getAllMemories();
	}

	/**
	 * Syncs the picker input context to the provided snapshot. The incoming array is authoritative:
	 * memories no longer present are removed, the rest are added or updated. Short-circuits when the
	 * snapshot already matches to avoid redundant writes and the re-entrant change events they trigger.
	 * @param {Array<UmbInteractionMemoryModel> | undefined} value - The authoritative snapshot of interaction memories.
	 * @memberof UmbEntityInputInteractionMemoryManager
	 */
	setMemories(value: Array<UmbInteractionMemoryModel> | undefined): void {
		const next = value ?? [];
		const current = this.#interactionMemory.getAllMemories();

		this.#snapshot = next;

		if (jsonStringComparison(next, current)) return;

		const nextUniques = new Set(next.map((memory) => memory.unique));
		current
			.filter((memory) => !nextUniques.has(memory.unique))
			.forEach((memory) => this.#interactionMemory.deleteMemory(memory.unique));

		next.forEach((memory) => this.#interactionMemory.setMemory(memory));
	}
}
