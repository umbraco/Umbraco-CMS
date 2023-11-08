import { UmbUserDetailDataSource, UmbUserSetGroupDataSource } from '../types.js';
import { UmbUserServerDataSource } from './sources/user.server.data.js';
import { UmbUserSetGroupsServerDataSource } from './sources/user-set-group.server.data.js';

import { UmbUserRepositoryBase } from './user-repository-base.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbDetailRepository } from '@umbraco-cms/backoffice/repository';
import {
	CreateUserRequestModel,
	CreateUserResponseModel,
	UpdateUserRequestModel,
	UserResponseModel,
} from '@umbraco-cms/backoffice/backend-api';
import { UmbNotificationContext } from '@umbraco-cms/backoffice/notification';

export type UmbUserDetailRepository = UmbDetailRepository<
	CreateUserRequestModel,
	CreateUserResponseModel,
	UpdateUserRequestModel,
	UserResponseModel
>;

export class UmbUserRepository extends UmbUserRepositoryBase implements UmbUserDetailRepository {
	#detailSource: UmbUserDetailDataSource;
	#setUserGroupsSource: UmbUserSetGroupDataSource;
	#notificationContext?: UmbNotificationContext;

	constructor(host: UmbControllerHost) {
		super(host);

		this.#detailSource = new UmbUserServerDataSource(host);
		this.#setUserGroupsSource = new UmbUserSetGroupsServerDataSource(host);
	}

	// DETAILS
	createScaffold(parentId: string | null) {
		if (parentId === undefined) throw new Error('Parent id is missing');
		return this.#detailSource.createScaffold(parentId);
	}

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

	async byId(id: string) {
		if (!id) throw new Error('Key is missing');
		await this.init;
		return this.detailStore!.byId(id);
	}

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
				data: { message:  'User saved' },
			};
			this.#notificationContext?.peek('positive', notification);
		}

		return { data, error };
	}

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
}
