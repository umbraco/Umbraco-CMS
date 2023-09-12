import { UMB_DOCUMENT_TREE_STORE_CONTEXT_TOKEN, UmbDocumentTreeStore } from '../../repository/document.tree.store.js';
import { UmbDocumentRecycleBinTreeServerDataSource } from './sources/document-recycle-bin.tree.server.data.js';
import type { UmbTreeDataSource, UmbTreeRepository } from '@umbraco-cms/backoffice/repository';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextConsumerController } from '@umbraco-cms/backoffice/context-api';
import { DocumentTreeItemResponseModel } from '@umbraco-cms/backoffice/backend-api';

export class UmbDocumentRecycleBinRepository implements UmbTreeRepository<DocumentTreeItemResponseModel> {
	#init!: Promise<unknown>;

	#host: UmbControllerHostElement;

	#treeSource: UmbTreeDataSource;
	#treeStore?: UmbDocumentTreeStore;

	constructor(host: UmbControllerHostElement) {
		this.#host = host;

		// TODO: figure out how spin up get the correct data source
		this.#treeSource = new UmbDocumentRecycleBinTreeServerDataSource(this.#host);

		this.#init = Promise.all([
			new UmbContextConsumerController(this.#host, UMB_DOCUMENT_TREE_STORE_CONTEXT_TOKEN, (instance) => {
				this.#treeStore = instance;
			}),
		]);
	}

	async requestTreeRoot() {
		await this.#init;

		const data = {
			id: null,
			type: 'document-recycle-bin-root',
			name: 'Recycle Bin',
			icon: 'umb:trash',
			hasChildren: true,
		};

		return { data };
	}

	async requestRootTreeItems() {
		await this.#init;

		const { data, error } = await this.#treeSource.getRootItems();

		if (data) {
			this.#treeStore?.appendItems(data.items);
		}

		return { data, error, asObservable: () => this.#treeStore!.rootItems };
	}

	async requestTreeItemsOf(parentId: string | null) {
		await this.#init;
		if (parentId === undefined) throw new Error('Parent id is missing');

		const { data, error } = await this.#treeSource.getChildrenOf(parentId);

		if (data) {
			this.#treeStore?.appendItems(data.items);
		}

		return { data, error, asObservable: () => this.#treeStore!.childrenOf(parentId) };
	}

	async rootTreeItems() {
		await this.#init;
		return this.#treeStore!.rootItems;
	}

	async treeItemsOf(parentId: string | null) {
		await this.#init;
		return this.#treeStore!.childrenOf(parentId);
	}
}
