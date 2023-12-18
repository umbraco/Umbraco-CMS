import { UmbStylesheetRepository } from '../repository/stylesheet-detail.repository.js';
import type { UmbStylesheetDetailModel } from '../types.js';
import {
	type UmbSaveableWorkspaceContextInterface,
	UmbEditableWorkspaceContextBase,
} from '@umbraco-cms/backoffice/workspace';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbArrayState, UmbBooleanState, UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import { loadCodeEditor } from '@umbraco-cms/backoffice/code-editor';
import type { RichTextRuleModel, UpdateStylesheetRequestModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export type RichTextRuleModelSortable = RichTextRuleModel & { sortOrder?: number };

export class UmbStylesheetWorkspaceContext
	extends UmbEditableWorkspaceContextBase<UmbStylesheetRepository, UmbStylesheetDetailModel>
	implements UmbSaveableWorkspaceContextInterface<UmbStylesheetDetailModel | undefined>
{
	#data = new UmbObjectState<UmbStylesheetDetailModel | undefined>(undefined);
	#rules = new UmbArrayState<RichTextRuleModelSortable>([], (rule) => rule.name);
	readonly data = this.#data.asObservable();
	readonly rules = this.#rules.asObservable();
	readonly name = this.#data.asObservablePart((data) => data?.name);
	readonly content = this.#data.asObservablePart((data) => data?.content);
	readonly path = this.#data.asObservablePart((data) => data?.path);

	#isCodeEditorReady = new UmbBooleanState(false);
	readonly isCodeEditorReady = this.#isCodeEditorReady.asObservable();

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
		const path = this.getData()?.path?.replace(/\//g, '%2F');
		const name = this.getData()?.name;

		// Note: %2F is a slash (/)
		return path && name ? `${path}%2F${name}` : name || '';
	}

	getData() {
		return this.#data.getValue();
	}

	getRules() {
		return this.#rules.getValue();
	}

	updateRule(unique: string, rule: RichTextRuleModelSortable) {
		this.#rules.updateOne(unique, rule);
		this.sendRulesGetContent();
	}

	setRules(rules: RichTextRuleModelSortable[]) {
		const newRules = rules.map((r, i) => ({ ...r, sortOrder: i }));
		this.#rules.next(newRules);
		this.sendRulesGetContent();
	}

	setName(value: string) {
		this.#data.update({ name: value });
	}

	setContent(value: string) {
		this.#data.update({ content: value });
	}

	async load(path: string) {
		const [{ data }, rules] = await Promise.all([
			this.repository.requestById(path),
			this.repository.getStylesheetRules(path),
		]);

		if (data) {
			this.setIsNew(false);
			this.#data.update(data);
		} else {
			this.#data.update(undefined);
		}

		if (rules.data) {
			const x = rules.data.rules?.map((r, i) => ({ ...r, sortOrder: i })) ?? [];
			this.#rules.next(x);
		} else {
			this.#rules.next([]);
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
		const content = this.getData()?.content;
		if (!content) throw Error('No content');

		const { data } = await this.repository.extractStylesheetRules({ content });
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

		if (!stylesheet) {
			return Promise.reject('Something went wrong, there is no data for partial view you want to save...');
		}

		if (this.getIsNew()) {
			const createRequestBody = {
				name: stylesheet.name,
				content: stylesheet.content,
				parentPath: stylesheet.path ?? '',
			};

			const { error } = await this.repository.create(createRequestBody);
			if (!error) {
				this.setIsNew(false);
			}
			return Promise.resolve();
		} else {
			if (!stylesheet.path) return Promise.reject('There is no path');
			const updateRequestBody: UpdateStylesheetRequestModel = {
				name: stylesheet.name,
				existingPath: stylesheet.path,
				content: stylesheet.content,
			};

			const { error } = await this.repository.save(stylesheet.path, updateRequestBody);
			if (!error) {
				//TODO Update the URL to the new name
			}
			return Promise.resolve();
		}
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
		this.#data.destroy();
	}
}

export const UMB_STYLESHEET_WORKSPACE_CONTEXT = new UmbContextToken<
	UmbSaveableWorkspaceContextInterface,
	UmbStylesheetWorkspaceContext
>(
	'UmbWorkspaceContext',
	undefined,
	(context): context is UmbStylesheetWorkspaceContext => context.getEntityType?.() === 'stylesheet',
);
