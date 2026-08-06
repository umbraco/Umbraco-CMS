// Keep internal
interface UmbDetailCacheEntryModel<DetailDataModelType> {
	id: string;
	data: DetailDataModelType;
	timestamp: string;
}

/**
 * A runtime cache for storing entity detail data from the Management Api
 * @class UmbManagementApiDetailDataCache
 * @template DetailDataModelType
 */
export class UmbManagementApiDetailDataCache<DetailDataModelType> {
	#entries: Map<string, UmbDetailCacheEntryModel<DetailDataModelType>> = new Map();
	#serverEventsConnected = false;

	/**
	 * Checks if an entry exists in the cache
	 * @param {string} id - The ID of the entry to check
	 * @returns {boolean} - True if the entry exists, false otherwise
	 * @memberof UmbManagementApiDetailDataCache
	 */
	has(id: string): boolean {
		return this.#entries.has(id);
	}

	/**
	 * Adds an entry to the cache
	 * @param {string} id - The ID of the entry to add
	 * @param {DetailDataModelType} data - The data to cache
	 * @memberof UmbManagementApiDetailDataCache
	 */
	set(id: string, data: DetailDataModelType): void {
		const cacheEntry: UmbDetailCacheEntryModel<DetailDataModelType> = {
			id: id,
			data,
			timestamp: new Date().toISOString(),
		};

		this.#entries.set(id, cacheEntry);
	}

	/**
	 * Retrieves an entry from the cache
	 * @param {string} id - The ID of the entry to retrieve
	 * @returns {DetailDataModelType | undefined} - The cached entry or undefined if not found
	 * @memberof UmbManagementApiDetailDataCache
	 */
	get(id: string): DetailDataModelType | undefined {
		const entry = this.#entries.get(id);
		return entry ? entry.data : undefined;
	}

	/**
	 * Retrieves all entries from the cache
	 * @returns {Array<DetailDataModelType>} - An array of all cached entries
	 * @memberof UmbManagementApiItemDataCache
	 */
	getAll(): Array<DetailDataModelType> {
		return Array.from(this.#entries.values()).map((entry) => entry.data);
	}

	/**
	 * Deletes an entry from the cache
	 * @param {string} id - The ID of the entry to delete
	 * @memberof UmbManagementApiDetailDataCache
	 */
	delete(id: string): void {
		this.#entries.delete(id);
	}

	/**
	 * Clears all entries from the cache
	 * @memberof UmbManagementApiDetailDataCache
	 */
	clear(): void {
		this.#entries.clear();
	}

	/**
	 * Whether server events are connected, which is the condition under which cached entries
	 * can be trusted — server events are what invalidates them.
	 * This lives on the cache rather than on each consumer because the cache is shared: a
	 * consumer constructed later inherits the known state instead of re-deriving it from an
	 * asynchronous context lookup, during which it would otherwise bypass the cache.
	 * @returns {boolean} - True if server events are connected, false otherwise
	 * @memberof UmbManagementApiDetailDataCache
	 */
	get serverEventsConnected(): boolean {
		return this.#serverEventsConnected;
	}

	/**
	 * Sets whether server events are connected, clearing the cache when they are not
	 * @param {boolean} value - True if server events are connected, false otherwise
	 * @memberof UmbManagementApiDetailDataCache
	 */
	setServerEventsConnected(value: boolean): void {
		this.#serverEventsConnected = value;

		if (value === false) {
			this.clear();
		}
	}
}
