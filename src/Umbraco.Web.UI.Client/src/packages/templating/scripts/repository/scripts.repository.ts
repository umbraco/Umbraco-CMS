import { PARTIAL_VIEW_ROOT_ENTITY_TYPE } from '../../partial-views/config.js';
import { UMB_PARTIAL_VIEW_TREE_STORE_CONTEXT_TOKEN } from '../../partial-views/repository/partial-views.tree.store.js';
import { PartialViewGetFolderResponse } from '../../partial-views/repository/sources/partial-views.folder.server.data.js';
import { UmbScriptsTreeServerDataSource } from './sources/scripts.tree.server.data.js';
import { UmbScriptsServerDataSource } from './sources/scripts.detail.server.data.js';
import { UmbScriptsFolderServerDataSource } from './sources/scripts.folder.server.data.js';
import { UmbScriptsTreeStore } from './scripts.tree.store.js';
import {
	DataSourceResponse,
	UmbDataSourceErrorResponse,
	UmbDetailRepository,
	UmbFolderRepository,
	UmbTreeRepository,
} from '@umbraco-cms/backoffice/repository';
import {
	CreateFolderRequestModel,
	CreateScriptRequestModel,
	FileItemResponseModelBaseModel,
	FileSystemTreeItemPresentationModel,
	FolderModelBaseModel,
	FolderResponseModel,
	ProblemDetails,
	ScriptResponseModel,
	TextFileResponseModelBaseModel,
	UpdateScriptRequestModel,
} from '@umbraco-cms/backoffice/backend-api';
import { UmbContextConsumerController } from '@umbraco-cms/backoffice/context-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { Observable } from '@umbraco-cms/backoffice/external/rxjs';

export class UmbScriptsRepository
	implements
		UmbTreeRepository<FileSystemTreeItemPresentationModel>,
		UmbDetailRepository<CreateScriptRequestModel, string, UpdateScriptRequestModel, ScriptResponseModel, string>,
		UmbFolderRepository
{
	#init;
	#host: UmbControllerHostElement;

	#treeDataSource: UmbScriptsTreeServerDataSource;
	#detailDataSource: UmbScriptsServerDataSource;
	#folderDataSource: UmbScriptsFolderServerDataSource;

	#treeStore?: UmbScriptsTreeStore;

	constructor(host: UmbControllerHostElement) {
		this.#host = host;

		this.#treeDataSource = new UmbScriptsTreeServerDataSource(this.#host);
		this.#detailDataSource = new UmbScriptsServerDataSource(this.#host);
		this.#folderDataSource = new UmbScriptsFolderServerDataSource(this.#host);

		this.#init = Promise.all([
			new UmbContextConsumerController(this.#host, UMB_PARTIAL_VIEW_TREE_STORE_CONTEXT_TOKEN, (instance) => {
				this.#treeStore = instance;
			}),
		]);
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
		const promise = this.#folderDataSource.insert(req);
		await promise;
		this.requestTreeItemsOf(requestBody.parentId ? requestBody.parentId : null);
		return promise;
	}
	async requestFolder(
		unique: string,
	): Promise<{ data?: PartialViewGetFolderResponse | undefined; error?: ProblemDetails | undefined }> {
		await this.#init;
		return this.#folderDataSource.get(unique);
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

	//#region TREE

	async requestTreeRoot() {
		await this.#init;

		const data = {
			id: null,
			path: null,
			type: PARTIAL_VIEW_ROOT_ENTITY_TYPE,
			name: 'Partial Views',
			icon: 'umb:folder',
			hasChildren: true,
		};
		return { data };
	}

	async requestRootTreeItems() {
		await this.#init;

		const { data, error } = await this.#treeDataSource.getRootItems();

		if (data) {
			this.#treeStore?.appendItems(data.items);
		}

		return { data, error, asObservable: () => this.#treeStore!.rootItems };
	}

	async requestTreeItemsOf(path: string | null) {
		if (path === null) {
			return this.requestRootTreeItems();
		}

		await this.#init;

		const { data, error } = await this.#treeDataSource.getChildrenOf({ path, skip: 0, take: 100 });
		if (data) {
			this.#treeStore!.appendItems(data.items);
		}

		return { data, error, asObservable: () => this.#treeStore!.childrenOf(path) };
	}

	async requestTreeItems(keys: Array<string>) {
		await this.#init;

		if (!keys) {
			const error: ProblemDetails = { title: 'Keys are missing' };
			return { data: undefined, error };
		}

		const { data, error } = await this.#treeDataSource.getItem(keys);

		return { data, error, asObservable: () => this.#treeStore!.items(keys) };
	}

	async rootTreeItems() {
		await this.#init;
		return this.#treeStore!.rootItems;
	}

	async treeItemsOf(parentPath: string | null) {
		if (!parentPath) throw new Error('Parent Path is missing');
		await this.#init;
		return this.#treeStore!.childrenOf(parentPath);
	}

	async treeItems(paths: Array<string>) {
		if (!paths) throw new Error('Paths are missing');
		await this.#init;
		return this.#treeStore!.items(paths);
	}
	//#endregion

	//#region DETAILS
	async requestByKey(path: string) {
		if (!path) throw new Error('Path is missing');
		await this.#init;
		const { data, error } = await this.#detailDataSource.get(path);
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
		const promise = this.#detailDataSource.insert(data);
		await promise;
		this.requestTreeItemsOf(data.parentPath ? data.parentPath : null);
		return promise;
	}
	save(id: string, requestBody: UpdateScriptRequestModel): Promise<UmbDataSourceErrorResponse> {
		return this.#detailDataSource.update(id, requestBody);
	}
	async delete(id: string): Promise<UmbDataSourceErrorResponse> {
		const promise = this.#detailDataSource.delete(id);
		const parentPath = id.substring(0, id.lastIndexOf('/'));
		this.requestTreeItemsOf(parentPath ? parentPath : null);
		return promise;
	}

	requestItems(keys: Array<string>): Promise<DataSourceResponse<FileItemResponseModelBaseModel[]>> {
		return this.#detailDataSource.getItems(keys);
	}

	//#endregion
}
