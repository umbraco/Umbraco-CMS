import { UmbTemplateRepository } from '../repository/template.repository.js';
import { UmbWorkspaceContext } from '@umbraco-cms/backoffice/workspace';
import { createObservablePart, UmbDeepState } from '@umbraco-cms/backoffice/observable-api';
import { TemplateResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';

export class UmbTemplateWorkspaceContext extends UmbWorkspaceContext<UmbTemplateRepository, TemplateResponseModel> {
	#data = new UmbDeepState<TemplateResponseModel | undefined>(undefined);
	data = this.#data.asObservable();
	name = createObservablePart(this.#data, (data) => data?.name);
	content = createObservablePart(this.#data, (data) => data?.content);

	constructor(host: UmbControllerHostElement) {
		super(host, new UmbTemplateRepository(host));
	}

	getEntityType(): string {
		return 'template';
	}

	getEntityId() {
		return this.getData()?.id || '';
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

	async load(entityId: string) {
		const { data } = await this.repository.requestById(entityId);
		if (data) {
			this.setIsNew(false);
			this.#data.next(data);
		}
	}

	public async save() {
		throw new Error('Save method not implemented.');
	}

	async createScaffold(parentId: string | null) {
		const { data } = await this.repository.createScaffold(parentId);
		if (!data) return;
		this.setIsNew(true);
		this.#data.next(data as any);
	}

	public destroy() {
		this.#data.complete();
	}
}
