// Keep internal
interface UmbItemCacheEntryModel<ItemDataModelType> {
	id: string;
	data: ItemDataModelType;
	timestamp: string;
}

/**
 * A runtime cache for storing entity item data from the Management Api
 * @class UmbManagementApiItemDataCache
 * @template ItemDataModelType
 */
export class UmbManagementApiItemDataCache<ItemDataModelType> {
	#entries: Map<string, UmbItemCacheEntryModel<ItemDataModelType>> = new Map();
	#serverEventsConnected = false;

	/**
	 * Checks if an entry exists in the cache
	 * @param {string} id - The ID of the entry to check
	 * @returns {boolean} - True if the entry exists, false otherwise
	 * @memberof UmbManagementApiItemDataCache
	 */
	has(id: string): boolean {
		return this.#entries.has(id);
	}

	/**
	 * Adds an entry to the cache
	 * @param {string} id - The ID of the entry to add
	 * @param {ItemDataModelType} data - The data to cache
	 * @memberof UmbManagementApiItemDataCache
	 */
	set(id: string, data: ItemDataModelType): void {
		const cacheEntry: UmbItemCacheEntryModel<ItemDataModelType> = {
			id: id,
			data,
			timestamp: new Date().toISOString(),
		};

		this.#entries.set(id, cacheEntry);
	}

	/**
	 * Retrieves an entry from the cache
	 * @param {string} id - The ID of the entry to retrieve
	 * @returns {ItemDataModelType | undefined} - The cached entry or undefined if not found
	 * @memberof UmbManagementApiItemDataCache
	 */
	get(id: string): ItemDataModelType | undefined {
		const entry = this.#entries.get(id);
		return entry ? entry.data : undefined;
	}

	/**
	 * Retrieves all entries from the cache
	 * @returns {Array<ItemDataModelType>} - An array of all cached entries
	 * @memberof UmbManagementApiItemDataCache
	 */
	getAll(): Array<ItemDataModelType> {
		return Array.from(this.#entries.values()).map((entry) => entry.data);
	}

	/**
	 * Deletes an entry from the cache
	 * @param {string} id - The ID of the entry to delete
	 * @memberof UmbManagementApiItemDataCache
	 */
	delete(id: string): void {
		this.#entries.delete(id);
	}

	/**
	 * Clears all entries from the cache
	 * @memberof UmbManagementApiItemDataCache
	 */
	clear(): void {
		this.#entries.clear();
	}

	/**
	 * Whether server events are connected, which is the condition under which cached entries
	 * can be trusted, as server events are what invalidate them.
	 * The cache's invalidation manager owns this state for the lifetime of the app, so every
	 * consumer reads the same value, including one constructed after the connection changed.
	 * Without an invalidation manager it stays false, and consumers read from the server.
	 * @returns {boolean} - True if server events are connected, false otherwise
	 * @memberof UmbManagementApiItemDataCache
	 */
	get serverEventsConnected(): boolean {
		return this.#serverEventsConnected;
	}

	/**
	 * Sets whether server events are connected, clearing the cache when they are not.
	 * Called by the cache's invalidation manager.
	 * @param {boolean} value - True if server events are connected, false otherwise
	 * @memberof UmbManagementApiItemDataCache
	 */
	setServerEventsConnected(value: boolean): void {
		this.#serverEventsConnected = value;

		if (value === false) {
			this.clear();
		}
	}
}
