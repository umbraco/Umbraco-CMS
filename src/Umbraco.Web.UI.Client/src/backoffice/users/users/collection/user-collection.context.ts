import { USER_REPOSITORY_ALIAS } from '../repository/manifests';
import { UmbUserCollectionFilterModel } from '../types';
import { UmbCollectionContext } from '@umbraco-cms/backoffice/collection';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';
import { UserResponseModel } from '@umbraco-cms/backoffice/backend-api';

export class UmbUserCollectionContext extends UmbCollectionContext<UserResponseModel, UmbUserCollectionFilterModel> {
	constructor(host: UmbControllerHostElement) {
		super(host, 'user', USER_REPOSITORY_ALIAS);
	}
}
