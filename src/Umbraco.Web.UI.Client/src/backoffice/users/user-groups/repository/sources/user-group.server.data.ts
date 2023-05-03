import { UmbUserGroupDetailDataSource } from '../../types';
import { DataSourceResponse } from '@umbraco-cms/backoffice/repository';
import {
	CreateUserRequestModel,
	UpdateUserRequestModel,
	UserPresentationBaseModel,
	UserResource,
	InviteUserRequestModel,
	EnableUserRequestModel,
	DisableUserRequestModel,
	UserGroupPresentationModel,
	UserGroupResource,
} from '@umbraco-cms/backoffice/backend-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for the User that fetches data from the server
 * @export
 * @class UmbUserGroupServerDataSource
 * @implements {RepositoryDetailDataSource}
 */
export class UmbUserGroupServerDataSource implements UmbUserGroupDetailDataSource {
	#host: UmbControllerHostElement;

	/**
	 * Creates an instance of UmbUserGroupServerDataSource.
	 * @param {UmbControllerHostElement} host
	 * @memberof UmbUserGroupServerDataSource
	 */
	constructor(host: UmbControllerHostElement) {
		this.#host = host;
	}
	createScaffold(parentId: string | null): Promise<DataSourceResponse<UserGroupPresentationModel>> {
		throw new Error('Method not implemented.');
	}
	get(id: string): Promise<DataSourceResponse<UserGroupPresentationModel>> {
		if (!id) throw new Error('Id is missing');
		return tryExecuteAndNotify(this.#host, UserGroupResource.getUserGroupById({ id }));
	}
	insert(data: UserGroupPresentationModel): Promise<DataSourceResponse<void>> {
		throw new Error('Method not implemented.');
	}
	update(unique: string, data: UserGroupPresentationModel): Promise<DataSourceResponse<UserGroupPresentationModel>> {
		throw new Error('Method not implemented.');
	}
	delete(unique: string): Promise<DataSourceResponse<undefined>> {
		throw new Error('Method not implemented.');
	}
}
