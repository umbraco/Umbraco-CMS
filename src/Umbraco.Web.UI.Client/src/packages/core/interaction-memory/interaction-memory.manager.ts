import type { UmbInteractionMemoryModel } from './types.js';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { Observable } from '@umbraco-cms/backoffice/observable-api';

/**
 * A manager for handling interaction memory items.
 * @exports
 * @class UmbInteractionMemoryManager
 * @augments {UmbControllerBase}
 */
export class UmbInteractionMemoryManager extends UmbControllerBase {
	#memories = new UmbArrayState<UmbInteractionMemoryModel>([], (x) => x.unique);
	/** Observable for all memory items. */
	memories = this.#memories.asObservable();

	/**
	 * Observable for a specific memory item by its unique identifier.
	 * @param {string} unique - The unique identifier of the memory item.
	 * @returns {(Observable<UmbInteractionMemoryModel | undefined>)} An observable that emits the memory item or undefined if not found.
	 * @memberof UmbInteractionMemoryManager
	 */
	memory(unique: string): Observable<UmbInteractionMemoryModel | undefined> {
		return this.#memories.asObservablePart((items) => items.find((item) => item.unique === unique));
	}

	/**
	 * Get a specific memory item by its unique identifier.
	 * @param {string} unique - The unique identifier of the memory item.
	 * @returns {(UmbInteractionMemoryModel | undefined)} The memory item or undefined if not found.
	 * @memberof UmbInteractionMemoryManager
	 */
	getMemory(unique: string): UmbInteractionMemoryModel | undefined {
		return this.#memories.getValue().find((item) => item.unique === unique);
	}

	/**
	 * Add or update a memory item.
	 * @param {UmbInteractionMemoryModel} memory - The memory item to add or update.
	 * @memberof UmbInteractionMemoryManager
	 */
	setMemory(memory: UmbInteractionMemoryModel) {
		this.#memories.appendOne(memory);
	}

	/**
	 * Delete a memory item by its unique identifier.
	 * @param {string} unique - The unique identifier of the memory item.
	 * @memberof UmbInteractionMemoryManager
	 */
	deleteMemory(unique: string) {
		this.#memories.removeOne(unique);
	}

	/**
	 * Get all memory items from the manager.
	 * @returns {Array<UmbInteractionMemoryModel>} An array of all memory items.
	 * @memberof UmbInteractionMemoryManager
	 */
	getAllMemories(): Array<UmbInteractionMemoryModel> {
		return this.#memories.getValue();
	}

	/**
	 * Clear all memory items from the manager.
	 * @memberof UmbInteractionMemoryManager
	 */
	clear() {
		this.#memories.clear();
	}
}
