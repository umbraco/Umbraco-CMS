import { UmbTemplateDetailRepository } from './data/template.detail.repository';
import { createObservablePart, DeepState } from '@umbraco-cms/observable-api';
import { TemplateModel } from '@umbraco-cms/backend-api';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';
import { UmbContextProviderController } from '@umbraco-cms/context-api';

export class UmbTemplateWorkspaceContext {
	#host: UmbControllerHostInterface;
	#templateDetailRepo: UmbTemplateDetailRepository;

	#data = new DeepState<TemplateModel | undefined>(undefined);
	data = this.#data.asObservable();
	name = createObservablePart(this.#data, (data) => data?.name);
	content = createObservablePart(this.#data, (data) => data?.content);

	isNew = false;

	constructor(host: UmbControllerHostInterface) {
		this.#host = host;
		this.#templateDetailRepo = new UmbTemplateDetailRepository(this.#host);
		new UmbContextProviderController(this.#host, 'umbWorkspaceContext', this);
	}

	getData() {
		return this.#data.getValue();
	}

	setName(value: string) {
		this.#data.next({ ...this.#data.value, $type: this.#data.value?.$type || '', name: value });
	}

	setContent(value: string) {
		this.#data.next({ ...this.#data.value, $type: this.#data.value?.$type || '', content: value });
	}

	async load(entityKey: string) {
		const { data } = await this.#templateDetailRepo.get(entityKey);
		if (data) {
			this.#data.next(data);
		}
	}

	async createScaffold(parentKey: string | null) {
		this.isNew = true;
		const { data } = await this.#templateDetailRepo.createScaffold(parentKey);
		if (!data) return;
		this.#data.next(data);
	}
}
