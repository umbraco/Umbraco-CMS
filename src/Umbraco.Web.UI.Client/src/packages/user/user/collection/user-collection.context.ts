import type { UmbUserDetailModel } from '../types.js';
import { UMB_COLLECTION_VIEW_USER_GRID } from './views/index.js';
import type { UmbUserCollectionFilterModel, UmbUserOrderByOption } from './types.js';
import type { UmbUserOrderByType, UmbUserStateFilterType } from './utils/index.js';
import { UmbUserOrderBy } from './utils/index.js';
import { UmbDefaultCollectionContext } from '@umbraco-cms/backoffice/collection';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbArrayState, UmbStringState } from '@umbraco-cms/backoffice/observable-api';
import type { UmbDirectionType } from '@umbraco-cms/backoffice/utils';
import { UmbDirection } from '@umbraco-cms/backoffice/utils';

const orderByOptions: Array<UmbUserOrderByOption> = [
	{
		unique: 'nameAscending',
		label: '#user_sortNameAscending',
		config: {
			orderBy: UmbUserOrderBy.NAME,
			orderDirection: UmbDirection.ASCENDING,
		},
	},
	{
		unique: 'nameDescending',
		label: '#user_sortNameDescending',
		config: {
			orderBy: UmbUserOrderBy.NAME,
			orderDirection: UmbDirection.DESCENDING,
		},
	},
	{
		unique: 'createDateDescending',
		label: '#user_sortCreateDateDescending',
		config: {
			orderBy: UmbUserOrderBy.CREATE_DATE,
			orderDirection: UmbDirection.DESCENDING,
		},
	},
	{
		unique: 'createDateAscending',
		label: '#user_sortCreateDateAscending',
		config: {
			orderBy: UmbUserOrderBy.CREATE_DATE,
			orderDirection: UmbDirection.ASCENDING,
		},
	},
	{
		unique: 'lastLoginDateDescending',
		label: '#user_sortLastLoginDateDescending',
		config: {
			orderBy: UmbUserOrderBy.LAST_LOGIN_DATE,
			orderDirection: UmbDirection.DESCENDING,
		},
	},
];

export class UmbUserCollectionContext extends UmbDefaultCollectionContext<
	UmbUserDetailModel,
	UmbUserCollectionFilterModel
> {
	#orderByOptions = new UmbArrayState<UmbUserOrderByOption>([], (x) => x.label);
	orderByOptions = this.#orderByOptions.asObservable();

	#activeOrderByOption = new UmbStringState<string | undefined>(undefined);
	activeOrderByOption = this.#activeOrderByOption.asObservable();

	constructor(host: UmbControllerHost) {
		const firstOption: UmbUserOrderByOption = orderByOptions[0];

		super(host, UMB_COLLECTION_VIEW_USER_GRID, {
			orderBy: firstOption.config.orderBy,
			orderDirection: firstOption.config.orderDirection,
		});

		this.#orderByOptions.setValue(orderByOptions);
		this.#activeOrderByOption.setValue(firstOption.unique);
	}

	/**
	 * Sets the active order by option for the collection and refreshes the collection.
	 * @param {string} unique
	 * @memberof UmbUserCollectionContext
	 */
	setActiveOrderByOption(unique: string) {
		const option = this.#orderByOptions.getValue().find((x) => x.unique === unique);
		this.#activeOrderByOption.setValue(unique);
		this.setFilter({ orderBy: option?.config.orderBy, orderDirection: option?.config.orderDirection });
	}

	/**
	 * Sets the state filter for the collection and refreshes the collection.
	 * @param {Array<UmbUserStateFilterModel>} selection
	 * @memberof UmbUserCollectionContext
	 */
	setStateFilter(selection: Array<UmbUserStateFilterType>) {
		this.setFilter({ userStates: selection });
	}

	/**
	 * Sets the order by filter for the collection and refreshes the collection.
	 * @param {UmbUserOrderByModel} orderBy
	 * @memberof UmbUserCollectionContext
	 */
	setOrderByFilter(orderBy: UmbUserOrderByType) {
		this.setFilter({ orderBy });
	}

	/**
	 * Sets the user group filter for the collection and refreshes the collection.
	 * @param {Array<string>} selection
	 * @memberof UmbUserCollectionContext
	 */
	setUserGroupFilter(selection: Array<string>) {
		this.setFilter({ userGroupIds: selection });
	}

	/**
	 * Sets the order direction filter for the collection and refreshes the collection.
	 * @param {any} orderDirection
	 * @memberof UmbUserCollectionContext
	 */
	setOrderDirectionFilter(orderDirection: UmbDirectionType) {
		this.setFilter({ orderDirection });
	}
}

export { UmbUserCollectionContext as api };
