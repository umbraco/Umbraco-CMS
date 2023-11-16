import { UmbScriptTreeRepository } from '../tree/index.js';
import { UmbScriptServerDataSource } from './sources/script-detail.server.data.js';
import { ScriptGetFolderResponse, UmbScriptFolderServerDataSource } from './sources/script-folder.server.data.js';
import {
	DataSourceResponse,
	UmbDataSourceErrorResponse,
	UmbDetailRepository,
	UmbFolderRepository,
} from '@umbraco-cms/backoffice/repository';
import {
	CreateFolderRequestModel,
	CreateScriptRequestModel,
	FileItemResponseModelBaseModel,
	FolderModelBaseModel,
	FolderResponseModel,
	ProblemDetails,
	ScriptResponseModel,
	TextFileResponseModelBaseModel,
	UpdateScriptRequestModel,
} from '@umbraco-cms/backoffice/backend-api';
import { UmbBaseController, UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export class UmbScriptRepository
	extends UmbBaseController
	implements
		UmbDetailRepository<CreateScriptRequestModel, string, UpdateScriptRequestModel, ScriptResponseModel, string>,
		UmbFolderRepository,
		UmbApi
{
	#detailDataSource: UmbScriptServerDataSource;
	#folderDataSource: UmbScriptFolderServerDataSource;

	// TODO: temp solution until it is automated
	#treeRepository = new UmbScriptTreeRepository(this);

	constructor(host: UmbControllerHost) {
		super(host);

		this.#detailDataSource = new UmbScriptServerDataSource(this);
		this.#folderDataSource = new UmbScriptFolderServerDataSource(this);
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
		const req = {
			parentPath: requestBody.parentId,
			name: requestBody.name,
		};
		const promise = this.#folderDataSource.create(req);
		await promise;
		this.#treeRepository.requestTreeItemsOf(requestBody.parentId ? requestBody.parentId : null);
		return promise;
	}
	async requestFolder(
		unique: string,
	): Promise<{ data?: ScriptGetFolderResponse | undefined; error?: ProblemDetails | undefined }> {
		return this.#folderDataSource.read(unique);
	}
	updateFolder(
		unique: string,
		folder: FolderModelBaseModel,
	): Promise<{ data?: FolderModelBaseModel | undefined; error?: ProblemDetails | undefined }> {
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
	async create(data: CreateScriptRequestModel): Promise<DataSourceResponse<any>> {
		const promise = this.#detailDataSource.create(data);
		await promise;
		this.#treeRepository.requestTreeItemsOf(data.parentPath ? data.parentPath : null);
		return promise;
	}
	save(id: string, requestBody: UpdateScriptRequestModel): Promise<UmbDataSourceErrorResponse> {
		return this.#detailDataSource.update(id, requestBody);
	}
	async delete(id: string): Promise<UmbDataSourceErrorResponse> {
		const promise = this.#detailDataSource.delete(id);
		const parentPath = id.substring(0, id.lastIndexOf('/'));
		this.#treeRepository.requestTreeItemsOf(parentPath ? parentPath : null);
		return promise;
	}

	requestItems(keys: Array<string>): Promise<DataSourceResponse<FileItemResponseModelBaseModel[]>> {
		return this.#detailDataSource.getItems(keys);
	}

	//#endregion
}
