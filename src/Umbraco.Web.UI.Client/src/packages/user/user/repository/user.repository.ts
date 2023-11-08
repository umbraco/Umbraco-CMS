import { UmbUserDetailDataSource, UmbUserSetGroupDataSource } from '../types.js';
import { UmbUserServerDataSource } from './sources/user.server.data.js';
import { UmbUserSetGroupsServerDataSource } from './sources/user-set-group.server.data.js';

import { UmbUserRepositoryBase } from './user-repository-base.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbDataSourceErrorResponse, UmbDetailRepository } from '@umbraco-cms/backoffice/repository';
import {
	CreateUserRequestModel,
	CreateUserResponseModel,
	UpdateUserRequestModel,
	UserResponseModel,
} from '@umbraco-cms/backoffice/backend-api';
import { UmbNotificationContext } from '@umbraco-cms/backoffice/notification';

export interface IUmbUserDetailRepository
	extends UmbDetailRepository<
		CreateUserRequestModel,
		CreateUserResponseModel,
		UpdateUserRequestModel,
		UserResponseModel
	> {
	uploadAvatar(id: string, file: File): Promise<UmbDataSourceErrorResponse>;
	deleteAvatar(id: string): Promise<UmbDataSourceErrorResponse>;
}

export class UmbUserRepository extends UmbUserRepositoryBase implements IUmbUserDetailRepository {
	#detailSource: UmbUserDetailDataSource;
	#setUserGroupsSource: UmbUserSetGroupDataSource;
	#notificationContext?: UmbNotificationContext;

	constructor(host: UmbControllerHost) {
		super(host);

		this.#detailSource = new UmbUserServerDataSource(host);
		this.#setUserGroupsSource = new UmbUserSetGroupsServerDataSource(host);
	}

	/**
	 * Creates a new user scaffold
	 * @param {(string | null)} parentId
	 * @return {*}
	 * @memberof UmbUserRepository
	 */
	createScaffold(parentId: string | null) {
		if (parentId === undefined) throw new Error('Parent id is missing');
		return this.#detailSource.createScaffold(parentId);
	}

	/**
	 * Requests the user with the given id
	 * @param {string} id
	 * @return {*}
	 * @memberof UmbUserRepository
	 */
	async requestById(id: string) {
		if (!id) throw new Error('Id is missing');
		await this.init;

		const { data, error } = await this.#detailSource.get(id);

		if (data) {
			this.detailStore!.append(data);
		}

		return { data, error, asObservable: () => this.detailStore!.byId(id) };
	}

	async setUserGroups(userIds: Array<string>, userGroupIds: Array<string>) {
		if (userGroupIds.length === 0) throw new Error('User group ids are missing');
		if (userIds.length === 0) throw new Error('User ids are missing');

		const { error } = await this.#setUserGroupsSource.setGroups(userIds, userGroupIds);

		if (!error) {
			//TODO: Update relevant stores
		}

		return { error };
	}

	/**
	 * Returns an observable for the user with the given id
	 * @param {string} id
	 * @return {Promise<Observable<UserDetailModel>>}
	 * @memberof UmbUserRepository
	 */
	async byId(id: string) {
		if (!id) throw new Error('Key is missing');
		await this.init;
		return this.detailStore!.byId(id);
	}

	/**
	 * Creates a new user
	 * @param {CreateUserRequestModel} userRequestData
	 * @return { Promise<UmbDataSourceSuccessResponse, UmbDataSourceErrorResponse>}
	 * @memberof UmbUserRepository
	 */
	async create(userRequestData: CreateUserRequestModel) {
		if (!userRequestData) throw new Error('Data is missing');

		const { data, error } = await this.#detailSource.insert(userRequestData);

		if (data) {
			this.detailStore?.append(data);

			const notification = { data: { message: `User created` } };
			this.#notificationContext?.peek('positive', notification);
		}

		return { data, error };
	}

	/**
	 * Saves the user with the given id
	 * @param {string} id
	 * @param {UpdateUserRequestModel} user
	 * @return {Promise<UmbDataSourceSuccessResponse, UmbDataSourceErrorResponse>}
	 * @memberof UmbUserRepository
	 */
	async save(id: string, user: UpdateUserRequestModel) {
		if (!id) throw new Error('User id is missing');
		if (!user) throw new Error('User update data is missing');

		const { data, error } = await this.#detailSource.update(id, user);

		if (data) {
			this.detailStore?.append(data);
		}

		if (!error) {
			// TODO: how do we localize here?
			// The localize method shouldn't be part of the UmbControllerHost interface
			// this._host.localize?.term('speechBubbles_editUserSaved') ??
			const notification = {
				data: { message: 'User saved' },
			};
			this.#notificationContext?.peek('positive', notification);
		}

		return { data, error };
	}

	/**
	 * Deletes the user with the given id
	 * @param {string} id
	 * @return {Promise<UmbDataSourceErrorResponse>}
	 * @memberof UmbUserRepository
	 */
	async delete(id: string) {
		if (!id) throw new Error('Id is missing');

		const { error } = await this.#detailSource.delete(id);

		if (!error) {
			this.detailStore?.removeItem(id);

			const notification = { data: { message: `User deleted` } };
			this.#notificationContext?.peek('positive', notification);
		}

		return { error };
	}

	/**
	 * Uploads an avatar for the user with the given id
	 * @param {string} id
	 * @param {File} file
	 * @return {Promise<UmbDataSourceErrorResponse>}
	 * @memberof UmbUserRepository
	 */
	async uploadAvatar(id: string, file: File) {
		if (!id) throw new Error('Id is missing');

		const { error } = await this.#detailSource.uploadAvatar(id, file);

		if (!error) {
			// TODO: update store + current user
			const notification = { data: { message: `Avatar uploaded` } };
			this.#notificationContext?.peek('positive', notification);
		}

		return { error };
	}

	/**
	 * Removes the avatar for the user with the given id
	 * @param {string} id
	 * @return {Promise<UmbDataSourceErrorResponse>}
	 * @memberof UmbUserRepository
	 */
	async deleteAvatar(id: string) {
		if (!id) throw new Error('Id is missing');

		const { error } = await this.#detailSource.deleteAvatar(id);

		if (!error) {
			// TODO: update store + current user
			const notification = { data: { message: `Avatar deleted` } };
			this.#notificationContext?.peek('positive', notification);
		}

		return { error };
	}
}
