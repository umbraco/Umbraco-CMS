import type { UmbMockMediaModel } from '../data/mock-data-set.types.js';
import type { UmbMediaMockDB } from './media.db.js';
import { DirectionModel } from '@umbraco-cms/backoffice/external/backend-api';
import type { MediaCollectionResponseModel } from '@umbraco-cms/backoffice/external/backend-api';

export interface UmbMockMediaCollectionRequestArgs {
	id?: string;
	dataTypeId?: string;
	orderBy?: string;
	orderDirection?: DirectionModel;
	filter?: string;
	skip?: number;
	take?: number;
}

export class UmbMockMediaCollectionManager {
	#mediaDb: UmbMediaMockDB;

	#collectionMapper: (item: UmbMockMediaModel) => MediaCollectionResponseModel;

	constructor(mediaDb: UmbMediaMockDB, collectionMapper: (item: UmbMockMediaModel) => MediaCollectionResponseModel) {
		this.#mediaDb = mediaDb;
		this.#collectionMapper = collectionMapper;
	}

	getCollectionMedia({
		id,
		orderBy = 'updateDate',
		orderDirection = DirectionModel.ASCENDING,
		filter,
		skip = 0,
		take = 100,
	}: UmbMockMediaCollectionRequestArgs) {
		const children = !id
			? this.#mediaDb.getAll().filter((item) => item.parent === null)
			: this.#mediaDb.getAll().filter((item) => item.parent?.id === id);

		// The mock data has no sort order, so the order the children are declared in is used as theirs.
		const items = children.map((child, index) => ({ ...this.#collectionMapper(child), sortOrder: index }));

		const filteredItems = filter ? items.filter((item) => this.#matchesFilter(item, filter)) : items;
		const sortedItems = this.#sortItems(filteredItems, orderBy, orderDirection);

		return { items: sortedItems.slice(skip, skip + take), total: filteredItems.length };
	}

	#matchesFilter(item: MediaCollectionResponseModel, filter: string): boolean {
		const term = filter.toLowerCase();
		return item.variants.some((variant) => variant.name.toLowerCase().includes(term));
	}

	#sortItems(
		items: Array<MediaCollectionResponseModel>,
		orderBy: string,
		orderDirection: DirectionModel,
	): Array<MediaCollectionResponseModel> {
		const direction = orderDirection === DirectionModel.DESCENDING ? -1 : 1;

		return [...items].sort((a, b) => {
			const aValue = this.#getSortValue(a, orderBy);
			const bValue = this.#getSortValue(b, orderBy);
			if (aValue === bValue) return 0;
			return (aValue > bValue ? 1 : -1) * direction;
		});
	}

	#getSortValue(item: MediaCollectionResponseModel, orderBy: string): string | number {
		if (orderBy === 'sortOrder') return item.sortOrder;

		const variant = item.variants[0];

		switch (orderBy) {
			case 'name':
				return variant?.name ?? '';
			case 'createDate':
				return variant?.createDate ?? '';
			case 'updateDate':
				return variant?.updateDate ?? '';
			default:
				// Anything else is a property of the media, ex. one of the user defined properties of the collection.
				return String(item.values.find((value) => value.alias === orderBy)?.value ?? '');
		}
	}
}
