import {
	UmbUserCollectionFilterModel,
	UmbUserDetail,
	UmbUserDetailDataSource,
	UmbUserSetGroupDataSource,
} from '../types.js';

import { UMB_USER_STORE_CONTEXT_TOKEN, UmbUserStore } from './user.store.js';
import { UmbUserServerDataSource } from './sources/user.server.data.js';
import { UmbUserCollectionServerDataSource } from './sources/user-collection.server.data.js';
import { UmbUserItemServerDataSource } from './sources/user-item.server.data.js';
import { UMB_USER_ITEM_STORE_CONTEXT_TOKEN, UmbUserItemStore } from './user-item.store.js';
import { UmbUserSetGroupsServerDataSource } from './sources/user-set-group.server.data.js';

import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import {
	UmbCollectionDataSource,
	UmbCollectionRepository,
	UmbDetailRepository,
	UmbItemDataSource,
	UmbItemRepository,
} from '@umbraco-cms/backoffice/repository';
import {
	CreateUserRequestModel,
	CreateUserResponseModel,
	UpdateUserRequestModel,
	UserItemResponseModel,
	UserResponseModel,
} from '@umbraco-cms/backoffice/backend-api';
import { UmbContextConsumerController } from '@umbraco-cms/backoffice/context-api';
import { UMB_NOTIFICATION_CONTEXT_TOKEN, UmbNotificationContext } from '@umbraco-cms/backoffice/notification';

export type UmbUserDetailRepository = UmbDetailRepository<
	CreateUserRequestModel,
	CreateUserResponseModel,
	UpdateUserRequestModel,
	UserResponseModel
>;

export class UmbUserRepository
	implements UmbUserDetailRepository, UmbCollectionRepository, UmbItemRepository<UserItemResponseModel>
{
	#host: UmbControllerHostElement;
	#init;

	#detailSource: UmbUserDetailDataSource;
	#detailStore?: UmbUserStore;
	#itemSource: UmbItemDataSource<UserItemResponseModel>;
	#itemStore?: UmbUserItemStore;
	#setUserGroupsSource: UmbUserSetGroupDataSource;

	#collectionSource: UmbCollectionDataSource<UmbUserDetail>;

	#notificationContext?: UmbNotificationContext;

	constructor(host: UmbControllerHostElement) {
		this.#host = host;

		this.#detailSource = new UmbUserServerDataSource(this.#host);
		this.#collectionSource = new UmbUserCollectionServerDataSource(this.#host);
		this.#itemSource = new UmbUserItemServerDataSource(this.#host);
		this.#setUserGroupsSource = new UmbUserSetGroupsServerDataSource(this.#host);

		this.#init = Promise.all([
			new UmbContextConsumerController(this.#host, UMB_USER_STORE_CONTEXT_TOKEN, (instance) => {
				this.#detailStore = instance;
			}).asPromise(),

			new UmbContextConsumerController(this.#host, UMB_USER_ITEM_STORE_CONTEXT_TOKEN, (instance) => {
				this.#itemStore = instance;
			}).asPromise(),

			new UmbContextConsumerController(this.#host, UMB_NOTIFICATION_CONTEXT_TOKEN, (instance) => {
				this.#notificationContext = instance;
			}).asPromise(),
		]);
	}

	// COLLECTION
	async requestCollection(filter: UmbUserCollectionFilterModel = { skip: 0, take: 100000 }) {
		//TODO: missing observable
		return this.#collectionSource.filterCollection(filter);
	}

	async filterCollection(filter: UmbUserCollectionFilterModel) {
		return this.#collectionSource.filterCollection(filter);
	}

	// ITEMS:
	async requestItems(ids: Array<string>) {
		if (!ids) throw new Error('Ids are missing');
		await this.#init;

		const { data, error } = await this.#itemSource.getItems(ids);

		if (data) {
			this.#itemStore?.appendItems(data);
		}

		return { data, error, asObservable: () => this.#itemStore!.items(ids) };
	}

	async items(ids: Array<string>) {
		await this.#init;
		return this.#itemStore!.items(ids);
	}

	// DETAILS
	createScaffold(parentId: string | null) {
		if (parentId === undefined) throw new Error('Parent id is missing');
		return this.#detailSource.createScaffold(parentId);
	}

	async requestById(id: string) {
		if (!id) throw new Error('Id is missing');
		await this.#init;

		const { data, error } = await this.#detailSource.get(id);

		if (data) {
			this.#detailStore!.append(data);
		}

		return { data, error, asObservable: () => this.#detailStore!.byId(id) };
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
		await this.#init;
		return this.#detailStore!.byId(id);
	}

	async create(userRequestData: CreateUserRequestModel) {
		if (!userRequestData) throw new Error('Data is missing');

		const { data, error } = await this.#detailSource.insert(userRequestData);

		if (data) {
			this.#detailStore?.append(data);

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
			this.#detailStore?.append(data);
		}

		if (!error) {
			const notification = {
				data: { message: this.#host.localize?.term('speechBubbles_editUserSaved') ?? 'User saved' },
			};
			this.#notificationContext?.peek('positive', notification);
		}

		return { data, error };
	}

	async delete(id: string) {
		if (!id) throw new Error('Id is missing');

		const { error } = await this.#detailSource.delete(id);

		if (!error) {
			this.#detailStore?.removeItem(id);

			const notification = { data: { message: `User deleted` } };
			this.#notificationContext?.peek('positive', notification);
		}

		return { error };
	}
}
