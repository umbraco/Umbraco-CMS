import { ScriptDetails } from '../config.js';
import { UmbScriptsRepository } from '../repository/scripts.repository.js';
import { createObservablePart, UmbBooleanState, UmbDeepState } from '@umbraco-cms/backoffice/observable-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbWorkspaceContext } from '@umbraco-cms/backoffice/workspace';
import { loadCodeEditor } from '@umbraco-cms/backoffice/code-editor';
import { UpdatePartialViewRequestModel } from '@umbraco-cms/backoffice/backend-api';

export class UmbScriptsWorkspaceContext extends UmbWorkspaceContext<UmbScriptsRepository, ScriptDetails> {
	getEntityId(): string | undefined {
		return this.getData()?.path;
	}
	getEntityType(): string {
		throw new Error('Method not implemented.');
	}
	save(): Promise<void> {
		const script = this.getData();

		if (!script) return Promise.reject('Something went wrong, there is no data for partial view you want to save...');
		if (this.getIsNew()) {
			const createRequestBody = {
				name: script.name,
				content: script.content,
				parentPath: script.path + '/',
			};

			this.repository.create(createRequestBody);
			console.log('create');
			return Promise.resolve();
		}
		if (!script.path) return Promise.reject('There is no path');
		const updateRequestBody: UpdatePartialViewRequestModel = {
			name: script.name,
			existingPath: script.path,
			content: script.content,
		};
		this.repository.save(script.path, updateRequestBody);
		return Promise.resolve();
	}
	destroy(): void {
		throw new Error('Method not implemented.');
	}
	#data = new UmbDeepState<ScriptDetails | undefined>(undefined);
	data = this.#data.asObservable();
	name = createObservablePart(this.#data, (data) => data?.name);
	content = createObservablePart(this.#data, (data) => data?.content);
	path = createObservablePart(this.#data, (data) => data?.path);

	#isCodeEditorReady = new UmbBooleanState(false);
	isCodeEditorReady = this.#isCodeEditorReady.asObservable();

	constructor(host: UmbControllerHostElement) {
		super(host, 'Umb.Workspace.PartialViews', new UmbScriptsRepository(host));
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
		this.#data.next({ ...this.#data.value, name: value });
	}

	setContent(value: string) {
		this.#data.next({ ...this.#data.value, content: value });
	}

	async load(entityKey: string) {
		const { data } = await this.repository.requestByKey(entityKey);
		if (data) {
			this.setIsNew(false);
			this.#data.next(data);
		}
	}

	async create(parentKey: string | null, name = 'Empty') {
		const { data } = await this.repository.createScaffold(parentKey, name);
		const newPartial = {
			...data,
			name: '',
			path: parentKey ?? '',
		};
		if (!data) return;
		this.setIsNew(true);
		this.#data.next(newPartial);
	}
}
