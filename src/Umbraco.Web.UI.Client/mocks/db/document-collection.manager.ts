import type { UmbMockDocumentModel } from '../data/mock-data-set.types.js';
import type { UmbDocumentMockDB } from './document.db.js';
import { DirectionModel } from '@umbraco-cms/backoffice/external/backend-api';
import type {
	DocumentCollectionResponseModel,
	DocumentVariantResponseModel,
} from '@umbraco-cms/backoffice/external/backend-api';

export interface UmbMockDocumentCollectionRequestArgs {
	id: string;
	dataTypeId?: string;
	orderBy?: string;
	orderCulture?: string;
	orderDirection?: DirectionModel;
	filter?: string;
	skip?: number;
	take?: number;
}

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
		orderBy = 'updateDate',
		orderCulture,
		orderDirection = DirectionModel.ASCENDING,
		filter,
		skip = 0,
		take = 100,
	}: UmbMockDocumentCollectionRequestArgs) {
		const children = this.#documentDb.getAll().filter((item) => item.parent?.id === id);

		// The mock data has no sort order, so the order the children are declared in is used as theirs.
		const items = children.map((child, index) => ({ ...this.#collectionMapper(child), sortOrder: index }));

		const filteredItems = filter ? items.filter((item) => this.#matchesFilter(item, filter)) : items;
		const sortedItems = this.#sortItems(filteredItems, orderBy, orderDirection, orderCulture);

		return { items: sortedItems.slice(skip, skip + take), total: filteredItems.length };
	}

	#matchesFilter(item: DocumentCollectionResponseModel, filter: string): boolean {
		const term = filter.toLowerCase();
		return item.variants.some((variant) => variant.name.toLowerCase().includes(term));
	}

	#sortItems(
		items: Array<DocumentCollectionResponseModel>,
		orderBy: string,
		orderDirection: DirectionModel,
		orderCulture?: string,
	): Array<DocumentCollectionResponseModel> {
		const direction = orderDirection === DirectionModel.DESCENDING ? -1 : 1;

		return [...items].sort((a, b) => {
			const aValue = this.#getSortValue(a, orderBy, orderCulture);
			const bValue = this.#getSortValue(b, orderBy, orderCulture);
			if (aValue === bValue) return 0;
			return (aValue > bValue ? 1 : -1) * direction;
		});
	}

	#getSortValue(item: DocumentCollectionResponseModel, orderBy: string, orderCulture?: string): string | number {
		if (orderBy === 'sortOrder') return item.sortOrder;

		const variant = this.#getVariant(item, orderCulture);

		switch (orderBy) {
			case 'name':
				return variant?.name ?? '';
			case 'createDate':
				return variant?.createDate ?? '';
			case 'updateDate':
				return variant?.updateDate ?? '';
			default:
				// Anything else is a property of the document, ex. one of the user defined properties of the collection.
				return String(item.values.find((value) => value.alias === orderBy)?.value ?? '');
		}
	}

	#getVariant(item: DocumentCollectionResponseModel, orderCulture?: string): DocumentVariantResponseModel | undefined {
		const cultureVariant = orderCulture ? item.variants.find((variant) => variant.culture === orderCulture) : undefined;
		return cultureVariant ?? item.variants[0];
	}
}
