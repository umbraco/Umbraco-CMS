import { UmbStylesheetRepository } from '../repository/stylesheet.repository.js';
import { StylesheetDetails } from '../index.js';
import { UmbWorkspaceContext } from '@umbraco-cms/backoffice/workspace';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbBooleanState, UmbObjectState, createObservablePart } from '@umbraco-cms/backoffice/observable-api';
import { loadCodeEditor } from '@umbraco-cms/backoffice/code-editor';
import {
	RichTextStylesheetRulesResponseModel,
	UpdateStylesheetRequestModel,
} from '@umbraco-cms/backoffice/backend-api';

export class UmbStylesheetWorkspaceContext extends UmbWorkspaceContext<UmbStylesheetRepository, StylesheetDetails> {
	#data = new UmbObjectState<StylesheetDetails | undefined>(undefined);
	#rules = new UmbObjectState<RichTextStylesheetRulesResponseModel | undefined>(undefined);
	data = this.#data.asObservable();
	name = createObservablePart(this.#data, (data) => data?.name);
	content = createObservablePart(this.#data, (data) => data?.content);
	path = createObservablePart(this.#data, (data) => data?.path);

	#isCodeEditorReady = new UmbBooleanState(false);
	isCodeEditorReady = this.#isCodeEditorReady.asObservable();

	constructor(host: UmbControllerHostElement) {
		super(host, 'Umb.Workspace.StyleSheet', new UmbStylesheetRepository(host));
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
		const [{ data }, rules] = await Promise.all([
			this.repository.requestById(path),
			this.repository.getStylesheetRules(path),
		]);

		if (data) {
			this.setIsNew(false);
			this.#data.update(data);
		}

		if (rules.data) {
			this.#rules.update(rules.data);
		}
	}

	public async save() {
		const stylesheet = this.getData();

		if (!stylesheet)
			return Promise.reject('Something went wrong, there is no data for partial view you want to save...');
		if (this.getIsNew()) {
			const createRequestBody = {
				name: stylesheet.name,
				content: stylesheet.content,
				parentPath: stylesheet.path === 'null' ? '' : stylesheet.path + '/',
			};

			this.repository.create(createRequestBody);
			return Promise.resolve();
		}
		if (!stylesheet.path) return Promise.reject('There is no path');
		const updateRequestBody: UpdateStylesheetRequestModel = {
			name: stylesheet.name,
			existingPath: stylesheet.path,
			content: stylesheet.content,
		};
		this.repository.save(stylesheet.path, updateRequestBody);
		return Promise.resolve();
	}

	async create(parentKey: string | null) {
		const newStylesheet = {
			name: '',
			path: parentKey ?? '',
			content: '',
		};
		this.setIsNew(true);
		this.#data.next(newStylesheet);
	}

	public destroy(): void {
		this.#data.complete();
	}
}
