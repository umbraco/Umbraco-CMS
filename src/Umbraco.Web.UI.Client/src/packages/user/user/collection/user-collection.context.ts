import type { UmbUserDetailModel } from '../types.js';
import { UMB_COLLECTION_VIEW_USER_GRID } from './views/index.js';
import type { UmbUserCollectionFilterModel, UmbUserOrderByOption, UmbUserStateFilterModel } from './types.js';
import { UmbUserOrderByModel } from './types.js';
import { UmbDefaultCollectionContext } from '@umbraco-cms/backoffice/collection';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbDirectionModel } from '@umbraco-cms/backoffice/models';
import { UmbArrayState, UmbStringState } from '@umbraco-cms/backoffice/observable-api';

export class UmbUserCollectionContext extends UmbDefaultCollectionContext<
	UmbUserDetailModel,
	UmbUserCollectionFilterModel
> {
	#orderByOptions = new UmbArrayState<UmbUserOrderByOption>(
		[
			{
				unique: 'nameAscending',
				label: '#user_sortNameAscending',
				config: {
					orderBy: UmbUserOrderByModel.NAME,
					orderDirection: UmbDirectionModel.ASCENDING,
				},
			},
			{
				unique: 'nameDescending',
				label: '#user_sortNameDescending',
				config: {
					orderBy: UmbUserOrderByModel.NAME,
					orderDirection: UmbDirectionModel.DESCENDING,
				},
			},
			{
				unique: 'createDateDescending',
				label: '#user_sortCreateDateDescending',
				config: {
					orderBy: UmbUserOrderByModel.CREATE_DATE,
					orderDirection: UmbDirectionModel.DESCENDING,
				},
			},
			{
				unique: 'createDateAscending',
				label: '#user_sortCreateDateAscending',
				config: {
					orderBy: UmbUserOrderByModel.CREATE_DATE,
					orderDirection: UmbDirectionModel.ASCENDING,
				},
			},
			{
				unique: 'lastLoginDateDescending',
				label: '#user_sortLastLoginDateDescending',
				config: {
					orderBy: UmbUserOrderByModel.LAST_LOGIN_DATE,
					orderDirection: UmbDirectionModel.DESCENDING,
				},
			},
		],
		(x) => x.label,
	);
	orderByOptions = this.#orderByOptions.asObservable();

	#activeOrderByOption = new UmbStringState<string | undefined>(undefined);
	activeOrderByOption = this.#activeOrderByOption.asObservable();

	constructor(host: UmbControllerHost) {
		super(host, UMB_COLLECTION_VIEW_USER_GRID);
		// init default orderBy option
		const defaultOrderByOption = this.#orderByOptions.getValue()[0];
		this.setActiveOrderByOption(defaultOrderByOption.unique);
	}

	/**
	 * Sets the active order by for the collection and refreshes the collection.
	 * @param {UmbUserOrderByModel} orderBy
	 * @param {UmbDirectionModel} orderDirection
	 * @memberof UmbUserCollectionContext
	 */
	setActiveOrderByOption(unique: string) {
		const option = this.#orderByOptions.getValue().find((x) => x.unique === unique);
		this.#activeOrderByOption.setValue(unique);
		this.setFilter({ orderBy: option?.config.orderBy, orderDirection: option?.config.orderDirection });
	}

	getActiveOrderByOption() {}

	/**
	 * Sets the state filter for the collection and refreshes the collection.
	 * @param {Array<UmbUserStateFilterModel>} selection
	 * @memberof UmbUserCollectionContext
	 */
	setStateFilter(selection: Array<UmbUserStateFilterModel>) {
		this.setFilter({ userStates: selection });
	}

	/**
	 * Sets the order by filter for the collection and refreshes the collection.
	 * @param {UmbUserOrderByModel} orderBy
	 * @memberof UmbUserCollectionContext
	 */
	setOrderByFilter(orderBy: UmbUserOrderByModel) {
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
	 * @param {UmbDirectionModel} orderDirection
	 * @memberof UmbUserCollectionContext
	 */
	setOrderDirectionFilter(orderDirection: UmbDirectionModel) {
		this.setFilter({ orderDirection });
	}
}

export default UmbUserCollectionContext;
