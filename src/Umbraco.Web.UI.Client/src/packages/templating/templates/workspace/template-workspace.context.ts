import { UmbTemplateRepository } from '../repository/template.repository.js';
import { loadCodeEditor } from '@umbraco-cms/backoffice/code-editor';
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
	masterTemplateID = createObservablePart(this.#data, (data) => data?.masterTemplateId);

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
		this.#data.next({ ...this.#data.value, name: value });
	}

	setAlias(value: string) {
		this.#data.next({ ...this.#data.value, alias: value });
	}

	setContent(value: string) {
		this.#data.next({ ...this.#data.value, content: value });
	}

	getLayoutBlockRegexPattern() {
		return new RegExp('(@{[\\s\\S][^if]*?Layout\\s*?=\\s*?)("[^"]*?"|null)(;[\\s\\S]*?})', 'gi');
	}

	getHasLayoutBlock() {
		return this.getData()?.content ? this.getLayoutBlockRegexPattern().test(this.getData()?.content as string) : false;
	}

	async load(entityId: string) {
		const { data } = await this.repository.requestById(entityId);
		if (data) {
			this.setIsNew(false);
			this.setMasterTemplate(data.masterTemplateId ?? null);
			this.#data.next(data);
		}
	}

	async setMasterTemplate(id: string | null) {
		if (id === null) {
			this.#masterTemplate.next(null);
			this.#updateMasterTemplateLayoutBlock();
			return null;
		}

		const { data } = await this.repository.requestItems([id]);
		if (data) {
			this.#masterTemplate.next(data[0]);
			this.#updateMasterTemplateLayoutBlock();
			return data[0];
		}
		return null;
	}

	#updateMasterTemplateLayoutBlock = () => {
		const currentContent = this.#data.getValue()?.content;
		const newMasterTemplateAlias = this.#masterTemplate?.getValue()?.alias;
		const hasLayoutBlock = this.getHasLayoutBlock();

		if (this.#masterTemplate.getValue() === null && hasLayoutBlock && currentContent) {
			const newString = currentContent.replace(this.getLayoutBlockRegexPattern(), `$1null$3`);
			this.setContent(newString);
			return;
		}

		//if has layout block in the content
		if (hasLayoutBlock && currentContent) {
			const string = currentContent.replace(
				this.getLayoutBlockRegexPattern(),
				`$1"${newMasterTemplateAlias}.cshtml"$3`
			);
			this.setContent(string);
			return;
		}

		//if no layout block in the content insert it at the beginning
		const string = `@{
	Layout = "${newMasterTemplateAlias}.cshtml";
}
${currentContent}`;
		this.setContent(string);
	};

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
			return;
		}

		if (template?.id) {
			await this.repository.save(template.id, {
				name: template.name,
				content: template.content,
				alias: template.alias,
			});
			this.repository.requestTreeItemsOf(this.#masterTemplate.value?.id ?? null);
		}
	}

	async create(parentId: string | null = null) {
		const { data } = await this.repository.createScaffold(parentId);
		if (!data) return;
		this.setIsNew(true);
		this.#data.next({ ...data, id: '', name: '', alias: '' });
		if (!parentId) return;
		await this.setMasterTemplate(parentId);
	}

	public destroy() {
		this.#data.complete();
	}
}
