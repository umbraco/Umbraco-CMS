import { v4 as uuid } from 'uuid';
import { Observable } from 'rxjs';
import { UmbTemplateDetailStore, UMB_TEMPLATE_DETAIL_STORE_CONTEXT_TOKEN } from './template.detail.store';
import { UmbTemplateTreeStore, UMB_TEMPLATE_TREE_STORE_CONTEXT_TOKEN } from './tree/template.tree.store';
import { EntityTreeItem, Template, TemplateResource } from '@umbraco-cms/backend-api';
import { UmbContextConsumerController } from '@umbraco-cms/context-api';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';
import { tryExecuteAndNotify } from '@umbraco-cms/resources';
import { UmbNotificationService, UMB_NOTIFICATION_SERVICE_CONTEXT_TOKEN } from '@umbraco-cms/notification';

/* we need to new up the repository from within the element context. We want the correct context for
the notifications to be displayed in the correct place. */

// element -> context -> repository -> (store) -> data source

/* TODO: don't call the server directly from the repository. 
We need to be able to swap it out with a local database in the future. 
Implement data sources */
export class UmbTemplateRepository {
	#host: UmbControllerHostInterface;
	#detailStore?: UmbTemplateDetailStore;
	#treeStore!: UmbTemplateTreeStore;
	#notificationService?: UmbNotificationService;
	#initResolver?: (value: unknown) => void;
	#ready = false;

	constructor(host: UmbControllerHostInterface) {
		this.#host = host;

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

	async new(parentKey: string | null): Promise<{ data?: Template; error?: unknown }> {
		let masterTemplateAlias: string | undefined = undefined;
		let error = undefined;
		let data = undefined;

		// TODO: can we do something so we don't have to call two endpoints?
		if (parentKey) {
			const { data: parentData, error: parentError } = await tryExecuteAndNotify(
				this.#host,
				TemplateResource.getTemplateByKey({ key: parentKey })
			);
			masterTemplateAlias = parentData?.alias;
			error = parentError;
		}

		const { data: scaffoldData, error: scaffoldError } = await tryExecuteAndNotify(
			this.#host,
			TemplateResource.getTemplateScaffold({ masterTemplateAlias })
		);
		error = scaffoldError;

		if (scaffoldData?.content) {
			data = {
				key: uuid(),
				name: '',
				alias: '',
				content: scaffoldData?.content,
			};
		}

		return { data, error };
	}

	async get(key: string): Promise<{ data?: Template; error?: unknown }> {
		if (!key) {
			return { error: 'key is missing' };
		}

		const { data, error } = await tryExecuteAndNotify(this.#host, TemplateResource.getTemplateByKey({ key }));
		return { data, error };
	}

	async insert(template: Template): Promise<{ error?: unknown }> {
		if (!template) {
			return { error: 'Template is missing' };
		}

		const payload = { requestBody: template };
		const { error } = await tryExecuteAndNotify(this.#host, TemplateResource.postTemplate(payload));

		if (!error) {
			const notification = { data: { message: `Template created` } };
			this.#notificationService?.peek('positive', notification);
		}

		// TODO: we currently don't use the detail store for anything.
		// Consider to look up the data before fetching from the server
		this.#detailStore?.append(template);

		return { error };
	}

	async update(template: Template): Promise<{ error?: unknown }> {
		if (!template.key) {
			return { error: 'Template key is missing' };
		}

		const payload = { key: template.key, requestBody: template };
		const { error } = await tryExecuteAndNotify(this.#host, TemplateResource.putTemplateByKey(payload));

		if (!error) {
			const notification = { data: { message: `Template saved` } };
			this.#notificationService?.peek('positive', notification);
		}

		// TODO: we currently don't use the detail store for anything.
		// Consider to look up the data before fetching from the server
		this.#detailStore?.append(template);

		return { error };
	}

	async delete(key: string): Promise<{ error?: unknown }> {
		if (key) {
			return { error: 'Key is missing' };
		}

		const { error } = await tryExecuteAndNotify(this.#host, TemplateResource.deleteTemplateByKey({ key }));

		if (!error) {
			const notification = { data: { message: `Template saved` } };
			this.#notificationService?.peek('positive', notification);
		}

		// TODO: remove from detail store
		// TODO: remove from tree store
		return { error };
	}

	// TODO: add delete
	// TODO: split into multiple repositories

	async getTreeRoot(): Promise<{ data?: unknown; error?: unknown }> {
		const { data, error } = await tryExecuteAndNotify(this.#host, TemplateResource.getTreeTemplateRoot({}));

		if (data) {
			this.#treeStore?.appendTreeItems(data.items);
		}

		return { data, error };
	}

	async getTreeItemChildren(key: string): Promise<{ data?: unknown; error?: unknown }> {
		if (key) {
			return { error: 'Key is missing' };
		}

		const { data, error } = await tryExecuteAndNotify(
			this.#host,
			TemplateResource.getTreeTemplateChildren({
				parentKey: key,
			})
		);

		if (data) {
			this.#treeStore?.appendTreeItems(data.items);
		}

		return { data, error };
	}

	async getTreeItems(keys: Array<string>): Promise<{ data?: unknown; error?: unknown }> {
		if (keys) {
			return { error: 'Keys are missing' };
		}

		const { data, error } = await tryExecuteAndNotify(
			this.#host,
			TemplateResource.getTreeTemplateItem({
				key: keys,
			})
		);

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
