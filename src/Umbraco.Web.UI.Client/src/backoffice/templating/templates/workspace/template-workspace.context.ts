import { UmbTemplateRepository } from '../repository/template.repository';
import { UmbWorkspaceContext } from '../../../shared/components/workspace/workspace-context/workspace-context';
import { createObservablePart, DeepState } from '@umbraco-cms/backoffice/observable-api';
import { TemplateResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';

export class UmbTemplateWorkspaceContext extends UmbWorkspaceContext<UmbTemplateRepository, TemplateResponseModel> {
	#data = new DeepState<TemplateResponseModel | undefined>(undefined);
	data = this.#data.asObservable();
	name = createObservablePart(this.#data, (data) => data?.name);
	content = createObservablePart(this.#data, (data) => data?.content);

	constructor(host: UmbControllerHostElement) {
		super(host, new UmbTemplateRepository(host));
	}

	getEntityType(): string {
		return 'template';
	}

	getEntityKey() {
		return this.getData()?.key || '';
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

	public async save() {
		throw new Error('Save method not implemented.');
	}

	async createScaffold(parentKey: string | null) {
		const { data } = await this.repository.createScaffold(parentKey);
		if (!data) return;
		this.setIsNew(true);
		this.#data.next(data);
	}

	public destroy() {
		this.#data.complete();
	}
}
