import { v4 as uuid } from 'uuid';
import { UmbTemplateDetailStore, UMB_TEMPLATE_DETAIL_STORE_CONTEXT_TOKEN } from '../template.detail.store';
import { createObservablePart, DeepState, UmbObserverController } from '@umbraco-cms/observable-api';
import { Template, TemplateResource } from '@umbraco-cms/backend-api';
import { UmbContextConsumerController } from '@umbraco-cms/context-api';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';
import { tryExecuteAndNotify } from '@umbraco-cms/resources';
import { UmbNotificationService, UMB_NOTIFICATION_SERVICE_CONTEXT_TOKEN } from '@umbraco-cms/notification';

export class UmbTemplateWorkspaceContext {
	#host: UmbControllerHostInterface;
	#store?: UmbTemplateDetailStore;
	#notificationService?: UmbNotificationService;
	#storeObserver?: UmbObserverController;
	#initResolver?: (value: unknown) => void;
	ready = false;

	#data = new DeepState<Template | undefined>(undefined);
	data = this.#data.asObservable();
	name = createObservablePart(this.#data, (data) => data?.name);
	content = createObservablePart(this.#data, (data) => data?.content);

	constructor(host: UmbControllerHostInterface) {
		this.#host = host;

		new UmbContextConsumerController(host, UMB_TEMPLATE_DETAIL_STORE_CONTEXT_TOKEN, (instance) => {
			this.#store = instance;
			this.ready = true;
			this.#initResolver?.(true);
		});

		new UmbContextConsumerController(host, UMB_NOTIFICATION_SERVICE_CONTEXT_TOKEN, (instance) => {
			this.#notificationService = instance;
		});
	}

	setName(value: string) {
		this.#data.next({ ...this.#data.value, name: value });
	}

	setContent(value: string) {
		this.#data.next({ ...this.#data.value, content: value });
	}

	init() {
		return new Promise((resolve) => {
			this.ready ? resolve(true) : (this.#initResolver = resolve);
		});
	}

	load(entityKey: string) {
		if (!this.ready || !this.#store) return;

		this.#storeObserver?.destroy();
		this.#storeObserver = new UmbObserverController(this.#host, this.#store.getByKey(entityKey), (data) => {
			if (!data) return;
			this.#data.next(data);
		});
	}

	async createScaffold(parentKey: string | null) {
		if (!this.ready || !this.#store) return;

		let masterTemplateAlias: string | undefined = undefined;

		// TODO: handle errors
		// TODO: can we do something so we don't have to call two endpoints?

		if (parentKey) {
			const { data: parentData } = await tryExecuteAndNotify(
				this.#host,
				TemplateResource.getTemplateByKey({ key: parentKey })
			);
			masterTemplateAlias = parentData?.alias;
		}

		const { data: scaffoldData } = await tryExecuteAndNotify(
			this.#host,
			TemplateResource.getTemplateScaffold({ masterTemplateAlias })
		);

		const template = {
			key: uuid(),
			name: '',
			alias: '',
			content: scaffoldData?.content,
		};

		this.#data.next(template);
	}

	async create() {
		// TODO: handle error
		if (!this.#data.value) return;

		try {
			await this.#store?.create(this.#data.value);
			this.#notificationService?.peek('positive', { data: { message: `Template created` } });
		} catch (error) {
			console.log(error);
		}
	}

	async update() {
		// TODO: handle error
		if (!this.#data.value) return;

		try {
			await this.#store?.save(this.#data.value);
			this.#notificationService?.peek('positive', { data: { message: `Template saved` } });
		} catch (error) {
			console.log(error);
		}
	}

	// TODO: add delete method
}
