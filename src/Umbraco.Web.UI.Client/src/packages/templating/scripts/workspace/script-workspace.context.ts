import { UmbScriptDetailRepository } from '../repository/index.js';
import { UMB_SCRIPT_WORKSPACE_ALIAS } from './manifests.js';
import { UmbBooleanState, UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbEditableWorkspaceContextBase } from '@umbraco-cms/backoffice/workspace';
import { loadCodeEditor } from '@umbraco-cms/backoffice/code-editor';
import { TextFileResponseModelBaseModel, UpdateScriptRequestModel } from '@umbraco-cms/backoffice/backend-api';

export class UmbScriptWorkspaceContext extends UmbEditableWorkspaceContextBase<
	UmbScriptDetailRepository,
	TextFileResponseModelBaseModel
> {
	#data = new UmbObjectState<TextFileResponseModelBaseModel | undefined>(undefined);
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
		const { data } = await this.repository.requestByKey(unique);
		if (data) {
			this.setIsNew(false);
			this.#data.next(data);
		}
	}

	async create(parentUnique: string) {
		const newScript: TextFileResponseModelBaseModel = {
			name: '',
			path: parentUnique,
			content: '',
		};
		this.#data.next(newScript);
		this.setIsNew(true);
	}

	getEntityId() {
		const path = this.getData()?.path?.replace(/\//g, '%2F');
		const name = this.getData()?.name;

		// Note: %2F is a slash (/)
		return path && name ? `${path}%2F${name}` : name || '';
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

			const { error } = await this.repository.create(createRequestBody);
			if (!error) {
				this.setIsNew(false);
			}
			return Promise.resolve();
		}
		if (!script.path) return Promise.reject('There is no path');
		const updateRequestBody: UpdateScriptRequestModel = {
			name: script.name,
			existingPath: script.path,
			content: script.content,
		};
		const { error } = await this.repository.save(script.path, updateRequestBody);
		if (!error) {
			//TODO Update the URL to the new name
		}
		return Promise.resolve();
	}

	destroy(): void {
		throw new Error('Method not implemented.');
	}

	getEntityType(): string {
		throw new Error('Method not implemented.');
	}
}
