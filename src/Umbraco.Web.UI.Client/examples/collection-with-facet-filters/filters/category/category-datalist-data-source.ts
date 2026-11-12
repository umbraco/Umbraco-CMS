import { EXAMPLE_PRODUCT_CATEGORY_ENTITY_TYPE } from '../../entity.js';
import { categories } from './categories.js';
import type {
	UmbDatalistDataSource,
	UmbDatalistItemModel,
	UmbDatalistRequestArgs,
	UmbDatalistResponse,
} from '@umbraco-cms/backoffice/datalist-data-source';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';

/**
 * Provides the categories of the category filter, and their names for the faceted options and the selected values.
 */
export class ExampleCategoryDatalistDataSource extends UmbControllerBase implements UmbDatalistDataSource {
	async requestOptions(args: UmbDatalistRequestArgs): Promise<UmbDatalistResponse<UmbDatalistItemModel>> {
		const text = args.filter?.toLowerCase();
		const matches = text ? categories.filter((item) => item.name.toLowerCase().includes(text)) : categories;
		const skip = args.skip ?? 0;
		const take = args.take ?? matches.length;
		return {
			data: { items: matches.slice(skip, skip + take).map(this.#toItem), total: matches.length },
		};
	}

	async requestItems(uniques: Array<string>) {
		return { data: categories.filter((item) => uniques.includes(item.id)).map(this.#toItem) };
	}

	#toItem = (item: { id: string; name: string }): UmbDatalistItemModel => ({
		entityType: EXAMPLE_PRODUCT_CATEGORY_ENTITY_TYPE,
		unique: item.id,
		name: item.name,
	});
}
