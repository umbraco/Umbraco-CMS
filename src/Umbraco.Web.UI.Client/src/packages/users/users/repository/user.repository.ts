import {
	UmbUserCollectionFilterModel,
	UmbUserDetailDataSource,
	UmbUserDetailRepository,
	UmbUserSetGroupDataSource,
} from '../types';
import { UMB_USER_STORE_CONTEXT_TOKEN, UmbUserStore } from './user.store';
import { UmbUserServerDataSource } from './sources/user.server.data';
import { UmbUserCollectionServerDataSource } from './sources/user-collection.server.data';
import { UmbUserItemServerDataSource } from './sources/user-item.server.data';
import { UMB_USER_ITEM_STORE_CONTEXT_TOKEN, UmbUserItemStore } from './user-item.store';
import { UmbUserSetGroupsServerDataSource } from './sources/user-set-group.server.data';
import { UmbUserEnableServerDataSource } from './sources/user-enable.server.data';
import { UmbUserDisableServerDataSource } from './sources/user-disable.server.data';
import { UmbUserUnlockServerDataSource } from './sources/user-unlock.server.data';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import {
	UmbCollectionDataSource,
	UmbCollectionRepository,
	UmbItemDataSource,
	UmbItemRepository,
} from '@umbraco-cms/backoffice/repository';
import {
	CreateUserRequestModel,
	InviteUserRequestModel,
	UpdateUserRequestModel,
	UserItemResponseModel,
	UserResponseModel,
} from '@umbraco-cms/backoffice/backend-api';
import { UmbContextConsumerController } from '@umbraco-cms/backoffice/context-api';
import { UMB_NOTIFICATION_CONTEXT_TOKEN, UmbNotificationContext } from '@umbraco-cms/backoffice/notification';

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

	//ACTIONS
	#enableSource: UmbUserEnableServerDataSource;
	#disableSource: UmbUserDisableServerDataSource;
	#unlockSource: UmbUserUnlockServerDataSource;

	#collectionSource: UmbCollectionDataSource<UserResponseModel>;

	#notificationContext?: UmbNotificationContext;

	constructor(host: UmbControllerHostElement) {
		this.#host = host;

		this.#detailSource = new UmbUserServerDataSource(this.#host);
		this.#collectionSource = new UmbUserCollectionServerDataSource(this.#host);
		this.#enableSource = new UmbUserEnableServerDataSource(this.#host);
		this.#disableSource = new UmbUserDisableServerDataSource(this.#host);
		this.#unlockSource = new UmbUserUnlockServerDataSource(this.#host);
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
	async requestCollection(filter: UmbUserCollectionFilterModel = { skip: 0, take: 100 }) {
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

		const { data, error } = await this.#detailSource.get(id);

		if (data) {
			this.#detailStore?.append(data);
		}

		return { data, error };
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

		const { data: createdData, error } = await this.#detailSource.insert(userRequestData);

		if (createdData && createdData.userId) {
			const { data: user, error } = await this.#detailSource.get(createdData?.userId);

			if (user) {
				this.#detailStore?.append(user);

				const notification = { data: { message: `User created` } };
				this.#notificationContext?.peek('positive', notification);

				const hello = {
					user,
					createData: createdData,
				};

				return { data: hello, error };
			}
		}

		return { error };
	}

	async invite(inviteRequestData: InviteUserRequestModel) {
		if (!inviteRequestData) throw new Error('Data is missing');
		const { data, error } = await this.#detailSource.invite(inviteRequestData);

		return { data, error };
	}

	async save(id: string, user: UpdateUserRequestModel) {
		if (!id) throw new Error('User id is missing');
		if (!user) throw new Error('User update data is missing');

		const { data, error } = await this.#detailSource.update(id, user);

		if (data) {
			this.#detailStore?.append(data);

			const notification = { data: { message: `User saved` } };
			this.#notificationContext?.peek('positive', notification);
		}

		return { data, error };
	}

	async delete(id: string) {
		if (!id) throw new Error('Id is missing');

		const { error } = await this.#detailSource.delete(id);

		if (!error) {
			this.#detailStore?.remove([id]);

			const notification = { data: { message: `User deleted` } };
			this.#notificationContext?.peek('positive', notification);
		}

		return { error };
	}

	async enable(ids: Array<string>) {
		if (ids.length === 0) throw new Error('User ids are missing');

		const { error } = await this.#enableSource.enable(ids);

		if (!error) {
			//TODO: UPDATE STORE
			const notification = { data: { message: `${ids.length > 1 ? 'Users' : 'User'} enabled` } };
			this.#notificationContext?.peek('positive', notification);
		}
	}

	async disable(ids: Array<string>) {
		if (ids.length === 0) throw new Error('User ids are missing');

		const { error } = await this.#disableSource.disable(ids);

		if (!error) {
			//TODO: UPDATE STORE
			const notification = { data: { message: `${ids.length > 1 ? 'Users' : 'User'} disabled` } };
			this.#notificationContext?.peek('positive', notification);
		}
	}

	async unlock(ids: Array<string>) {
		if (ids.length === 0) throw new Error('User ids are missing');

		const { error } = await this.#unlockSource.unlock(ids);

		if (!error) {
			//TODO: UPDATE STORE
			const notification = { data: { message: `${ids.length > 1 ? 'Users' : 'User'} unlocked` } };
			this.#notificationContext?.peek('positive', notification);
		}
	}
}
