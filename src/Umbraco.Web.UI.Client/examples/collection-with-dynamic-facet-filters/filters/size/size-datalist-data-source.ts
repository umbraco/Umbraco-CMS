import { EXAMPLE_PRODUCT_SIZE_ENTITY_TYPE } from '../../entity.js';
import { sizes } from './sizes.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type {
	UmbDatalistDataSource,
	UmbDatalistItemModel,
	UmbDatalistRequestArgs,
	UmbDatalistResponse,
} from '@umbraco-cms/backoffice/datalist-data-source';

/**
 * Provides the sizes of the size filter, and their names for the faceted options and the selected values.
 */
export class ExampleSizeDatalistDataSource extends UmbControllerBase implements UmbDatalistDataSource {
	async requestOptions(args: UmbDatalistRequestArgs): Promise<UmbDatalistResponse<UmbDatalistItemModel>> {
		const text = args.filter?.toLowerCase();
		const matches = text ? sizes.filter((item) => item.name.toLowerCase().includes(text)) : sizes;
		const skip = args.skip ?? 0;
		const take = args.take ?? matches.length;
		return {
			data: { items: matches.slice(skip, skip + take).map(this.#toItem), total: matches.length },
		};
	}

	async requestItems(uniques: Array<string>) {
		return { data: sizes.filter((item) => uniques.includes(item.id)).map(this.#toItem) };
	}

	#toItem = (item: { id: string; name: string }): UmbDatalistItemModel => ({
		entityType: EXAMPLE_PRODUCT_SIZE_ENTITY_TYPE,
		unique: item.id,
		name: item.name,
	});
}
