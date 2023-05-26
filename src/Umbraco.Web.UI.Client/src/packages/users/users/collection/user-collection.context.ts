import { USER_REPOSITORY_ALIAS } from '../repository/manifests.js';
import { UmbUserCollectionFilterModel } from '../types.js';
import { UmbCollectionContext } from '@umbraco-cms/backoffice/collection';
import { UserOrderModel, UserResponseModel, UserStateModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';

export class UmbUserCollectionContext extends UmbCollectionContext<UserResponseModel, UmbUserCollectionFilterModel> {
	constructor(host: UmbControllerHostElement) {
		super(host, 'user', USER_REPOSITORY_ALIAS);
	}

	setStateFilter(selection: Array<UserStateModel>) {
		this.setFilter({ userStates: selection });
	}

	setOrderByFilter(orderBy: UserOrderModel) {
		this.setFilter({ orderBy });
	}
}
