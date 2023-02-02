import { DocumentTreeServerDataSource } from '../tree/data/sources/document.tree.server.data';
import { UmbDocumentTreeStore, UMB_DOCUMENT_TREE_STORE_CONTEXT_TOKEN } from '../tree/data/document.tree.store';
import type { DocumentTreeDataSource } from '../tree/data/sources/document.tree.data.source.interface';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';
import { UmbContextConsumerController } from '@umbraco-cms/context-api';
import { ProblemDetails } from '@umbraco-cms/backend-api';
import type { UmbTreeRepository } from 'libs/repository/tree-repository.interface';

// Move to documentation / JSdoc
/* We need to create a new instance of the repository from within the element context. We want the notifications to be displayed in the right context. */
// element -> context -> repository -> (store) -> data source
// All methods should be async and return a promise. Some methods might return an observable as part of the promise response.
export class UmbDocumentRepository implements UmbTreeRepository {

	#init!: Promise<unknown>;

	#host: UmbControllerHostInterface;
	#source: DocumentTreeDataSource;

	#store?: UmbDocumentTreeStore;


	constructor(host: UmbControllerHostInterface) {
		this.#host = host;

		// TODO: figure out how spin up get the correct data source
		this.#source = new DocumentTreeServerDataSource(this.#host);

		this.#init = Promise.all([
			new UmbContextConsumerController(this.#host, UMB_DOCUMENT_TREE_STORE_CONTEXT_TOKEN, (instance) => {
				this.#store = instance;
			}).asPromise()
		])
	}



	// TODO: Trash
	// TODO: Move


	async requestRootTreeItems() {
		await this.#init;

		const { data, error } = await this.#source.getRootItems();

		if (data) {
			this.#store?.appendItems(data.items);
		}

		return { data, error };
	}

	async requestTreeItemsOf(parentKey: string | null) {
		await this.#init;

		if (!parentKey) {
			const error: ProblemDetails = { title: 'Parent key is missing' };
			return { data: undefined, error };
		}

		const { data, error } = await this.#source.getChildrenOf(parentKey);

		if (data) {
			this.#store?.appendItems(data.items);
		}

		return { data, error };
	}

	async requestTreeItems(keys: Array<string>) {
		await this.#init;

		if (!keys) {
			const error: ProblemDetails = { title: 'Keys are missing' };
			return { data: undefined, error };
		}

		const { data, error } = await this.#source.getItems(keys);

		return { data, error };
	}

	async rootTreeItems() {
		await this.#init;
		return this.#store!.rootItems;
	}

	async treeItemsOf(parentKey: string | null) {
		await this.#init;
		return this.#store!.childrenOf(parentKey);
	}

	async treeItems(keys: Array<string>) {
		await this.#init;
		return this.#store!.items(keys);
	}
}
