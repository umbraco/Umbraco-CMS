import { UmbUserGroupDetailDataSource } from '../../types';
import { DataSourceResponse } from '@umbraco-cms/backoffice/repository';
import {
	UserGroupPresentationModel,
	UserGroupResource,
	UpdateUserGroupRequestModel,
	SaveUserGroupRequestModel,
	UserGroupBaseModel,
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

	async createScaffold(parentId: string | null) {
		const data: SaveUserGroupRequestModel = {
			name: '',
			icon: '',
			sections: [],
			languages: [],
			hasAccessToAllLanguages: false,
			permissions: [],
		};
		return { data };
	}

	get(id: string): Promise<DataSourceResponse<UserGroupPresentationModel>> {
		if (!id) throw new Error('Id is missing');
		return tryExecuteAndNotify(this.#host, UserGroupResource.getUserGroupById({ id }));
	}
	insert(data: SaveUserGroupRequestModel) {
		return tryExecuteAndNotify(this.#host, UserGroupResource.postUserGroup({ requestBody: data }));
	}
	update(id: string, data: UpdateUserGroupRequestModel) {
		if (!id) throw new Error('Id is missing');

		return tryExecuteAndNotify(
			this.#host,
			UserGroupResource.putUserGroupById({
				id,
				requestBody: data,
			})
		);
	}
	delete(id: string): Promise<DataSourceResponse<undefined>> {
		if (!id) throw new Error('Id is missing');
		return tryExecuteAndNotify(this.#host, UserGroupResource.deleteUserGroupById({ id }));
	}
}
