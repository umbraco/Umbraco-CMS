import { UmbStylesheetRepository } from '../repository/stylesheet.repository.js';
import { StylesheetDetails } from '../index.js';
import { UmbWorkspaceContext } from '@umbraco-cms/backoffice/workspace';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbBooleanState, UmbObjectState, createObservablePart } from '@umbraco-cms/backoffice/observable-api';
import { loadCodeEditor } from '@umbraco-cms/backoffice/code-editor';

export class UmbStylesheetWorkspaceContext extends UmbWorkspaceContext<UmbStylesheetRepository, StylesheetDetails> {
	#data = new UmbObjectState<StylesheetDetails | undefined>(undefined);
	data = this.#data.asObservable();
	name = createObservablePart(this.#data, (data) => data?.name);
	content = createObservablePart(this.#data, (data) => data?.content);
	path = createObservablePart(this.#data, (data) => data?.path);

	#isCodeEditorReady = new UmbBooleanState(false);
	isCodeEditorReady = this.#isCodeEditorReady.asObservable();
	
	constructor(host: UmbControllerHostElement) {
		super(host, new UmbStylesheetRepository(host));
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
		return 'stylesheet';
	}

	getEntityId() {
		return this.getData()?.path || '';
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

	async load(path: string) {
		const { data } = await this.repository.requestByPath(path);
		if (data) {
			this.setIsNew(false);
			this.#data.update(data);
		}
	}

	public async save() {
		throw new Error('Save method not implemented.');

		// const stylesheet = this.getData();

		// if (!stylesheet)
		// 	return Promise.reject('Something went wrong, there is no data for partial view you want to save...');
		// if (this.getIsNew()) {

		// 	const createRequestBody = {
		// 		name: stylesheet.name,
		// 		content: stylesheet.content,
		// 		parentPath: stylesheet.path + '/',
		// 	}

		// 	this.repository.create(createRequestBody);
		// 	return Promise.resolve();
		// }
		// if (!stylesheet.path) return Promise.reject('There is no path');
		// const updateRequestBody: UpdatePartialViewRequestModel = {
		// 	name: stylesheet.name,
		// 	existingPath: stylesheet.path,
		// 	content: stylesheet.content,
		// };
		// this.repository.save(stylesheet.path, updateRequestBody);
		// return Promise.resolve();
	}

	async create(parentKey: string | null) {
		// const { data } = await this.repository.createScaffold(parentKey, name);
		// const newStylesheet = {
		// 	...data,
		// 	name: '',
		// 	path: parentKey ?? '',
		// };
		// if (!data) return;
		// this.setIsNew(true);
		// this.#data.next(newStylesheet);

		throw new Error('Create method not implemented.');

	}

	public destroy(): void {
		this.#data.complete();
	}
}
