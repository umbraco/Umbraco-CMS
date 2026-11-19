import { UmbInteractionMemoriesChangeEvent } from './event/interaction-memories-change.event.js';
import type { UmbInteractionMemoryManager } from './interaction-memory.manager.js';
import type { UmbInteractionMemoryModel } from './types.js';
import { jsonStringComparison } from '@umbraco-cms/backoffice/observable-api';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 * Bridges an interaction memory manager to the element hosting it: memories are given to the manager through
 * {@link setMemories} — mirroring an `interactionMemories` element property — and every change made to the manager
 * dispatches an `interaction-memories-change` event on the host element.
 *
 * Use this in an element that hands its interaction memories to whoever is responsible for storing them, ex. a
 * workspace view writing them to the global interaction memory.
 * @exports
 * @class UmbElementInteractionMemoryBridgeController
 * @augments {UmbControllerBase}
 */
export class UmbElementInteractionMemoryBridgeController extends UmbControllerBase {
	#interactionMemory: UmbInteractionMemoryManager;
	#snapshot: Array<UmbInteractionMemoryModel> = [];

	/**
	 * Creates an instance of UmbElementInteractionMemoryBridgeController.
	 * @param {UmbControllerHost} host - The host of this controller; the change event is dispatched on its element.
	 * @param {UmbInteractionMemoryManager} interactionMemory - The interaction memory manager to bridge.
	 * @memberof UmbElementInteractionMemoryBridgeController
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
			null,
		);
	}

	/**
	 * Gets all interaction memories currently held by the bridged manager.
	 * @returns {Array<UmbInteractionMemoryModel>} The current interaction memories.
	 * @memberof UmbElementInteractionMemoryBridgeController
	 */
	getMemories(): Array<UmbInteractionMemoryModel> {
		return this.#interactionMemory.getAllMemories();
	}

	/**
	 * Syncs the bridged manager to the provided snapshot. The incoming array is authoritative: memories no longer
	 * present are removed, the rest are added or updated. Applied in one write, so restoring a snapshot never
	 * dispatches a change event for the memories the element was just handed, nor for the intermediate states of
	 * applying them one by one.
	 * @param {Array<UmbInteractionMemoryModel> | undefined} value - The authoritative snapshot of interaction memories.
	 * @memberof UmbElementInteractionMemoryBridgeController
	 */
	setMemories(value: Array<UmbInteractionMemoryModel> | undefined): void {
		const next = value ?? [];
		this.#snapshot = next;
		this.#interactionMemory.setMemories(next);
	}
}
