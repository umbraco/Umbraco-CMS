import { UmbPartialViewDetailServerDataSource } from './sources/partial-views.detail.server.data.js';
import { UmbPartialViewTreeStore, UMB_PARTIAL_VIEW_TREE_STORE_CONTEXT_TOKEN } from './partial-views.tree.store.js';
import {
	PartialViewGetFolderResponse,
	UmbPartialViewFolderServerDataSource,
} from './sources/partial-views.folder.server.data.js';
import { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import { UmbBaseController, UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import {
	CreateFolderRequestModel,
	CreatePartialViewRequestModel,
	FolderModelBaseModel,
	FolderResponseModel,
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
	#init;

	#detailDataSource: UmbPartialViewDetailServerDataSource;
	#folderDataSource: UmbPartialViewFolderServerDataSource;

	#treeStore?: UmbPartialViewTreeStore;

	constructor(host: UmbControllerHostElement) {
		super(host);

		this.#detailDataSource = new UmbPartialViewDetailServerDataSource(this);
		this.#folderDataSource = new UmbPartialViewFolderServerDataSource(this);

		this.#init = this.consumeContext(UMB_PARTIAL_VIEW_TREE_STORE_CONTEXT_TOKEN, (instance) => {
			this.#treeStore = instance;
		}).asPromise();
	}

	//#region FOLDER
	createFolderScaffold(
		parentId: string | null,
	): Promise<{ data?: FolderResponseModel | undefined; error?: ProblemDetails | undefined }> {
		const data: FolderResponseModel = {
			name: '',
			parentId,
		};
		return Promise.resolve({ data, error: undefined });
	}
	async createFolder(
		requestBody: CreateFolderRequestModel,
	): Promise<{ data?: string | undefined; error?: ProblemDetails | undefined }> {
		await this.#init;
		const req = {
			parentPath: requestBody.parentId,
			name: requestBody.name,
		};
		const promise = this.#folderDataSource.create(req);
		await promise;
		this.requestTreeItemsOf(requestBody.parentId ? requestBody.parentId : null);
		return promise;
	}
	async requestFolder(
		unique: string,
	): Promise<{ data?: PartialViewGetFolderResponse | undefined; error?: ProblemDetails | undefined }> {
		await this.#init;
		return this.#folderDataSource.read(unique);
	}
	updateFolder(
		unique: string,
		folder: FolderModelBaseModel,
	): Promise<{ data?: FolderModelBaseModel | undefined; error?: ProblemDetails | undefined }> {
		throw new Error('Method not implemented.');
	}
	async deleteFolder(path: string): Promise<{ error?: ProblemDetails | undefined }> {
		await this.#init;
		const { data } = await this.requestFolder(path);
		const promise = this.#folderDataSource.delete(path);
		await promise;
		this.requestTreeItemsOf(data?.parentPath ? data?.parentPath : null);
		return promise;
	}
	//#endregion

	//#region DETAILS
	async requestByKey(path: string) {
		if (!path) throw new Error('Path is missing');
		await this.#init;
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
		this.requestTreeItemsOf(data.parentPath ? data.parentPath : null);
		return promise;
	}
	save(id: string, requestBody: UpdatePartialViewRequestModel): Promise<UmbDataSourceErrorResponse> {
		return this.#detailDataSource.update(id, requestBody);
	}
	async delete(id: string): Promise<UmbDataSourceErrorResponse> {
		const promise = this.#detailDataSource.delete(id);
		const parentPath = id.substring(0, id.lastIndexOf('/'));
		this.requestTreeItemsOf(parentPath ? parentPath : null);
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
