import { UmbUserStateFilter } from '../../utils/index.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type {
	UmbDatalistDataSource,
	UmbDatalistItemModel,
	UmbDatalistRequestArgs,
	UmbDatalistResponse,
} from '@umbraco-cms/backoffice/datalist-data-source';
import { UmbLocalizationController } from '@umbraco-cms/backoffice/localization-api';

const stateLocalizationKeyMap: Record<string, string> = {
	Active: 'user_stateActive',
	Disabled: 'user_stateDisabled',
	LockedOut: 'user_stateLockedOut',
	Invited: 'user_stateInvited',
	Inactive: 'user_stateInactive',
};

export class UmbUserStateDatalistDataSource extends UmbControllerBase implements UmbDatalistDataSource {
	#localize = new UmbLocalizationController(this);

	async requestOptions(args: UmbDatalistRequestArgs): Promise<UmbDatalistResponse<UmbDatalistItemModel>> {
		const allItems = Object.values(UmbUserStateFilter).map((state) => ({
			unique: state,
			name: this.#localize.term(stateLocalizationKeyMap[state]),
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
			name: this.#localize.term(stateLocalizationKeyMap[unique] ?? unique),
			entityType: 'user-state',
		}));

		return { data: items };
	}
}

export { UmbUserStateDatalistDataSource as api };
