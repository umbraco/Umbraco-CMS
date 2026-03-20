import {
	UmbExtensionTypeCollectionRepository,
	UmbExtensionTypeItemRepository,
} from '@umbraco-cms/backoffice/extension-type';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type {
	UmbDatalistDataSource,
	UmbDatalistItemModel,
	UmbDatalistRequestArgs,
	UmbDatalistResponse,
} from '@umbraco-cms/backoffice/datalist-data-source';

export class UmbExtensionCollectionDatalistDataSource extends UmbControllerBase implements UmbDatalistDataSource {
	#collectionRepository: UmbExtensionTypeCollectionRepository;
	#itemRepository: UmbExtensionTypeItemRepository;

	constructor(host: UmbControllerHost) {
		super(host);
		this.#collectionRepository = new UmbExtensionTypeCollectionRepository(this);
		this.#itemRepository = new UmbExtensionTypeItemRepository(this);
	}

	async requestOptions(args: UmbDatalistRequestArgs): Promise<UmbDatalistResponse<UmbDatalistItemModel>> {
		const { data } = await this.#collectionRepository.requestCollection({
			filter: args.filter,
			skip: args.skip,
			take: args.take,
		});

		if (!data) return { data: undefined };

		const items = data.items.map((item) => ({
			unique: item.unique,
			name: item.name,
			entityType: item.entityType,
		}));

		return {
			data: {
				items,
				total: data.total,
			},
		};
	}

	async requestItems(uniques: Array<string>): Promise<{ data?: Array<UmbDatalistItemModel> }> {
		const { data } = await this.#itemRepository.requestItems(uniques);

		if (!data) return { data: undefined };

		const items = data.map((item) => ({
			unique: item.unique,
			name: item.name,
			entityType: item.entityType,
		}));

		return { data: items };
	}
}

export { UmbExtensionCollectionDatalistDataSource as api };
