import type { UmbMockDocumentModel } from './document.data.js';
import type { UmbDocumentMockDB } from './document.db.js';
import type { DirectionModel, DocumentCollectionResponseModel } from '@umbraco-cms/backoffice/external/backend-api';

export class UmbMockDocumentCollectionManager {
	#documentDb: UmbDocumentMockDB;

	#collectionMapper: (item: UmbMockDocumentModel) => DocumentCollectionResponseModel;

	constructor(
		documentDb: UmbDocumentMockDB,
		collectionMapper: (item: UmbMockDocumentModel) => DocumentCollectionResponseModel,
	) {
		this.#documentDb = documentDb;
		this.#collectionMapper = collectionMapper;
	}

	getCollectionDocumentById({
		id,
		skip = 0,
		take = 100,
	}: {
		id: string;
		dataTypeId?: string;
		orderBy?: string;
		orderCulture?: string;
		orderDirection?: DirectionModel;
		filter?: string;
		skip?: number;
		take?: number;
	}) {
		const collection = this.#documentDb.getAll().filter((item) => item.parent?.id === id);
		const items = collection.map((item) => this.#collectionMapper(item)).slice(skip, skip + take);
		const total = collection.length;
		return { items: items, total };
	}
}
