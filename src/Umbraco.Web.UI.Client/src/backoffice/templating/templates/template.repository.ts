import { Observable } from 'rxjs';
import { UmbTemplateDetailStore, UMB_TEMPLATE_DETAIL_STORE_CONTEXT_TOKEN } from './template.detail.store';
import { UmbTemplateTreeStore, UMB_TEMPLATE_TREE_STORE_CONTEXT_TOKEN } from './tree/template.tree.store';
import { TemplateServerDataSource } from './data/template.server';
import { EntityTreeItem, PagedEntityTreeItem, ProblemDetails, Template } from '@umbraco-cms/backend-api';
import { UmbContextConsumerController } from '@umbraco-cms/context-api';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';
import { UmbNotificationService, UMB_NOTIFICATION_SERVICE_CONTEXT_TOKEN } from '@umbraco-cms/notification';

/* We need to create a new instance of the repository from within the element context. We want the notifications to be displayed in the right context. */
// element -> context -> repository -> (store) -> data source
export class UmbTemplateRepository {
	#host: UmbControllerHostInterface;
	#dataSource: TemplateServerDataSource;
	#detailStore?: UmbTemplateDetailStore;
	#treeStore!: UmbTemplateTreeStore;
	#notificationService?: UmbNotificationService;
	#initResolver?: (value: unknown) => void;
	#ready = false;

	constructor(host: UmbControllerHostInterface) {
		this.#host = host;
		// TODO: figure out how spin up get the correct data source
		this.#dataSource = new TemplateServerDataSource(this.#host);

		new UmbContextConsumerController(this.#host, UMB_TEMPLATE_DETAIL_STORE_CONTEXT_TOKEN, (instance) => {
			this.#detailStore = instance;
			this.#checkIfReady();
		});

		new UmbContextConsumerController(this.#host, UMB_TEMPLATE_TREE_STORE_CONTEXT_TOKEN, (instance) => {
			this.#treeStore = instance;
			this.#checkIfReady();
		});

		new UmbContextConsumerController(this.#host, UMB_NOTIFICATION_SERVICE_CONTEXT_TOKEN, (instance) => {
			this.#notificationService = instance;
			this.#checkIfReady();
		});
	}

	init() {
		return new Promise((resolve) => {
			this.#ready ? resolve(true) : (this.#initResolver = resolve);
		});
	}

	#checkIfReady() {
		if (this.#detailStore && this.#treeStore && this.#notificationService) {
			this.#ready = true;
			this.#initResolver?.(true);
		}
	}

	async createScaffold(parentKey: string | null): Promise<{ data?: Template; error?: ProblemDetails }> {
		if (!parentKey) {
			const error: ProblemDetails = { title: 'Parent key is missing' };
			return { error };
		}

		return this.#dataSource.createScaffold(parentKey);
	}

	async get(key: string): Promise<{ data?: Template; error?: ProblemDetails }> {
		if (!key) {
			const error: ProblemDetails = { title: 'Key is missing' };
			return { error };
		}

		return this.#dataSource.get(key);
	}

	async insert(template: Template): Promise<{ error?: ProblemDetails }> {
		if (!template) {
			const error: ProblemDetails = { title: 'Template is missing' };
			return { error };
		}

		const { error } = await this.#dataSource.insert(template);

		if (!error) {
			const notification = { data: { message: `Template created` } };
			this.#notificationService?.peek('positive', notification);
		}

		// TODO: we currently don't use the detail store for anything.
		// Consider to look up the data before fetching from the server
		this.#detailStore?.append(template);

		return { error };
	}

	async update(template: Template): Promise<{ error?: ProblemDetails }> {
		if (!template) {
			const error: ProblemDetails = { title: 'Template is missing' };
			return { error };
		}

		const { error } = await this.#dataSource.update(template);

		if (!error) {
			const notification = { data: { message: `Template saved` } };
			this.#notificationService?.peek('positive', notification);
		}

		// TODO: we currently don't use the detail store for anything.
		// Consider to look up the data before fetching from the server
		this.#detailStore?.append(template);

		return { error };
	}

	async delete(key: string): Promise<{ error?: ProblemDetails }> {
		if (!key) {
			const error: ProblemDetails = { title: 'Key is missing' };
			return { error };
		}

		const { error } = await this.#dataSource.delete(key);

		if (!error) {
			const notification = { data: { message: `Template deleted` } };
			this.#notificationService?.peek('positive', notification);
		}

		// TODO: remove from detail store
		// TODO: remove from tree store
		return { error };
	}

	// TODO: split into multiple repositories
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
