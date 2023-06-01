import { loadCodeEditor } from '@umbraco-cms/backoffice/code-editor';
import { UmbTemplateRepository } from '../repository/template.repository.js';
import { UmbWorkspaceContext } from '@umbraco-cms/backoffice/workspace';
import {
	createObservablePart,
	UmbBooleanState,
	UmbDeepState,
	UmbObjectState,
} from '@umbraco-cms/backoffice/observable-api';
import type { TemplateItemResponseModel, TemplateResponseModel } from '@umbraco-cms/backoffice/backend-api';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';

export class UmbTemplateWorkspaceContext extends UmbWorkspaceContext<UmbTemplateRepository, TemplateResponseModel> {
	#data = new UmbDeepState<TemplateResponseModel | undefined>(undefined);
	data = this.#data.asObservable();
	#masterTemplate = new UmbObjectState<TemplateItemResponseModel | null>(null);
	masterTemplate = this.#masterTemplate.asObservable();
	name = createObservablePart(this.#data, (data) => data?.name);
	alias = createObservablePart(this.#data, (data) => data?.alias);
	content = createObservablePart(this.#data, (data) => data?.content);
	id = createObservablePart(this.#data, (data) => data?.id);

	#isCodeEditorReady = new UmbBooleanState(false);
	isCodeEditorReady = this.#isCodeEditorReady.asObservable();

	constructor(host: UmbControllerHostElement) {
		super(host, new UmbTemplateRepository(host));
		this.#loadCodeEditor();
	}

	async #loadCodeEditor() {
		try {
			await loadCodeEditor();
			this.#isCodeEditorReady.next(true);
		} catch (error) {
			console.error(error);
		}
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

	setAlias(value: string) {
		this.#data.next({ ...this.#data.value, $type: this.#data.value?.$type || '', alias: value });
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
			if (match[2] === 'null') {
				this.#masterTemplate.next(null);
				return null;
			}

			this.#masterTemplate.next({ name: match[2].replace(/"/g, '').replace('.cshtml', '') });
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

		const { data } = await this.repository.requestItems([id]);
		if (data) {
			this.#masterTemplate.next(data[0]);
			return data[0];
		}
		return null;
	}

	public async save() {
		const template = this.#data.getValue();
		const isNew = this.getIsNew();

		if (isNew && template) {
			await this.repository.create({
				name: template.name,
				content: template.content,
				alias: template.alias,
			});
			if (this.#masterTemplate.value?.id) {
				this.repository.requestTreeItemsOf(this.#masterTemplate.value?.id ?? '');
			} else {
				this.repository.requestRootTreeItems();
			}
		}

		if (template?.id) {
			await this.repository.save(template.id, {
				name: template.name,
				content: template.content,
				alias: template.alias,
			});
		}
	}

	async createScaffold(parentId: string | null = null) {
		const { data } = await this.repository.createScaffold();
		if (!data) return;
		this.setIsNew(true);
		this.#data.next({ ...data, id: '', name: '', alias: '', $type: 'TemplateResponseModel' });
		if (!parentId || parentId === 'root') return;
		await this.setMasterTemplate(parentId);
		const RegexString = /(@{[\s\S][^if]*?Layout\s*?=\s*?)("[^"]*?"|null)(;[\s\S]*?})/gi;
		const content = this.#data.value?.content ?? '';
		const masterTemplateName = this.#masterTemplate.value?.name ?? '';
		const string = content.replace(RegexString, `$1"${masterTemplateName}.cshtml"$3`) ?? '';

		this.setContent(string);
	}

	public destroy() {
		this.#data.complete();
	}
}
