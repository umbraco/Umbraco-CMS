import { Observable } from 'rxjs';
import { UmbUserGroupCollectionFilterModel, UmbUserGroupDetailDataSource } from '../types';
import { UmbUserGroupServerDataSource } from './sources/user-group.server.data';
import { UmbUserGroupCollectionServerDataSource } from './sources/user-group-collection.server.data';
import { UserGroupPresentationModel } from '@umbraco-cms/backoffice/backend-api';
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
	implements UmbDetailRepository<UserGroupPresentationModel>, UmbCollectionRepository
{
	#host: UmbControllerHostElement;

	#detailSource: UmbUserGroupDetailDataSource;
	#collectionSource: UmbCollectionDataSource<UserGroupPresentationModel>;

	constructor(host: UmbControllerHostElement) {
		this.#host = host;
		this.#detailSource = new UmbUserGroupServerDataSource(this.#host);
		this.#collectionSource = new UmbUserGroupCollectionServerDataSource(this.#host);
	}

	// COLLECTION
	async requestCollection(filter: UmbUserGroupCollectionFilterModel = { skip: 0, take: 100 }) {
		//TODO: missing observable
		return this.#collectionSource.filterCollection(filter);
	}

	// DETAIL
	createScaffold(parentId: string | null): Promise<UmbRepositoryResponse<any>> {
		throw new Error('Method not implemented.');
	}

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

	async save(id: string, userGroup: UserGroupPresentationModel) {
		if (!id) throw new Error('UserGroup id is missing');
		if (!userGroup) throw new Error('UserGroup update data is missing');

		const { data, error } = await this.#detailSource.update(id, userGroup);

		//TODO Put it in the store

		return { data, error };
	}

	delete(id: string): Promise<UmbRepositoryErrorResponse> {
		throw new Error('Method not implemented.');
	}
}
