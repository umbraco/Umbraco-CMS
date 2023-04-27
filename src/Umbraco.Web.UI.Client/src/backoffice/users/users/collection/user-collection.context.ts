import { USER_REPOSITORY_ALIAS } from '../repository/manifests';
import { UmbCollectionContext } from '../../../shared/components/collection/collection.context';
import { UmbUserCollectionFilterModel } from '../types';
import { UserResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';

export class UmbUserCollectionContext extends UmbCollectionContext<UserResponseModel, UmbUserCollectionFilterModel> {
	constructor(host: UmbControllerHostElement) {
		super(host, 'user', USER_REPOSITORY_ALIAS);
	}
}
