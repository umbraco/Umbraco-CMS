import { UmbTemplateDetailRepository } from './data/template.detail.repository';
import { createObservablePart, DeepState } from '@umbraco-cms/observable-api';
import type { TemplateModel } from '@umbraco-cms/backend-api';
import type { UmbControllerHostInterface } from '@umbraco-cms/controller';

export class UmbTemplateWorkspaceContext {
	#host: UmbControllerHostInterface;
	#templateDetailRepo: UmbTemplateDetailRepository;

	#data = new DeepState<TemplateModel | undefined>(undefined);
	data = this.#data.asObservable();
	name = createObservablePart(this.#data, (data) => data?.name);
	content = createObservablePart(this.#data, (data) => data?.content);

	constructor(host: UmbControllerHostInterface) {
		this.#host = host;
		this.#templateDetailRepo = new UmbTemplateDetailRepository(this.#host);
	}

	setName(value: string) {
		this.#data.next({ ...this.#data.value, name: value });
	}

	setContent(value: string) {
		this.#data.next({ ...this.#data.value, content: value });
	}

	async load(entityKey: string) {
		const { data } = await this.#templateDetailRepo.get(entityKey);
		if (data) {
			this.#data.next(data);
		}
	}

	async createScaffold(parentKey: string | null) {
		const { data } = await this.#templateDetailRepo.createScaffold(parentKey);
		if (!data) return;
		this.#data.next(data);
	}

	async save(isNew: boolean) {
		if (!this.#data.value) return;
		isNew ? this.#templateDetailRepo.insert(this.#data.value) : this.#templateDetailRepo.update(this.#data.value);
	}

	async delete(key: string) {
		await this.#templateDetailRepo.delete(key);
	}
}
