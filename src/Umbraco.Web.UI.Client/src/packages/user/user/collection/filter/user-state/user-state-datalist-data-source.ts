import { UmbUserStateFilter } from '../../utils/index.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type {
	UmbDatalistDataSource,
	UmbDatalistItemModel,
	UmbDatalistRequestArgs,
	UmbDatalistResponse,
} from '@umbraco-cms/backoffice/datalist-data-source';

export class UmbUserStateDatalistDataSource extends UmbControllerBase implements UmbDatalistDataSource {
	constructor(host: UmbControllerHost) {
		super(host);
	}

	async requestOptions(args: UmbDatalistRequestArgs): Promise<UmbDatalistResponse<UmbDatalistItemModel>> {
		const allItems = Object.values(UmbUserStateFilter).map((state) => ({
			unique: state,
			name: state,
			entityType: 'user-state',
		}));

		const filter = args.filter?.toLowerCase();
		const items = filter ? allItems.filter((item) => item.name.toLowerCase().includes(filter)) : allItems;

		return {
			data: {
				items,
				total: items.length,
			},
		};
	}

	async requestItems(uniques: Array<string>): Promise<{ data?: Array<UmbDatalistItemModel> }> {
		const items = uniques.map((unique) => ({
			unique,
			name: unique,
			entityType: 'user-state',
		}));

		return { data: items };
	}
}

export { UmbUserStateDatalistDataSource as api };
