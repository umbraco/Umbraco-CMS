import type { UmbMockMediaModel } from './media.data.js';
import type { UmbMediaMockDB } from './media.db.js';
import type { DirectionModel, MediaCollectionResponseModel } from '@umbraco-cms/backoffice/external/backend-api';

export class UmbMockMediaCollectionManager {
	#mediaDb: UmbMediaMockDB;

	#collectionMapper: (item: UmbMockMediaModel) => MediaCollectionResponseModel;

	constructor(mediaDb: UmbMediaMockDB, collectionMapper: (item: UmbMockMediaModel) => MediaCollectionResponseModel) {
		this.#mediaDb = mediaDb;
		this.#collectionMapper = collectionMapper;
	}

	getCollectionMedia({
		id,
		skip = 0,
		take = 100,
	}: {
		id?: string;
		dataTypeId?: string;
		orderBy?: string;
		orderDirection?: DirectionModel;
		filter?: string;
		skip?: number;
		take?: number;
	}) {
		const collection = !id
			? this.#mediaDb.getAll().filter((item) => item.parent === null)
			: this.#mediaDb.getAll().filter((item) => item.parent?.id === id);

		const items = collection.map((item) => this.#collectionMapper(item)).slice(skip, skip + take);
		const total = collection.length;
		return { items: items, total };
	}
}
