import { UmbTemplateRepository } from '../repository/template.repository';
import { UmbWorkspaceContext } from '@umbraco-cms/backoffice/workspace';
import { createObservablePart, UmbDeepState, UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import { TemplateItemResponseModel, TemplateResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';

export class UmbTemplateWorkspaceContext extends UmbWorkspaceContext<UmbTemplateRepository, TemplateResponseModel> {
	#data = new UmbDeepState<TemplateResponseModel | undefined>(undefined);
	data = this.#data.asObservable();
	#masterTemplate = new UmbObjectState<TemplateItemResponseModel | null>(null);
	masterTemplate = this.#masterTemplate.asObservable();
	name = createObservablePart(this.#data, (data) => data?.name);
	content = createObservablePart(this.#data, (data) => data?.content);
	id = createObservablePart(this.#data, (data) => data?.id);

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
			this.#getMasterTemplateFromContent(data.content ?? '');
		}
	}

	#getMasterTemplateFromContent(content: string) {
		if (!content) this.#masterTemplate.next(null);
		const RegexString = /(@{[\s\S][^if]*?Layout\s*?=\s*?)("[^"]*?"|null)(;[\s\S]*?})/gi;
		const match = RegexString.exec(content ?? '');

		if (match) {
			if (match[2] === 'null') return null;
			this.#masterTemplate.next({ id: match[2].replace(/"/g, '') });
			return match[2].replace(/"/g, '');
		}
		this.#masterTemplate.next(null);
		return null;
	}

	async setMasterTemplate(id: string | null) {
		if (id === null) {
			this.#masterTemplate.next(null);
			return null;
		}

		const { data } = await this.repository.requestItem([id]);
		if (data) {
			this.#masterTemplate.next(data[0]);
			return data[0];
		}
		return null;
	}

	public async save() {
		throw new Error('Save method not implemented.');
	}

	async createScaffold() {
		const { data } = await this.repository.createScaffold();
		if (!data) return;
		this.setIsNew(true);
		this.#data.next(data as any);
	}

	public destroy() {
		this.#data.complete();
	}
}
