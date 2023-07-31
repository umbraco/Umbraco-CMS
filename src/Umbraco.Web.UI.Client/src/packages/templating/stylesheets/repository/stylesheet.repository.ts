import { UmbStylesheetTreeStore, UMB_STYLESHEET_TREE_STORE_CONTEXT_TOKEN } from './stylesheet.tree.store.js';
import { UmbStylesheetTreeServerDataSource } from './sources/stylesheet.tree.server.data.js';
import { UmbStylesheetServerDataSource } from './sources/stylesheet.server.data.js';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextConsumerController } from '@umbraco-cms/backoffice/context-api';
import {
	DataSourceResponse,
	UmbDataSourceErrorResponse,
	UmbDetailRepository,
	UmbFolderRepository,
	UmbTreeRepository,
} from '@umbraco-cms/backoffice/repository';
import {
	CreateFolderRequestModel,
	CreateStylesheetRequestModel,
	CreateTextFileViewModelBaseModel,
	FileSystemTreeItemPresentationModel,
	FolderModelBaseModel,
	FolderReponseModel,
	ProblemDetails,
	TextFileResponseModelBaseModel,
	UpdateStylesheetRequestModel,
	UpdateTextFileViewModelBaseModel,
} from '@umbraco-cms/backoffice/backend-api';
import type { UmbTreeRootFileSystemModel } from '@umbraco-cms/backoffice/tree';
import { StylesheetDetails } from '../index.js';
import { Observable } from 'rxjs';
import {
	StylesheetGetFolderResponse,
	UmbStylesheetFolderServerDataSource,
} from './sources/stylesheet.folder.server.data.js';

export class UmbStylesheetRepository
	implements
		UmbTreeRepository<FileSystemTreeItemPresentationModel, UmbTreeRootFileSystemModel>,
		UmbDetailRepository<CreateStylesheetRequestModel, string, UpdateStylesheetRequestModel, StylesheetDetails>,
		UmbFolderRepository
{
	#host;
	#dataSource;
	#treeDataSource;
	#treeStore?: UmbStylesheetTreeStore;
	#folderDataSource;
	#init;

	constructor(host: UmbControllerHostElement) {
		this.#host = host;

		// TODO: figure out how spin up get the correct data source
		this.#dataSource = new UmbStylesheetServerDataSource(this.#host);
		this.#treeDataSource = new UmbStylesheetTreeServerDataSource(this.#host);
		this.#folderDataSource = new UmbStylesheetFolderServerDataSource(this.#host);

		this.#init = new UmbContextConsumerController(this.#host, UMB_STYLESHEET_TREE_STORE_CONTEXT_TOKEN, (instance) => {
			this.#treeStore = instance;
		}).asPromise();
	}

	//#region FOLDER:

	createFolderScaffold(
		parentId: string | null
	): Promise<{ data?: FolderReponseModel | undefined; error?: ProblemDetails | undefined }> {
		const data: FolderReponseModel = {
			name: '',
			parentId,
		};
		return Promise.resolve({ data, error: undefined });
	}

	async createFolder(
		folderRequest: CreateFolderRequestModel
	): Promise<{ data?: string | undefined; error?: ProblemDetails | undefined }> {
		await this.#init;
		const req = {
			parentPath: folderRequest.parentId,
			name: folderRequest.name,
		};
		const promise = this.#folderDataSource.insert(req);
		await promise;
		this.requestTreeItemsOf(folderRequest.parentId ? folderRequest.parentId : null);
		return promise;
	}
	async requestFolder(
		unique: string
	): Promise<{ data?: StylesheetGetFolderResponse | undefined; error?: ProblemDetails | undefined }> {
		await this.#init;
		return this.#folderDataSource.get(unique);
	}
	updateFolder(
		unique: string,
		folder: FolderModelBaseModel
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

	//#region DETAIL:

	createScaffold(
		parentId: string | null,
		preset?: Partial<CreateTextFileViewModelBaseModel> | undefined
	): Promise<DataSourceResponse<CreateTextFileViewModelBaseModel>> {
		throw new Error('Method not implemented.');
	}

	async requestById(id: string): Promise<DataSourceResponse<TextFileResponseModelBaseModel | undefined>> {
		if (!id) throw new Error('id is missing');
		await this.#init;
		const { data, error } = await this.#dataSource.get(id);
		return { data, error };
	}
	byId(id: string): Promise<Observable<TextFileResponseModelBaseModel | undefined>> {
		throw new Error('Method not implemented.');
	}
	create(data: CreateTextFileViewModelBaseModel): Promise<DataSourceResponse<string>> {
		throw new Error('Method not implemented.');
	}
	save(id: string, data: UpdateTextFileViewModelBaseModel): Promise<UmbDataSourceErrorResponse> {
		throw new Error('Method not implemented.');
	}
	delete(id: string): Promise<UmbDataSourceErrorResponse> {
		throw new Error('Method not implemented.');
	}

	//#endregion

	//#region TREE:
	async requestTreeRoot() {
		await this.#init;

		const data = {
			path: null,
			type: 'stylesheet-root',
			name: 'Stylesheets',
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

		return { data, error };
	}

	async requestTreeItemsOf(path: string | null) {
		if (path === undefined) throw new Error('Cannot request tree item with missing path');

		await this.#init;

		const { data, error } = await this.#treeDataSource.getChildrenOf(path);

		if (data) {
			this.#treeStore!.appendItems(data.items);
		}

		return { data, error, asObservable: () => this.#treeStore!.childrenOf(path) };
	}

	async requestItemsLegacy(paths: Array<string>) {
		if (!paths) throw new Error('Paths are missing');
		await this.#init;
		const { data, error } = await this.#treeDataSource.getItems(paths);
		return { data, error };
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

	async itemsLegacy(paths: Array<string>) {
		if (!paths) throw new Error('Paths are missing');
		await this.#init;
		return this.#treeStore!.items(paths);
	}

	//#endregion
}
