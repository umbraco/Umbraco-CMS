import { UmbScriptDetailRepository } from '../repository/index.js';
import type { UmbScriptDetailModel } from '../types.js';
import { UMB_SCRIPT_ENTITY_TYPE } from '../entity.js';
import { UMB_SCRIPT_WORKSPACE_ALIAS } from './manifests.js';
import { UmbBooleanState, UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbEditableWorkspaceContextBase } from '@umbraco-cms/backoffice/workspace';
import { loadCodeEditor } from '@umbraco-cms/backoffice/code-editor';

export class UmbScriptWorkspaceContext extends UmbEditableWorkspaceContextBase<UmbScriptDetailModel> {
	public readonly repository = new UmbScriptDetailRepository(this);

	#data = new UmbObjectState<UmbScriptDetailModel | undefined>(undefined);
	readonly data = this.#data.asObservable();
	readonly name = this.#data.asObservablePart((data) => data?.name);
	readonly content = this.#data.asObservablePart((data) => data?.content);
	readonly path = this.#data.asObservablePart((data) => data?.path);

	#isCodeEditorReady = new UmbBooleanState(false);
	readonly isCodeEditorReady = this.#isCodeEditorReady.asObservable();

	constructor(host: UmbControllerHost) {
		super(host, UMB_SCRIPT_WORKSPACE_ALIAS);
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
		return UMB_SCRIPT_ENTITY_TYPE;
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
			this.#data.setValue(data);
		}
	}

	async create(parentUnique: string | null) {
		const { data } = await this.repository.createScaffold();

		if (data) {
			this.setIsNew(true);
			this.#data.setValue(data);
		}
	}

	async save() {
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
			this.saveComplete(newData);
		}
	}

	destroy(): void {
		throw new Error('Method not implemented.');
	}
}
