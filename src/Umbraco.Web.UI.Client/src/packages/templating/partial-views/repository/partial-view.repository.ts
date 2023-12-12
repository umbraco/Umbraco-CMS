import { UmbPartialViewTreeRepository } from '../tree/index.js';
import { UmbPartialViewDetailServerDataSource } from './sources/partial-view-detail.server.data-source.js';
import { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import { type UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbBaseController } from '@umbraco-cms/backoffice/class-api';
import {
	CreatePartialViewRequestModel,
	PagedSnippetItemResponseModel,
	PartialViewItemResponseModel,
	PartialViewResponseModel,
	TextFileResponseModelBaseModel,
	UpdatePartialViewRequestModel,
} from '@umbraco-cms/backoffice/backend-api';
import {
	DataSourceResponse,
	UmbDataSourceErrorResponse,
	UmbDetailRepository,
} from '@umbraco-cms/backoffice/repository';
import { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export class UmbPartialViewRepository
	extends UmbBaseController
	implements
		UmbDetailRepository<
			CreatePartialViewRequestModel,
			string,
			UpdatePartialViewRequestModel,
			PartialViewResponseModel,
			string
		>,
		UmbApi
{
	#detailDataSource: UmbPartialViewDetailServerDataSource;

	// TODO: temp solution until it is automated
	#treeRepository = new UmbPartialViewTreeRepository(this);

	constructor(host: UmbControllerHostElement) {
		super(host);
		this.#detailDataSource = new UmbPartialViewDetailServerDataSource(this);
	}

	//#region DETAILS
	async requestByKey(path: string) {
		if (!path) throw new Error('Path is missing');
		const { data, error } = await this.#detailDataSource.read(path);
		return { data, error };
	}

	requestById(id: string): Promise<DataSourceResponse<any>> {
		throw new Error('Method not implemented.');
	}

	byId(id: string): Promise<Observable<any>> {
		throw new Error('Method not implemented.');
	}

	createScaffold(parentId: string | null, preset: string): Promise<DataSourceResponse<TextFileResponseModelBaseModel>> {
		return this.#detailDataSource.createScaffold(parentId, preset);
	}

	async create(data: CreatePartialViewRequestModel): Promise<DataSourceResponse<any>> {
		const promise = this.#detailDataSource.create(data);
		await promise;
		this.#treeRepository.requestTreeItemsOf(data.parentPath ? data.parentPath : null);
		return promise;
	}

	save(id: string, requestBody: UpdatePartialViewRequestModel): Promise<UmbDataSourceErrorResponse> {
		return this.#detailDataSource.update(id, requestBody);
	}

	async delete(id: string): Promise<UmbDataSourceErrorResponse> {
		const promise = this.#detailDataSource.delete(id);
		const parentPath = id.substring(0, id.lastIndexOf('/'));
		this.#treeRepository.requestTreeItemsOf(parentPath ? parentPath : null);
		return promise;
	}

	getSnippets({ skip = 0, take = 100 }): Promise<DataSourceResponse<PagedSnippetItemResponseModel>> {
		return this.#detailDataSource.getSnippets({ skip, take });
	}

	requestItems(keys: Array<string>): Promise<DataSourceResponse<PartialViewItemResponseModel[]>> {
		return this.#detailDataSource.getItems(keys);
	}

	//#endregion
}
