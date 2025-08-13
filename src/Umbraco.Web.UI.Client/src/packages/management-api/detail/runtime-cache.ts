// Keep internal
interface UmbCacheEntryModel<DataModelType> {
	id: string;
	data: DataModelType;
}

/**
 * A runtime cache for storing entity detail data from the Management Api
 * @class UmbManagementApiDetailDataRuntimeCache
 * @template DataModelType
 */
export class UmbManagementApiDetailDataRuntimeCache<DataModelType> {
	#entries: Map<string, UmbCacheEntryModel<DataModelType>> = new Map();

	/**
	 * Checks if an entry exists in the cache
	 * @param {string} id - The ID of the entry to check
	 * @returns {boolean} - True if the entry exists, false otherwise
	 * @memberof UmbManagementApiDetailDataRuntimeCache
	 */
	has(id: string): boolean {
		return this.#entries.has(id);
	}

	/**
	 * Adds an entry to the cache
	 * @param {string} id - The ID of the entry to add
	 * @param {DataModelType} data - The data to cache
	 * @memberof UmbManagementApiDetailDataRuntimeCache
	 */
	set(id: string, data: DataModelType): void {
		const cacheEntry: UmbCacheEntryModel<DataModelType> = {
			id: id,
			data,
		};

		this.#entries.set(id, cacheEntry);
	}

	/**
	 * Retrieves an entry from the cache
	 * @param {string} id - The ID of the entry to retrieve
	 * @returns {DataModelType | undefined} - The cached entry or undefined if not found
	 * @memberof UmbManagementApiDetailDataRuntimeCache
	 */
	get(id: string): DataModelType | undefined {
		const entry = this.#entries.get(id);
		return entry ? entry.data : undefined;
	}

	/**
	 * Deletes an entry from the cache
	 * @param {string} id - The ID of the entry to delete
	 * @memberof UmbManagementApiDetailDataRuntimeCache
	 */
	delete(id: string): void {
		this.#entries.delete(id);
	}

	/**
	 * Clears all entries from the cache
	 * @memberof UmbManagementApiDetailDataRuntimeCache
	 */
	clear(): void {
		this.#entries.clear();
	}
}
