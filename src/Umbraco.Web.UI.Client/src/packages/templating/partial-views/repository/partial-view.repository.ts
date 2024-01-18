import { UmbPartialViewTreeRepository } from '../tree/index.js';
import { UmbPartialViewDetailServerDataSource } from './sources/partial-view-detail.server.data-source.js';
import { UmbPartialViewFolderServerDataSource } from './sources/partial-view-folder.server.data-source.js';
import { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import { type UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbBaseController } from '@umbraco-cms/backoffice/class-api';
import {
	CreateFolderRequestModel,
	CreatePartialViewRequestModel,
	PagedSnippetItemResponseModel,
	PartialViewItemResponseModel,
	PartialViewResponseModel,
	ProblemDetails,
	TextFileResponseModelBaseModel,
	UpdatePartialViewRequestModel,
} from '@umbraco-cms/backoffice/backend-api';
import {
	DataSourceResponse,
	UmbDataSourceErrorResponse,
	UmbDetailRepository,
	UmbFolderRepository,
} from '@umbraco-cms/backoffice/repository';
import { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import { UmbId } from '@umbraco-cms/backoffice/id';

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
		UmbFolderRepository,
		UmbApi
{
	#detailDataSource: UmbPartialViewDetailServerDataSource;
	#folderDataSource: UmbPartialViewFolderServerDataSource;

	// TODO: temp solution until it is automated
	#treeRepository = new UmbPartialViewTreeRepository(this);

	constructor(host: UmbControllerHost) {
		super(host);

		this.#detailDataSource = new UmbPartialViewDetailServerDataSource(this);
		this.#folderDataSource = new UmbPartialViewFolderServerDataSource(this);
	}

	//#region FOLDER
	createFolderScaffold(parentId: string | null) {
		const data = {
			id: UmbId.new(),
			name: '',
			parentId,
		};
		return Promise.resolve({ data });
	}

	async createFolder(requestBody: CreateFolderRequestModel) {
		const req = {
			parentPath: requestBody.parentId,
			name: requestBody.name,
		};

		const promise = this.#folderDataSource.create(req);
		await promise;
		this.#treeRepository.requestTreeItemsOf(requestBody.parentId ? requestBody.parentId : null);
		return promise;
	}

	async requestFolder(unique: string) {
		return this.#folderDataSource.read(unique);
	}

	updateFolder(): any {
		throw new Error('Method not implemented.');
	}

	async deleteFolder(path: string): Promise<{ error?: ProblemDetails | undefined }> {
		const { data } = await this.requestFolder(path);
		const promise = this.#folderDataSource.delete(path);
		await promise;
		this.#treeRepository.requestTreeItemsOf(data?.parentPath ? data?.parentPath : null);
		return promise;
	}
	//#endregion

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
