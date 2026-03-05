import type { UmbMockDBBase } from '../mock-db-base.js';
import { pagedResult } from '../paged-result.js';

const contentQueryFilter = () => {
	return true;
	console.log('implement filter logic for content items');
	//queryFilter(filterOptions.filter, item.name);
};

export class UmbMockContentCollectionManager<T extends { id: string }> {
	#db: UmbMockDBBase<T>;
	#collectionItemReadMapper: (item: T) => any;

	constructor(db: UmbMockDBBase<T>, collectionItemReadMapper: (item: T) => any) {
		this.#db = db;
		this.#collectionItemReadMapper = collectionItemReadMapper;
	}

	getItems(options: any): any {
		const allItems = this.#db.getAll();

		const filterOptions = {
			skip: options.skip || 0,
			take: options.take || 25,
			filter: options.filter,
		};

		// eslint-disable-next-line @typescript-eslint/ban-ts-comment
		// @ts-ignore
		const filteredItems = allItems.filter((item) => contentQueryFilter(filterOptions, item));
		const paginatedResult = pagedResult(filteredItems, filterOptions.skip, filterOptions.take);
		const mappedItems = paginatedResult.items.map((item) => this.#collectionItemReadMapper(item));

		return { items: mappedItems, total: paginatedResult.total };
	}
}
