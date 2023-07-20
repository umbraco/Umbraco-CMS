import { UmbPartialViewsRepository } from '../repository/partial-views.repository.js';
import { PartialViewDetails } from '../config.js';
import { createObservablePart, UmbBooleanState, UmbDeepState } from '@umbraco-cms/backoffice/observable-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbWorkspaceContext } from '@umbraco-cms/backoffice/workspace';
import { loadCodeEditor } from '@umbraco-cms/backoffice/code-editor';
import { UpdatePartialViewRequestModel } from '@umbraco-cms/backoffice/backend-api';

export class UmbPartialViewsWorkspaceContext extends UmbWorkspaceContext<UmbPartialViewsRepository, PartialViewDetails> {
	getEntityId(): string | undefined {
		throw new Error('Method not implemented.');
	}
	getEntityType(): string {
		throw new Error('Method not implemented.');
	}
	save(): Promise<void> {
		const partialView = this.getData();

		if (!partialView)
			return Promise.reject('Something went wrong, there is no data for partial view you want to save...');
		if (this.getIsNew()) {
			//this.repository.create()
			console.log('create');
			return Promise.resolve();
		}
		if (!partialView.path) return Promise.reject('There is no path');
		const updateRequestBody: UpdatePartialViewRequestModel = {
			name: partialView.name,
			existingPath: partialView.path,
			content: partialView.content,
		};
		this.repository.save(partialView.path, updateRequestBody);
		return Promise.resolve();
	}
	destroy(): void {
		throw new Error('Method not implemented.');
	}
	#data = new UmbDeepState<PartialViewDetails | undefined>(undefined);
	data = this.#data.asObservable();
	name = createObservablePart(this.#data, (data) => data?.name);
	content = createObservablePart(this.#data, (data) => data?.content);
	path = createObservablePart(this.#data, (data) => data?.path);

	#isCodeEditorReady = new UmbBooleanState(false);
	isCodeEditorReady = this.#isCodeEditorReady.asObservable();

	constructor(host: UmbControllerHostElement) {
		super(host, new UmbPartialViewsRepository(host));
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
		debugger;
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
