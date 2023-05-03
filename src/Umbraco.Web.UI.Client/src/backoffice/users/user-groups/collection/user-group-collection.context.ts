import { USER_GROUP_REPOSITORY_ALIAS } from '../repository/manifests';
import { UmbUserGroupCollectionFilterModel } from '../types';
import { UmbCollectionContext } from '@umbraco-cms/backoffice/collection';
import { UserGroupPresentationModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';

export class UmbUserGroupCollectionContext extends UmbCollectionContext<
	UserGroupPresentationModel,
	UmbUserGroupCollectionFilterModel
> {
	constructor(host: UmbControllerHostElement) {
		super(host, 'user-group', USER_GROUP_REPOSITORY_ALIAS);
	}
}
