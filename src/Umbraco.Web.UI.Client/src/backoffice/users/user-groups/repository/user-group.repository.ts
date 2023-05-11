import { Observable } from 'rxjs';
import { UmbUserGroupCollectionFilterModel, UmbUserGroupDetailDataSource } from '../types';
import { UmbUserGroupServerDataSource } from './sources/user-group.server.data';
import { UmbUserGroupCollectionServerDataSource } from './sources/user-group-collection.server.data';
import {
	CreateUserGroupRequestModel,
	UpdateUserGroupRequestModel,
	UserGroupBaseModel,
	UserGroupResponseModel,
} from '@umbraco-cms/backoffice/backend-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';
import {
	UmbCollectionDataSource,
	UmbCollectionRepository,
	UmbDetailRepository,
	UmbRepositoryErrorResponse,
	UmbRepositoryResponse,
} from '@umbraco-cms/backoffice/repository';

// TODO: implement
export class UmbUserGroupRepository
	implements
		UmbDetailRepository<CreateUserGroupRequestModel, any, UpdateUserGroupRequestModel, UserGroupResponseModel>,
		UmbCollectionRepository
{
	#host: UmbControllerHostElement;

	#detailSource: UmbUserGroupDetailDataSource;
	#collectionSource: UmbCollectionDataSource<UserGroupResponseModel>;

	constructor(host: UmbControllerHostElement) {
		this.#host = host;
		this.#detailSource = new UmbUserGroupServerDataSource(this.#host);
		this.#collectionSource = new UmbUserGroupCollectionServerDataSource(this.#host);
	}
	createScaffold(parentId: string | null): Promise<UmbRepositoryResponse<UserGroupBaseModel>> {
		return this.#detailSource.createScaffold(parentId);
	}

	// COLLECTION
	async requestCollection(filter: UmbUserGroupCollectionFilterModel = { skip: 0, take: 100 }) {
		//TODO: missing observable
		return this.#collectionSource.filterCollection(filter);
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

		//TODO Put it in the store

		return { error };
	}

	async save(id: string, userGroup: UserGroupResponseModel) {
		if (!id) throw new Error('UserGroup id is missing');
		if (!userGroup) throw new Error('UserGroup update data is missing');

		const { data, error } = await this.#detailSource.update(id, userGroup);

		//TODO Put it in the store

		return { data, error };
	}

	async delete(id: string): Promise<UmbRepositoryErrorResponse> {
		if (!id) throw new Error('UserGroup id is missing');

		const { error } = await this.#detailSource.delete(id);

		//TODO Update store

		return { error };
	}
}
