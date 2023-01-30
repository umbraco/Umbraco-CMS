import { UmbTemplateDetailRepository } from '../template.detail.repository';
import { createObservablePart, DeepState } from '@umbraco-cms/observable-api';
import { Template } from '@umbraco-cms/backend-api';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';

export class UmbTemplateWorkspaceContext {
	#host: UmbControllerHostInterface;
	#templateRepository: UmbTemplateDetailRepository;

	#data = new DeepState<Template | undefined>(undefined);
	data = this.#data.asObservable();
	name = createObservablePart(this.#data, (data) => data?.name);
	content = createObservablePart(this.#data, (data) => data?.content);

	constructor(host: UmbControllerHostInterface) {
		this.#host = host;
		this.#templateRepository = new UmbTemplateDetailRepository(this.#host);
	}

	setName(value: string) {
		this.#data.next({ ...this.#data.value, name: value });
	}

	setContent(value: string) {
		this.#data.next({ ...this.#data.value, content: value });
	}

	async load(entityKey: string) {
		const { data } = await this.#templateRepository.get(entityKey);
		if (data) {
			this.#data.next(data);
		}
	}

	async createScaffold(parentKey: string | null) {
		const { data } = await this.#templateRepository.createScaffold(parentKey);
		if (!data) return;
		this.#data.next(data);
	}

	async save(isNew: boolean) {
		if (!this.#data.value) return;
		isNew ? this.#templateRepository.insert(this.#data.value) : this.#templateRepository.update(this.#data.value);
	}

	async delete(key: string) {
		await this.#templateRepository.delete(key);
	}
}
