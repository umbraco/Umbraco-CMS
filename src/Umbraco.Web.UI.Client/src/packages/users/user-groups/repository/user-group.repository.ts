import { Observable } from 'rxjs';
import { UmbUserGroupCollectionFilterModel, UmbUserGroupDetailDataSource } from '../types';
import { UmbUserGroupServerDataSource } from './sources/user-group.server.data';
import { UmbUserGroupCollectionServerDataSource } from './sources/user-group-collection.server.data';
import { UMB_USER_GROUP_ITEM_STORE_CONTEXT_TOKEN, UmbUserGroupItemStore } from './user-group-item.store';
import { UMB_USER_GROUP_STORE_CONTEXT_TOKEN, UmbUserGroupStore } from './user-group.store';
import { UmbUserGroupItemServerDataSource } from './sources/user-group-item.server.data';
import {
	CreateUserGroupRequestModel,
	UpdateUserGroupRequestModel,
	UserGroupBaseModel,
	UserGroupItemResponseModel,
	UserGroupResponseModel,
} from '@umbraco-cms/backoffice/backend-api';
import {
	UmbCollectionDataSource,
	UmbCollectionRepository,
	UmbDetailRepository,
	UmbItemDataSource,
	UmbItemRepository,
	UmbRepositoryErrorResponse,
	UmbRepositoryResponse,
} from '@umbraco-cms/backoffice/repository';
import { UmbContextConsumerController } from '@umbraco-cms/backoffice/context-api';
import { UMB_NOTIFICATION_CONTEXT_TOKEN, UmbNotificationContext } from 'src/packages/core/notification';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';

// TODO: implement
export class UmbUserGroupRepository
	implements
		UmbDetailRepository<CreateUserGroupRequestModel, any, UpdateUserGroupRequestModel, UserGroupResponseModel>,
		UmbCollectionRepository,
		UmbItemRepository<UserGroupItemResponseModel>
{
	#host: UmbControllerHostElement;
	#init;

	#detailSource: UmbUserGroupDetailDataSource;
	#detailStore?: UmbUserGroupStore;

	#collectionSource: UmbCollectionDataSource<UserGroupResponseModel>;

	#itemSource: UmbItemDataSource<UserGroupItemResponseModel>;
	#itemStore?: UmbUserGroupItemStore;

	#notificationContext?: UmbNotificationContext;

	constructor(host: UmbControllerHostElement) {
		this.#host = host;
		this.#detailSource = new UmbUserGroupServerDataSource(this.#host);
		this.#itemSource = new UmbUserGroupItemServerDataSource(this.#host);
		this.#collectionSource = new UmbUserGroupCollectionServerDataSource(this.#host);

		new UmbContextConsumerController(this.#host, UMB_NOTIFICATION_CONTEXT_TOKEN, (instance) => {
			this.#notificationContext = instance;
		});

		this.#init = Promise.all([
			new UmbContextConsumerController(this.#host, UMB_USER_GROUP_STORE_CONTEXT_TOKEN, (instance) => {
				this.#detailStore = instance;
			}).asPromise(),

			new UmbContextConsumerController(this.#host, UMB_USER_GROUP_ITEM_STORE_CONTEXT_TOKEN, (instance) => {
				this.#itemStore = instance;
			}).asPromise(),

			new UmbContextConsumerController(this.#host, UMB_NOTIFICATION_CONTEXT_TOKEN, (instance) => {
				this.#notificationContext = instance;
			}).asPromise(),
		]);
	}
	createScaffold(parentId: string | null): Promise<UmbRepositoryResponse<UserGroupBaseModel>> {
		return this.#detailSource.createScaffold(parentId);
	}

	// COLLECTION
	async requestCollection(filter: UmbUserGroupCollectionFilterModel = { skip: 0, take: 100 }) {
		//TODO: missing observable
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

	// DETAIL
	async requestById(id: string) {
		if (!id) throw new Error('Id is missing');

		const { data, error } = await this.#detailSource.get(id);

		//TODO Put it in the store

		return { data, error };
	}

	byId(id: string): Promise<Observable<any>> {
		throw new Error('Method not implemented.');
	}

	async create(userGroupRequestData: any): Promise<UmbRepositoryResponse<any>> {
		if (!userGroupRequestData) throw new Error('Data is missing');

		const { data, error } = await this.#detailSource.insert(userGroupRequestData);

		//TODO Update store

		if (!error) {
			const notification = { data: { message: `User group created` } };
			this.#notificationContext?.peek('positive', notification);
		}

		return { error };
	}

	async save(id: string, userGroup: UserGroupResponseModel) {
		if (!id) throw new Error('UserGroup id is missing');
		if (!userGroup) throw new Error('UserGroup update data is missing');

		const { data, error } = await this.#detailSource.update(id, userGroup);

		//TODO Update store

		if (!error) {
			const notification = { data: { message: `User group saved` } };
			this.#notificationContext?.peek('positive', notification);
		}

		return { data, error };
	}

	async delete(id: string): Promise<UmbRepositoryErrorResponse> {
		if (!id) throw new Error('UserGroup id is missing');

		const { error } = await this.#detailSource.delete(id);

		//TODO Update store

		if (!error) {
			const notification = { data: { message: `User group deleted` } };
			this.#notificationContext?.peek('positive', notification);
		}

		return { error };
	}
}
