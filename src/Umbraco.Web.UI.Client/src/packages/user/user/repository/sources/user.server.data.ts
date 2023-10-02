import { UmbUserDetail, UmbUserDetailDataSource } from '../../types.js';
import { DataSourceResponse, extendDataSourceResponseData } from '@umbraco-cms/backoffice/repository';
import {
	CreateUserRequestModel,
	UpdateUserRequestModel,
	UserPresentationBaseModel,
	UserResource,
	InviteUserRequestModel,
} from '@umbraco-cms/backoffice/backend-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for the User that fetches data from the server
 * @export
 * @class UmbUserServerDataSource
 * @implements {RepositoryDetailDataSource}
 */
export class UmbUserServerDataSource implements UmbUserDetailDataSource {
	#host: UmbControllerHostElement;

	/**
	 * Creates an instance of UmbUserServerDataSource.
	 * @param {UmbControllerHostElement} host
	 * @memberof UmbUserServerDataSource
	 */
	constructor(host: UmbControllerHostElement) {
		this.#host = host;
	}

	// Details
	createScaffold(parentId: string | null): Promise<DataSourceResponse<UserPresentationBaseModel>> {
		throw new Error('Method not implemented.');
	}

	async get(id: string) {
		if (!id) throw new Error('Id is missing');
		const response = await tryExecuteAndNotify(this.#host, UserResource.getUserById({ id }));
		return extendDataSourceResponseData<UmbUserDetail>(response, {
			entityType: 'user',
		});
	}

	insert(data: CreateUserRequestModel) {
		return tryExecuteAndNotify(this.#host, UserResource.postUser({ requestBody: data }));
	}

	update(id: string, data: UpdateUserRequestModel) {
		if (!id) throw new Error('Id is missing');

		return tryExecuteAndNotify(
			this.#host,
			UserResource.putUserById({
				id,
				requestBody: data,
			})
		);
	}

	delete(id: string) {
		if (!id) throw new Error('Id is missing');
		return tryExecuteAndNotify(this.#host, UserResource.deleteUserById({ id }));
	}

	// Invite
	invite(data: InviteUserRequestModel) {
		if (!data) throw new Error('Invite data is missing');
		return tryExecuteAndNotify(this.#host, UserResource.postUserInvite({ requestBody: data }));
	}
}
