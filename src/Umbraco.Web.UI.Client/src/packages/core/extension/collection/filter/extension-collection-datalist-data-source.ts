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

export class UmbExtensionCollectionDatalistDataSource extends UmbControllerBase implements UmbDatalistDataSource {
	#repository: UmbExtensionCollectionRepository;

	constructor(host: UmbControllerHost) {
		super(host);
		this.#repository = new UmbExtensionCollectionRepository(this);
	}

	async requestOptions(args: UmbDatalistRequestArgs): Promise<UmbDatalistResponse<UmbDatalistItemModel>> {
		const { data } = await this.#repository.requestCollection({});

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
		const items = uniques.map((unique) => ({
			unique,
			name: fromCamelCase(unique),
			entityType: 'extension-type',
		}));

		return { data: items };
	}
}

export { UmbExtensionCollectionDatalistDataSource as api };
