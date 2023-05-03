import { Observable } from 'rxjs';
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

	constructor(host: UmbControllerHostElement) {
		this.#host = host;
	}

	createScaffold(parentId: string | null): Promise<UmbRepositoryResponse<any>> {
		throw new Error('Method not implemented.');
	}
	requestById(id: string): Promise<UmbRepositoryResponse<any>> {
		throw new Error('Method not implemented.');
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
