import { UmbPartialViewDetailRepository } from '../repository/partial-view-detail.repository.js';
import type { UmbPartialViewDetailModel } from '../types.js';
import { UMB_PARTIAL_VIEW_ENTITY_TYPE } from '../entity.js';
import { UmbBooleanState, UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbSaveableWorkspaceContextInterface } from '@umbraco-cms/backoffice/workspace';
import { UmbEditableWorkspaceContextBase } from '@umbraco-cms/backoffice/workspace';
import { loadCodeEditor } from '@umbraco-cms/backoffice/code-editor';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';
import { PartialViewResource } from '@umbraco-cms/backoffice/external/backend-api';

export class UmbPartialViewWorkspaceContext
	extends UmbEditableWorkspaceContextBase<UmbPartialViewDetailModel>
	implements UmbSaveableWorkspaceContextInterface
{
	public readonly repository = new UmbPartialViewDetailRepository(this);

	#data = new UmbObjectState<UmbPartialViewDetailModel | undefined>(undefined);
	readonly data = this.#data.asObservable();
	readonly name = this.#data.asObservablePart((data) => data?.name);
	readonly content = this.#data.asObservablePart((data) => data?.content);
	readonly path = this.#data.asObservablePart((data) => data?.path);

	#isCodeEditorReady = new UmbBooleanState(false);
	readonly isCodeEditorReady = this.#isCodeEditorReady.asObservable();

	constructor(host: UmbControllerHost) {
		super(host, 'Umb.Workspace.PartialView');
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

	getEntityId() {
		const data = this.getData();
		if (!data) throw new Error('Data is missing');
		return data.unique;
	}

	getEntityType(): string {
		return UMB_PARTIAL_VIEW_ENTITY_TYPE;
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

	async create(parentUnique: string | null, snippetId?: string) {
		let snippetContent = '';

		if (snippetId) {
			const { data: snippet } = await this.#getSnippet(snippetId);
			snippetContent = snippet?.content || '';
		}

		const { data } = await this.repository.createScaffold({ content: snippetContent });

		if (data) {
			this.setIsNew(true);
			this.#data.setValue(data);
		}
	}

	public async save() {
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

	public destroy(): void {
		this.#data.destroy();
		super.destroy();
	}

	#getSnippet(snippetId: string) {
		return tryExecuteAndNotify(
			this,
			PartialViewResource.getPartialViewSnippetById({
				id: snippetId,
			}),
		);
	}
}

export const UMB_PARTIAL_VIEW_WORKSPACE_CONTEXT = new UmbContextToken<
	UmbSaveableWorkspaceContextInterface,
	UmbPartialViewWorkspaceContext
>(
	'UmbWorkspaceContext',
	undefined,
	(context): context is UmbPartialViewWorkspaceContext => context.getEntityType?.() === UMB_PARTIAL_VIEW_ENTITY_TYPE,
);
