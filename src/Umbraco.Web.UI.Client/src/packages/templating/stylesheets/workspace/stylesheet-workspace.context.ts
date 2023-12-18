import { UmbStylesheetDetailRepository } from '../repository/stylesheet-detail.repository.js';
import type { UmbStylesheetDetailModel } from '../types.js';
import { UMB_STYLESHEET_ENTITY_TYPE } from '../entity.js';
import { UMB_STYLESHEET_WORKSPACE_ALIAS } from './manifests.js';
import {
	type UmbSaveableWorkspaceContextInterface,
	UmbEditableWorkspaceContextBase,
} from '@umbraco-cms/backoffice/workspace';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbBooleanState, UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import { loadCodeEditor } from '@umbraco-cms/backoffice/code-editor';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export class UmbStylesheetWorkspaceContext
	extends UmbEditableWorkspaceContextBase<UmbStylesheetDetailRepository, UmbStylesheetDetailModel>
	implements UmbSaveableWorkspaceContextInterface<UmbStylesheetDetailModel | undefined>
{
	#data = new UmbObjectState<UmbStylesheetDetailModel | undefined>(undefined);
	readonly data = this.#data.asObservable();
	readonly name = this.#data.asObservablePart((data) => data?.name);
	readonly content = this.#data.asObservablePart((data) => data?.content);
	readonly path = this.#data.asObservablePart((data) => data?.path);

	#isCodeEditorReady = new UmbBooleanState(false);
	readonly isCodeEditorReady = this.#isCodeEditorReady.asObservable();

	constructor(host: UmbControllerHostElement) {
		super(host, UMB_STYLESHEET_WORKSPACE_ALIAS, new UmbStylesheetDetailRepository(host));
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
		const data = this.getData();
		if (!data) throw new Error('Data is missing');
		return data.entityType;
	}

	getEntityId() {
		const data = this.getData();
		if (!data) throw new Error('Data is missing');
		return data.unique;
	}

	getData() {
		return this.#data.getValue();
	}

	setName(value: string) {
		this.#data.update({ name: value });
	}

	setContent(value: string) {
		this.#data.update({ content: value });
	}

	async load(unique: string) {
		const { data } = await this.repository.requestByUnique(unique);

		if (data) {
			this.setIsNew(false);
			this.#data.next(data);
		}
	}

	async create(parentUnique: string | null) {
		const { data } = await this.repository.createScaffold(parentUnique);

		if (data) {
			this.setIsNew(true);
			this.#data.next(data);
		}
	}

	public async save() {
		if (!this.#data.value) throw new Error('Data is missing');

		let newData = undefined;

		if (this.getIsNew()) {
			const { data } = await this.repository.create(this.#data.value);
			newData = data;
		} else {
			const { data } = await this.repository.save(this.#data.value);
			newData = data;
		}

		if (newData) {
			this.#data.next(newData);
			this.saveComplete(newData);
		}
	}

	/*
	async sendRulesGetContent() {
		const requestBody = {
			content: this.getData()?.content,
			rules: this.getRules(),
		};
		const { data } = await this.repository.interpolateStylesheetRules(requestBody);
		this.setContent(data?.content ?? '');
	}

	async sendContentGetRules() {
		const content = this.getData()?.content;
		if (!content) throw Error('No content');

		const { data } = await this.repository.extractStylesheetRules({ content });
		this.setRules(data?.rules ?? []);
	}

	getRules() {
		return this.#rules.getValue();
	}

	updateRule(unique: string, rule: RichTextRuleModelSortable) {
		this.#rules.updateOne(unique, rule);
		this.sendRulesGetContent();
	}
	*/

	public destroy(): void {
		this.#data.destroy();
	}
}

export const UMB_STYLESHEET_WORKSPACE_CONTEXT = new UmbContextToken<
	UmbSaveableWorkspaceContextInterface,
	UmbStylesheetWorkspaceContext
>(
	'UmbWorkspaceContext',
	undefined,
	(context): context is UmbStylesheetWorkspaceContext => context.getEntityType?.() === UMB_STYLESHEET_ENTITY_TYPE,
);
