import { UmbUserCollectionFilterModel, UmbUserDetailDataSource, UmbUserDetailRepository } from '../types';
import { UMB_USER_STORE_CONTEXT_TOKEN, UmbUserStore } from './user.store';
import { UmbUserServerDataSource } from './sources/user.server.data';
import { UmbUserCollectionServerDataSource } from './sources/user-collection.server.data';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbCollectionDataSource, UmbCollectionRepository } from '@umbraco-cms/backoffice/repository';
import {
	CreateUserRequestModel,
	InviteUserRequestModel,
	UpdateUserRequestModel,
	UserResponseModel,
} from '@umbraco-cms/backoffice/backend-api';
import { UmbContextConsumerController } from '@umbraco-cms/backoffice/context-api';
import { UMB_NOTIFICATION_CONTEXT_TOKEN, UmbNotificationContext } from '@umbraco-cms/backoffice/notification';

export class UmbUserRepository implements UmbUserDetailRepository, UmbCollectionRepository {
	#host: UmbControllerHostElement;
	#init;

	#detailSource: UmbUserDetailDataSource;
	#detailStore?: UmbUserStore;

	#collectionSource: UmbCollectionDataSource<UserResponseModel>;

	#notificationContext?: UmbNotificationContext;

	constructor(host: UmbControllerHostElement) {
		this.#host = host;

		this.#detailSource = new UmbUserServerDataSource(this.#host);
		this.#collectionSource = new UmbUserCollectionServerDataSource(this.#host);

		this.#init = Promise.all([
			new UmbContextConsumerController(this.#host, UMB_USER_STORE_CONTEXT_TOKEN, (instance) => {
				this.#detailStore = instance;
			}),

			new UmbContextConsumerController(this.#host, UMB_NOTIFICATION_CONTEXT_TOKEN, (instance) => {
				this.#notificationContext = instance;
			}),
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

		const { data, error } = await this.#detailSource.enable({ userIds: ids });
	}

	async disable(ids: Array<string>) {
		if (ids.length === 0) throw new Error('User ids are missing');

		const { data, error } = await this.#detailSource.disable({ userIds: ids });
	}
}
