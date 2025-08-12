// Keep internal
interface UmbCacheEntryModel<DataModelType> {
	id: string;
	data: DataModelType;
}

export class UmbManagementApiDetailDataRuntimeCache<DataModelType> {
	#entries: Map<string, UmbCacheEntryModel<DataModelType>> = new Map();

	has(id: string): boolean {
		return this.#entries.has(id);
	}

	set(id: string, data: DataModelType): void {
		const cacheEntry: UmbCacheEntryModel<DataModelType> = {
			id: id,
			data,
		};

		this.#entries.set(id, cacheEntry);
	}

	get(id: string) {
		const entry = this.#entries.get(id);
		return entry ? entry.data : undefined;
	}

	delete(id: string): void {
		this.#entries.delete(id);
	}
}
