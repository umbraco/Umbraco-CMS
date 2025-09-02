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
}
