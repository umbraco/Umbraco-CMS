import { UmbStylesheetRepository } from '../repository/stylesheet.repository.js';
import { StylesheetDetails } from '../index.js';
import { UmbWorkspaceContext } from '@umbraco-cms/backoffice/workspace';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbArrayState, UmbBooleanState, UmbObjectState, createObservablePart } from '@umbraco-cms/backoffice/observable-api';
import { loadCodeEditor } from '@umbraco-cms/backoffice/code-editor';
import { RichTextRuleModel, UpdateStylesheetRequestModel } from '@umbraco-cms/backoffice/backend-api';

export type RichTextRuleModelSortable = RichTextRuleModel & { sortOrder?: number };

export class UmbStylesheetWorkspaceContext extends UmbWorkspaceContext<UmbStylesheetRepository, StylesheetDetails> {
	#data = new UmbObjectState<StylesheetDetails | undefined>(undefined);
	#rules = new UmbArrayState<RichTextRuleModelSortable>([], (rule) => rule.name);
	data = this.#data.asObservable();
	rules = this.#rules.asObservable();
	name = createObservablePart(this.#data, (data) => data?.name);
	content = createObservablePart(this.#data, (data) => data?.content);
	path = createObservablePart(this.#data, (data) => data?.path);

	#isCodeEditorReady = new UmbBooleanState(false);
	isCodeEditorReady = this.#isCodeEditorReady.asObservable();

	constructor(host: UmbControllerHostElement) {
		super(host, 'Umb.Workspace.StyleSheet', new UmbStylesheetRepository(host));
		this.#rules.sortBy((a, b) => (a.sortOrder ?? 0) - (b.sortOrder ?? 0));
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

	getRules() {
		return this.#rules.getValue();
	}

	updateRule(unique: string, rule: RichTextRuleModelSortable) {
		this.#rules.updateOne(unique, rule);
	}

	setRules(rules: RichTextRuleModelSortable[]) {
		const newRules = rules.map((r, i) => ({ ...r, sortOrder: i }));
		this.#rules.next(newRules);
		this.sendRulesGetContent();
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
			const x = rules.data.rules?.map((r, i) => ({ ...r, sortOrder: i })) ?? [];
			this.#rules.next(x);
		}
	}

	async sendRulesGetContent() {
		const requestBody = {
			content: this.getData()?.content,
			rules: this.getRules(),
		};

		const { data } = await this.repository.interpolateStylesheetRules(requestBody);
		this.setContent(data?.content ?? '');
	}

	async sendContentGetRules() {

		if (!this.getData()?.content) return;

		const requestBody = {
			content: this.getData()?.content,
		};

		const { data } = await this.repository.extractStylesheetRules(requestBody);
		this.setRules(data?.rules ?? []);
	}

	findNewSortOrder(rule: RichTextRuleModel, newIndex: number) {
		const rules = [...this.getRules()].sort((a, b) => (a.sortOrder ?? 0) - (b.sortOrder ?? 0));
		const oldIndex = rules.findIndex((r) => r.name === rule.name);

		if (oldIndex === -1) return false;
		rules.splice(oldIndex, 1);
		rules.splice(newIndex, 0, rule);
		this.setRules(rules.map((r, i) => ({ ...r, sortOrder: i })));
		return true;
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
		this.#data.next(newStylesheet);
		this.setIsNew(true);
	}

	public destroy(): void {
		this.#data.complete();
	}
}
