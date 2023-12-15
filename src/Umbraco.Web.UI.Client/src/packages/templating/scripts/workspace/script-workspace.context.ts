import { UmbScriptDetailRepository } from '../repository/index.js';
import { UmbScriptDetailModel } from '../types.js';
import { UMB_SCRIPT_WORKSPACE_ALIAS } from './manifests.js';
import { UmbBooleanState, UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbEditableWorkspaceContextBase } from '@umbraco-cms/backoffice/workspace';
import { loadCodeEditor } from '@umbraco-cms/backoffice/code-editor';

export class UmbScriptWorkspaceContext extends UmbEditableWorkspaceContextBase<
	UmbScriptDetailRepository,
	UmbScriptDetailModel
> {
	#data = new UmbObjectState<UmbScriptDetailModel | undefined>(undefined);
	data = this.#data.asObservable();
	name = this.#data.asObservablePart((data) => data?.name);
	content = this.#data.asObservablePart((data) => data?.content);
	path = this.#data.asObservablePart((data) => data?.path);

	#isCodeEditorReady = new UmbBooleanState(false);
	isCodeEditorReady = this.#isCodeEditorReady.asObservable();

	constructor(host: UmbControllerHostElement) {
		super(host, UMB_SCRIPT_WORKSPACE_ALIAS, new UmbScriptDetailRepository(host));
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

	getEntityId() {
		const data = this.getData();
		if (!data) throw new Error('Data is missing');
		return data.unique;
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
			this.#data.next(newData);
			this.saveComplete(newData);
		}
	}

	destroy(): void {
		throw new Error('Method not implemented.');
	}

	getEntityType(): string {
		const data = this.getData();
		if (!data) throw new Error('Data is missing');
		return data.entityType;
	}
}
