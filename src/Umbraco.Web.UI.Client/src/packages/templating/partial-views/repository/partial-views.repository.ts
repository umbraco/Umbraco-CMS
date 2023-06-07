import { UmbPartialViewDetailServerDataSource } from './sources/partial-views.detail.server.data.js';
import { UmbPartialViewsTreeServerDataSource } from './sources/partial-views.tree.server.data.js';
import { UmbPartialViewsTreeStore, UMB_PARTIAL_VIEW_TREE_STORE_CONTEXT_TOKEN } from './partial-views.tree.store.js';
import { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextConsumerController } from '@umbraco-cms/backoffice/context-api';
import { ProblemDetailsModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbDetailRepository, UmbTreeRepository } from '@umbraco-cms/backoffice/repository';
import { UmbTreeRootEntityModel } from '@umbraco-cms/backoffice/tree';

export class UmbTemplateRepository implements UmbTreeRepository<any>, UmbDetailRepository<any> {
	#init;
	#host: UmbControllerHostElement;

	#treeDataSource: UmbPartialViewsTreeServerDataSource;
	#detailDataSource: UmbPartialViewDetailServerDataSource;

	#treeStore?: UmbPartialViewsTreeStore;

	constructor(host: UmbControllerHostElement) {
		this.#host = host;

		this.#treeDataSource = new UmbPartialViewsTreeServerDataSource(this.#host);
		this.#detailDataSource = new UmbPartialViewDetailServerDataSource(this.#host);

		this.#init = Promise.all([
			new UmbContextConsumerController(this.#host, UMB_PARTIAL_VIEW_TREE_STORE_CONTEXT_TOKEN, (instance) => {
				this.#treeStore = instance;
			}),
		]);
	}

	requestTreeRoot(): Promise<{ data?: UmbTreeRootEntityModel | undefined; error?: ProblemDetailsModel | undefined }> {
		//throw new Error('Method not implemented.');
		return {data: undefined, error: undefined} as any;
	}

	requestItemsLegacy?:
		| ((uniques: string[]) => Promise<{
				data?: any[] | undefined;
				error?: ProblemDetailsModel | undefined;
				asObservable?: (() => Observable<any[]>) | undefined;
		  }>)
		| undefined;

	itemsLegacy?: ((uniques: string[]) => Promise<Observable<any[]>>) | undefined;

	byId(id: string): Promise<Observable<any>> {
		throw new Error('Method not implemented.');
	}

	// TODO: This method to be done, or able to go away?
	// eslint-disable-next-line @typescript-eslint/ban-ts-comment
	// @ts-ignore
	requestById(id: string): Promise<{ data?: any; error?: ProblemDetailsModel | undefined }> {
		throw new Error('Method not implemented.');
	}

	// TREE:

	async requestRootTreeItems() {
		await this.#init;

		const { data, error } = await this.#treeDataSource.getRootItems();

		if (data) {
			this.#treeStore?.appendItems(data.items);
		}

		return { data, error, asObservable: () => this.#treeStore!.rootItems };
	}

	async requestTreeItemsOf(path: string | null) {
		if (!path) throw new Error('Cannot request tree item with missing path');

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
			const error: ProblemDetailsModel = { title: 'Keys are missing' };
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

	// DETAILS
	async requestByKey(path: string) {
		if (!path) throw new Error('Path is missing');
		await this.#init;
		const { data, error } = await this.#detailDataSource.get(path);
		return { data, error };
	}

	// DETAILS:

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
}
