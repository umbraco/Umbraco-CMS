import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';
import { UserResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { USER_REPOSITORY_ALIAS } from '../repository/manifests';
import { UmbCollectionContext } from '../../../shared/components/collection/collection.context';
import { UmbUserCollectionFilterModel } from '../types';

export class UmbUserCollectionContext extends UmbCollectionContext<UserResponseModel, UmbUserCollectionFilterModel> {
	constructor(host: UmbControllerHostElement) {
		super(host, 'user', USER_REPOSITORY_ALIAS);
	}
}
