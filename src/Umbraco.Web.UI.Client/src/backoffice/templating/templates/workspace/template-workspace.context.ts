import { UmbTemplateDetailStore, UMB_TEMPLATE_DETAIL_STORE_CONTEXT_TOKEN } from '../template.detail.store';
import { UmbLitElement } from '@umbraco-cms/element';
import { createObservablePart, DeepState, UmbObserverController } from '@umbraco-cms/observable-api';
import { Template } from '@umbraco-cms/backend-api';

export class UmbTemplateWorkspaceContext {
	#host: UmbLitElement;
	#store?: UmbTemplateDetailStore;
	#storeObserver?: UmbObserverController;
	#initResolver?: (value: unknown) => void;
	ready = false;

	#data = new DeepState<Template | undefined>(undefined);
	data = this.#data.asObservable();
	name = createObservablePart(this.#data, (data) => data?.name);
	content = createObservablePart(this.#data, (data) => data?.content);

	constructor(host: UmbLitElement) {
		this.#host = host;

		this.#host.consumeContext(UMB_TEMPLATE_DETAIL_STORE_CONTEXT_TOKEN, (instance) => {
			this.#store = instance;
			this.ready = true;
			this.#initResolver?.(true);
		});
	}

	init() {
		return new Promise((resolve) => {
			this.ready ? resolve(true) : (this.#initResolver = resolve);
		});
	}

	load(entityKey: string) {
		if (!this.#store) return;

		this.#storeObserver?.destroy();
		this.#storeObserver = new UmbObserverController(this.#host, this.#store.getByKey(entityKey), (data) => {
			if (!data) return;
			this.#data.next(data);
			console.log(data);
		});
	}

	// TODO: create method
	// TODO: save method
	// TODO: delete method
}
