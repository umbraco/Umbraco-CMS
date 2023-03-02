import { UmbTemplateRepository } from '../repository/template.repository';
import { UmbWorkspaceContext } from '../../../shared/components/workspace/workspace-context/workspace-context';
import { createObservablePart, DeepState } from '@umbraco-cms/observable-api';
import { TemplateModel } from '@umbraco-cms/backend-api';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';

export class UmbTemplateWorkspaceContext extends UmbWorkspaceContext<UmbTemplateRepository> {
	#data = new DeepState<TemplateModel | undefined>(undefined);
	data = this.#data.asObservable();
	name = createObservablePart(this.#data, (data) => data?.name);
	content = createObservablePart(this.#data, (data) => data?.content);

	constructor(host: UmbControllerHostInterface) {
		super(host, new UmbTemplateRepository(host));
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
		const { data } = await this.repository.requestByKey(entityKey);
		if (data) {
			this.setIsNew(false);
			this.#data.next(data);
		}
	}

	async createScaffold(parentKey: string | null) {
		const { data } = await this.repository.createScaffold(parentKey);
		if (!data) return;
		this.setIsNew(true);
		this.#data.next(data);
	}
}
