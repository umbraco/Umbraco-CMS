import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type {
	UmbDatalistDataSource,
	UmbDatalistItemModel,
	UmbDatalistRequestArgs,
	UmbDatalistResponse,
} from '@umbraco-cms/backoffice/datalist-data-source';
import { UmbMemberGroupCollectionRepository } from '@umbraco-cms/backoffice/member-group';

export class UmbMemberGroupDatalistDataSource extends UmbControllerBase implements UmbDatalistDataSource {
	#repository: UmbMemberGroupCollectionRepository;

	constructor(host: UmbControllerHost) {
		super(host);
		this.#repository = new UmbMemberGroupCollectionRepository(this);
	}

	async requestOptions(args: UmbDatalistRequestArgs): Promise<UmbDatalistResponse<UmbDatalistItemModel>> {
		const { data } = await this.#repository.requestCollection({});

		if (!data) return { data: undefined };

		const allItems = data.items.map((item) => ({
			unique: item.name,
			name: item.name,
			entityType: item.entityType,
		}));

		const filter = args.filter?.toLowerCase();
		const items = filter ? allItems.filter((item) => item.name?.toLowerCase().includes(filter)) : allItems;

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
		const { data } = await this.#repository.requestCollection({});

		if (!data) return { data: undefined };

		const items = data.items
			.filter((item) => uniques.includes(item.name))
			.map((item) => ({
				unique: item.name,
				name: item.name,
				entityType: item.entityType,
			}));

		return { data: items };
	}
}
