// Keep internal
interface UmbCacheEntryModel<ItemModelType> {
	id: string;
	data: ItemModelType;
}

/**
 * A runtime cache for storing entity item data from the Management Api
 * @class UmbManagementApiItemDataCache
 * @template ItemModelType
 */
export class UmbManagementApiItemDataCache<ItemModelType> {
	#entries: Map<string, UmbCacheEntryModel<ItemModelType>> = new Map();

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
	 * @param {ItemModelType} data - The data to cache
	 * @memberof UmbManagementApiItemDataCache
	 */
	set(id: string, data: ItemModelType): void {
		const cacheEntry: UmbCacheEntryModel<ItemModelType> = {
			id: id,
			data,
		};

		this.#entries.set(id, cacheEntry);
	}

	/**
	 * Retrieves an entry from the cache
	 * @param {string} id - The ID of the entry to retrieve
	 * @returns {ItemModelType | undefined} - The cached entry or undefined if not found
	 * @memberof UmbManagementApiItemDataCache
	 */
	get(id: string): ItemModelType | undefined {
		const entry = this.#entries.get(id);
		return entry ? entry.data : undefined;
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
