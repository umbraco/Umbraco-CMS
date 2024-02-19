import type { UmbTemplateDetailModel } from '../types.js';
import type { UmbTemplateItemModel } from '../repository/index.js';
import { UmbTemplateDetailRepository, UmbTemplateItemRepository } from '../repository/index.js';
import { UMB_TEMPLATE_WORKSPACE_ALIAS } from './manifests.js';
import { loadCodeEditor } from '@umbraco-cms/backoffice/code-editor';
import type { UmbSaveableWorkspaceContextInterface } from '@umbraco-cms/backoffice/workspace';
import { UmbEditableWorkspaceContextBase } from '@umbraco-cms/backoffice/workspace';
import { UmbBooleanState, UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export class UmbTemplateWorkspaceContext
	extends UmbEditableWorkspaceContextBase<UmbTemplateDetailModel>
	implements UmbSaveableWorkspaceContextInterface
{
	public readonly detailRepository = new UmbTemplateDetailRepository(this);
	public readonly itemRepository = new UmbTemplateItemRepository(this);

	#parentUnique: string | null = null;

	#data = new UmbObjectState<UmbTemplateDetailModel | undefined>(undefined);
	data = this.#data.asObservable();
	#masterTemplate = new UmbObjectState<UmbTemplateItemModel | null>(null);
	masterTemplate = this.#masterTemplate.asObservable();
	name = this.#data.asObservablePart((data) => data?.name);
	alias = this.#data.asObservablePart((data) => data?.alias);
	content = this.#data.asObservablePart((data) => data?.content);
	unique = this.#data.asObservablePart((data) => data?.unique);
	masterTemplateUnique = this.#data.asObservablePart((data) => data?.masterTemplate?.unique);

	#isCodeEditorReady = new UmbBooleanState(false);
	isCodeEditorReady = this.#isCodeEditorReady.asObservable();

	constructor(host: UmbControllerHost) {
		super(host, UMB_TEMPLATE_WORKSPACE_ALIAS);
		this.#loadCodeEditor();
	}

	async #loadCodeEditor() {
		try {
			await loadCodeEditor();
			this.#isCodeEditorReady.setValue(true);
		} catch (error) {
			console.error(error);
		}
	}

	getEntityType(): string {
		return 'template';
	}

	getEntityId() {
		return this.getData()?.unique;
	}

	getData() {
		return this.#data.getValue();
	}

	setName(value: string) {
		this.#data.update({ name: value });
	}

	setAlias(value: string) {
		this.#data.update({ alias: value });
	}

	setContent(value: string) {
		this.#data.update({ content: value });
	}

	getLayoutBlockRegexPattern() {
		return new RegExp('(@{[\\s\\S][^if]*?Layout\\s*?=\\s*?)("[^"]*?"|null)(;[\\s\\S]*?})', 'gi');
	}

	getHasLayoutBlock() {
		return this.getData()?.content ? this.getLayoutBlockRegexPattern().test(this.getData()?.content as string) : false;
	}

	async load(unique: string) {
		const { data } = await this.detailRepository.requestByUnique(unique);
		if (data) {
			this.setIsNew(false);
			this.setMasterTemplate(data.masterTemplate?.unique ?? null);
			this.#data.setValue(data);
		}
	}

	async setMasterTemplate(id: string | null) {
		if (id === null) {
			this.#masterTemplate.setValue(null);
			this.#updateMasterTemplateLayoutBlock();
			return null;
		}

		const { data } = await this.itemRepository.requestItems([id]);
		if (data) {
			this.#masterTemplate.setValue(data[0]);
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
				`$1"${newMasterTemplateAlias}.cshtml"$3`,
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

	async create(parentUnique: string | null) {
		this.#parentUnique = parentUnique;
		const { data } = await this.detailRepository.createScaffold();
		if (!data) return;
		this.setIsNew(true);
		this.#data.setValue(data);

		if (!parentUnique) return;
		await this.setMasterTemplate(parentUnique);
	}

	async save() {
		if (!this.#data.value) throw new Error('Data is missing');

		let newData = undefined;

		if (this.getIsNew()) {
			const { data } = await this.detailRepository.create(this.#data.value, this.#parentUnique);
			newData = data;
		} else {
			const { data } = await this.detailRepository.save(this.#data.value);
			newData = data;
		}

		if (newData) {
			this.#data.setValue(newData);
			this.saveComplete(newData);
		}
	}

	public destroy() {
		this.#data.destroy();
		super.destroy();
	}
}

export const UMB_TEMPLATE_WORKSPACE_CONTEXT = new UmbContextToken<
	UmbSaveableWorkspaceContextInterface,
	UmbTemplateWorkspaceContext
>(
	'UmbWorkspaceContext',
	undefined,
	(context): context is UmbTemplateWorkspaceContext => context.getEntityType?.() === 'template',
);
