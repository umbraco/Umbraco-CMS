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
	extends UmbEditableWorkspaceContextBase<UmbStylesheetDetailModel>
	implements UmbSaveableWorkspaceContextInterface
{
	public readonly repository = new UmbStylesheetDetailRepository(this);

	#data = new UmbObjectState<UmbStylesheetDetailModel | undefined>(undefined);
	readonly data = this.#data.asObservable();
	readonly name = this.#data.asObservablePart((data) => data?.name);
	readonly content = this.#data.asObservablePart((data) => data?.content);
	readonly path = this.#data.asObservablePart((data) => data?.path);

	#isCodeEditorReady = new UmbBooleanState(false);
	readonly isCodeEditorReady = this.#isCodeEditorReady.asObservable();

	constructor(host: UmbControllerHostElement) {
		super(host, UMB_STYLESHEET_WORKSPACE_ALIAS);
		this.#loadCodeEditor();
	}

	protected resetState(): void {
		super.resetState();
		this.#data.setValue(undefined);
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
		return UMB_STYLESHEET_ENTITY_TYPE;
	}

	getUnique() {
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
		this.resetState();
		const { data } = await this.repository.requestByUnique(unique);

		if (data) {
			this.setIsNew(false);
			this.#data.setValue(data);
		}
	}

	async create(parentUnique: string | null) {
		this.resetState();
		const { data } = await this.repository.createScaffold(parentUnique);

		if (data) {
			this.setIsNew(true);
			this.#data.setValue(data);
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
			this.#data.setValue(newData);
			this.workspaceComplete(newData);
		}
	}

	public destroy(): void {
		this.#data.destroy();
		super.destroy();
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
