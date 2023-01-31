import { TemplateTreeServerDataSource } from './sources/template.tree.server.data';
import { UmbTemplateTreeStore, UMB_TEMPLATE_TREE_STORE_CONTEXT_TOKEN } from './template.tree.store';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';
import { UmbNotificationService, UMB_NOTIFICATION_SERVICE_CONTEXT_TOKEN } from '@umbraco-cms/notification';
import { UmbContextConsumerController } from '@umbraco-cms/context-api';
import { ProblemDetails } from '@umbraco-cms/backend-api';

// Move to documentation / JSdoc
/* We need to create a new instance of the repository from within the element context. We want the notifications to be displayed in the right context. */
// element -> context -> repository -> (store) -> data source
// All methods should be async and return a promise. Some methods might return an observable as part of the promise response.
export class UmbTemplateTreeRepository {
	#host: UmbControllerHostInterface;
	#dataSource: TemplateTreeServerDataSource;
	#treeStore!: UmbTemplateTreeStore;
	#notificationService?: UmbNotificationService;
	#initResolver?: () => void;
	#initialized = false;

	constructor(host: UmbControllerHostInterface) {
		this.#host = host;
		// TODO: figure out how spin up get the correct data source
		this.#dataSource = new TemplateTreeServerDataSource(this.#host);

		new UmbContextConsumerController(this.#host, UMB_TEMPLATE_TREE_STORE_CONTEXT_TOKEN, (instance) => {
			this.#treeStore = instance;
			this.#checkIfInitialized();
		});

		new UmbContextConsumerController(this.#host, UMB_NOTIFICATION_SERVICE_CONTEXT_TOKEN, (instance) => {
			this.#notificationService = instance;
			this.#checkIfInitialized();
		});
	}

	#init() {
		return new Promise<void>((resolve) => {
			this.#initialized ? resolve() : (this.#initResolver = resolve);
		});
	}

	#checkIfInitialized() {
		if (this.#treeStore && this.#notificationService) {
			this.#initialized = true;
			this.#initResolver?.();
		}
	}

	async getRoot() {
		await this.#init();
		let updates = undefined;

		const { data, error } = await this.#dataSource.getRoot();

		if (data) {
			this.#treeStore?.appendItems(data.items);
			updates = this.#treeStore?.rootChanged();
		}

		return { data, updates, error };
	}

	async getItemChildren(parentKey: string) {
		await this.#init();
		let updates = undefined;

		if (!parentKey) {
			const error: ProblemDetails = { title: 'Parent key is missing' };
			return { data: undefined, updates, error };
		}

		const { data, error } = await this.#dataSource.getItemChildren(parentKey);

		if (data) {
			this.#treeStore?.appendItems(data.items);
			updates = this.#treeStore?.childrenChanged(parentKey);
		}

		return { data, error };
	}

	async getItems(keys: Array<string>) {
		await this.#init();
		let updates = undefined;

		if (!keys) {
			const error: ProblemDetails = { title: 'Keys are missing' };
			return { data: undefined, updates, error };
		}

		const { data, error } = await this.#dataSource.getItems(keys);

		if (data) {
			updates = this.#treeStore?.itemsChanged(keys);
		}

		return { data, updates, error };
	}
}
