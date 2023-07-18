import { PARTIAL_VIEW_ROOT_ENTITY_TYPE, PartialViewDetails } from '../config.js';
import { UmbPartialViewDetailServerDataSource } from './sources/partial-views.detail.server.data.js';
import { UmbPartialViewsTreeServerDataSource } from './sources/partial-views.tree.server.data.js';
import { UmbPartialViewsTreeStore, UMB_PARTIAL_VIEW_TREE_STORE_CONTEXT_TOKEN } from './partial-views.tree.store.js';
import { UmbPartialViewsFolderServerDataSource } from './sources/partial-views.folder.server.data.js';
import { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextConsumerController } from '@umbraco-cms/backoffice/context-api';
import {
	CreateFolderRequestModel,
	FileSystemTreeItemPresentationModel,
	FolderModelBaseModel,
	FolderReponseModel,
	ProblemDetails,
} from '@umbraco-cms/backoffice/backend-api';
import {
	DataSourceResponse,
	UmbDetailRepository,
	UmbFolderRepository,
	UmbTreeRepository,
} from '@umbraco-cms/backoffice/repository';

export class UmbPartialViewsRepository
	implements
		UmbTreeRepository<FileSystemTreeItemPresentationModel>,
		UmbDetailRepository<PartialViewDetails>,
		UmbFolderRepository
{
	#init;
	#host: UmbControllerHostElement;

	#treeDataSource: UmbPartialViewsTreeServerDataSource;
	#detailDataSource: UmbPartialViewDetailServerDataSource;
	#folderDataSource: UmbPartialViewsFolderServerDataSource;

	#treeStore?: UmbPartialViewsTreeStore;

	constructor(host: UmbControllerHostElement) {
		this.#host = host;

		this.#treeDataSource = new UmbPartialViewsTreeServerDataSource(this.#host);
		this.#detailDataSource = new UmbPartialViewDetailServerDataSource(this.#host);
		this.#folderDataSource = new UmbPartialViewsFolderServerDataSource(this.#host);

		this.#init = Promise.all([
			new UmbContextConsumerController(this.#host, UMB_PARTIAL_VIEW_TREE_STORE_CONTEXT_TOKEN, (instance) => {
				this.#treeStore = instance;
			}),
		]);
	}

	//#region FOLDER
	createFolderScaffold(
		parentId: string | null
	): Promise<{ data?: FolderReponseModel | undefined; error?: ProblemDetails | undefined }> {
		throw new Error('Method not implemented.');
	}
	createFolder(
		folderRequest: CreateFolderRequestModel
	): Promise<{ data?: string | undefined; error?: ProblemDetails | undefined }> {
		throw new Error('Method not implemented.');
	}
	requestFolder(
		unique: string
	): Promise<{ data?: FolderReponseModel | undefined; error?: ProblemDetails | undefined }> {
		throw new Error('Method not implemented.');
	}
	updateFolder(
		unique: string,
		folder: FolderModelBaseModel
	): Promise<{ data?: FolderModelBaseModel | undefined; error?: ProblemDetails | undefined }> {
		throw new Error('Method not implemented.');
	}
	deleteFolder(id: string): Promise<{ error?: ProblemDetails | undefined }> {
		throw new Error('Method not implemented.');
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

		const { data, error } = await this.#treeDataSource.getChildrenOf({ path });

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

	async createScaffold(parentKey: string | null) {
		return Promise.reject(new Error('Not implemented'));
	}

	async create(patrial: any) {
		return Promise.reject(new Error('Not implemented'));
	}

	async save(patrial: any) {
		return Promise.reject(new Error('Not implemented'));
	}

	async delete(key: string) {
		return Promise.reject(new Error('Not implemented'));
	}
	//#endregion
}
