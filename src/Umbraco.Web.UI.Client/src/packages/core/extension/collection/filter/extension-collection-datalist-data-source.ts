import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { fromCamelCase } from '@umbraco-cms/backoffice/utils';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type {
	UmbDatalistDataSource,
	UmbDatalistItemModel,
	UmbDatalistRequestArgs,
	UmbDatalistResponse,
} from '@umbraco-cms/backoffice/datalist-data-source';
import { UmbExtensionCollectionRepository } from '../repository';
import { UmbExtensionItemRepository } from '../../item';

export class UmbExtensionCollectionDatalistDataSource extends UmbControllerBase implements UmbDatalistDataSource {
	#collectionRepository: UmbExtensionCollectionRepository;
	#itemRepository: UmbExtensionItemRepository;

	constructor(host: UmbControllerHost) {
		super(host);
		this.#collectionRepository = new UmbExtensionCollectionRepository(this);
		this.#itemRepository = new UmbExtensionItemRepository(this);
	}

	async requestOptions(args: UmbDatalistRequestArgs): Promise<UmbDatalistResponse<UmbDatalistItemModel>> {
		const { data } = await this.#collectionRepository.requestCollection({});

		if (!data) return { data: undefined };

		const types = [...new Set(data.items.map((x) => x.manifest.type))];

		const allItems = types.sort().map((type) => ({
			unique: type,
			name: fromCamelCase(type),
			entityType: 'extension-type',
		}));

		const filter = args.filter?.toLowerCase();
		const items = filter ? allItems.filter((item) => item.name.toLowerCase().includes(filter)) : allItems;

		const skip = args.skip ?? 0;
		const take = args.take ?? items.length;
		const paged = items.slice(skip, skip + take);

		return {
			data: {
				items: paged,
				total: items.length,
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
