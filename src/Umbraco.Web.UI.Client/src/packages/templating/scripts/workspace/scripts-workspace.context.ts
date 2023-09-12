import { ScriptDetails, SCRIPTS_WORKSPACE_ALIAS } from '../config.js';
import { UmbScriptsRepository } from '../repository/scripts.repository.js';
import { createObservablePart, UmbBooleanState, UmbDeepState } from '@umbraco-cms/backoffice/observable-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbWorkspaceContext } from '@umbraco-cms/backoffice/workspace';
import { loadCodeEditor } from '@umbraco-cms/backoffice/code-editor';
import { TextFileResponseModelBaseModel, UpdateScriptRequestModel } from '@umbraco-cms/backoffice/backend-api';

export class UmbScriptsWorkspaceContext extends UmbWorkspaceContext<UmbScriptsRepository, ScriptDetails> {
	#data = new UmbDeepState<ScriptDetails | undefined>(undefined);
	data = this.#data.asObservable();
	name = createObservablePart(this.#data, (data) => data?.name);
	content = createObservablePart(this.#data, (data) => data?.content);
	path = createObservablePart(this.#data, (data) => data?.path);

	#isCodeEditorReady = new UmbBooleanState(false);
	isCodeEditorReady = this.#isCodeEditorReady.asObservable();

	constructor(host: UmbControllerHostElement) {
		super(host, SCRIPTS_WORKSPACE_ALIAS, new UmbScriptsRepository(host));
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

	async create(parentKey: string) {
		const newScript: TextFileResponseModelBaseModel = {
			name: '',
			path: parentKey,
			content: '',
		};
		this.#data.next(newScript);
		this.setIsNew(true);
	}

	getEntityId(): string | undefined {
		return this.getData()?.path;
	}

	public async save() {
		const script = this.getData();

		if (!script) {
			return Promise.reject('Something went wrong, there is no data for script you want to save...');
		}
		if (this.getIsNew()) {
			const createRequestBody = {
				name: script.name,
				content: script.content,
				parentPath: script.path + '/',
			};

			this.repository.create(createRequestBody);
			return Promise.resolve();
		}
		if (!script.path) return Promise.reject('There is no path');
		const updateRequestBody: UpdateScriptRequestModel = {
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

	getEntityType(): string {
		throw new Error('Method not implemented.');
	}
}
