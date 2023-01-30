import { UmbTemplateDetailStore, UMB_TEMPLATE_DETAIL_STORE_CONTEXT_TOKEN } from './template.detail.store';
import { UmbTemplateDetailServerDataSource } from './sources/template.detail.server.data';
import { ProblemDetails, Template } from '@umbraco-cms/backend-api';
import { UmbContextConsumerController } from '@umbraco-cms/context-api';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';
import { UmbNotificationService, UMB_NOTIFICATION_SERVICE_CONTEXT_TOKEN } from '@umbraco-cms/notification';

/* We need to create a new instance of the repository from within the element context. We want the notifications to be displayed in the right context. */
// element -> context -> repository -> (store) -> data source
export class UmbTemplateDetailRepository {
	#host: UmbControllerHostInterface;
	#dataSource: UmbTemplateDetailServerDataSource;
	#detailStore?: UmbTemplateDetailStore;
	#notificationService?: UmbNotificationService;
	#initResolver?: (value: unknown) => void;
	initialized = false;

	constructor(host: UmbControllerHostInterface) {
		this.#host = host;
		// TODO: figure out how spin up get the correct data source
		this.#dataSource = new UmbTemplateDetailServerDataSource(this.#host);

		new UmbContextConsumerController(this.#host, UMB_TEMPLATE_DETAIL_STORE_CONTEXT_TOKEN, (instance) => {
			this.#detailStore = instance;
			this.#checkIfInitialized();
		});

		new UmbContextConsumerController(this.#host, UMB_NOTIFICATION_SERVICE_CONTEXT_TOKEN, (instance) => {
			this.#notificationService = instance;
			this.#checkIfInitialized();
		});
	}

	init() {
		return new Promise((resolve) => {
			this.initialized ? resolve(true) : (this.#initResolver = resolve);
		});
	}

	#checkIfInitialized() {
		if (this.#detailStore && this.#notificationService) {
			this.initialized = true;
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
}
