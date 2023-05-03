import { Observable } from 'rxjs';
import { UmbUserGroupDetailDataSource } from '../types';
import { UmbUserGroupServerDataSource } from './sources/user-group.server.data';
import { UserGroupBaseModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';
import {
	UmbCollectionRepository,
	UmbDetailRepository,
	UmbRepositoryErrorResponse,
	UmbRepositoryResponse,
} from '@umbraco-cms/backoffice/repository';

// TODO: implement
export class UmbUserGroupRepository implements UmbDetailRepository<UserGroupBaseModel>, UmbCollectionRepository {
	#host: UmbControllerHostElement;

	#detailSource: UmbUserGroupDetailDataSource;

	constructor(host: UmbControllerHostElement) {
		this.#host = host;
		this.#detailSource = new UmbUserGroupServerDataSource(this.#host);
	}

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
	create(data: any): Promise<UmbRepositoryResponse<any>> {
		throw new Error('Method not implemented.');
	}
	save(id: string, data: any): Promise<UmbRepositoryErrorResponse> {
		throw new Error('Method not implemented.');
	}
	delete(id: string): Promise<UmbRepositoryErrorResponse> {
		throw new Error('Method not implemented.');
	}
	requestCollection(filter?: any): Promise<any> {
		throw new Error('Method not implemented.');
	}
}
