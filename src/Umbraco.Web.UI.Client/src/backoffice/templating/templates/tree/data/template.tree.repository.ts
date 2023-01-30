import { Observable } from 'rxjs';
import { TemplateTreeServerDataSource } from './sources/template.tree.server.data';
import { UmbTemplateTreeStore, UMB_TEMPLATE_TREE_STORE_CONTEXT_TOKEN } from './template.tree.store';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';
import { UmbNotificationService, UMB_NOTIFICATION_SERVICE_CONTEXT_TOKEN } from '@umbraco-cms/notification';
import { UmbContextConsumerController } from '@umbraco-cms/context-api';
import { EntityTreeItem, PagedEntityTreeItem, ProblemDetails } from '@umbraco-cms/backend-api';

export class UmbTemplateTreeRepository {
	#host: UmbControllerHostInterface;
	#dataSource: TemplateTreeServerDataSource;
	#treeStore!: UmbTemplateTreeStore;
	#notificationService?: UmbNotificationService;
	#initResolver?: (value: unknown) => void;
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

	init() {
		return new Promise((resolve) => {
			this.#initialized ? resolve(true) : (this.#initResolver = resolve);
		});
	}

	#checkIfInitialized() {
		if (this.#treeStore && this.#notificationService) {
			this.#initialized = true;
			this.#initResolver?.(true);
		}
	}

	async getTreeRoot(): Promise<{ data?: PagedEntityTreeItem; error?: ProblemDetails }> {
		const { data, error } = await this.#dataSource.getTreeRoot();

		if (data) {
			this.#treeStore?.appendTreeItems(data.items);
		}

		return { data, error };
	}

	async getTreeItemChildren(parentKey: string): Promise<{ data?: PagedEntityTreeItem; error?: ProblemDetails }> {
		if (!parentKey) {
			const error: ProblemDetails = { title: 'Parent key is missing' };
			return { error };
		}

		const { data, error } = await this.#dataSource.getTreeItemChildren(parentKey);

		if (data) {
			this.#treeStore?.appendTreeItems(data.items);
		}

		return { data, error };
	}

	async getTreeItems(keys: Array<string>): Promise<{ data?: EntityTreeItem[]; error?: ProblemDetails }> {
		if (!keys) {
			const error: ProblemDetails = { title: 'Keys are missing' };
			return { error };
		}

		const { data, error } = await this.#dataSource.getTreeItems(keys);

		if (data) {
			this.#treeStore?.appendTreeItems(data);
		}

		return { data, error };
	}

	treeRootChanged(): Observable<EntityTreeItem[]> {
		return this.#treeStore.treeRootChanged?.();
	}

	treeItemChildrenChanged(key: string): Observable<EntityTreeItem[]> {
		return this.#treeStore.treeItemChildrenChanged?.(key);
	}

	treeItemsChanged(keys: Array<string>): Observable<EntityTreeItem[]> {
		return this.#treeStore.treeItemsChanged?.(keys);
	}
}
