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
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import { UmbReloadTreeItemChildrenRequestEntityActionEvent } from '@umbraco-cms/backoffice/tree';

export class UmbPartialViewWorkspaceContext
	extends UmbEditableWorkspaceContextBase<UmbPartialViewDetailModel>
	implements UmbSaveableWorkspaceContextInterface
{
	public readonly repository = new UmbPartialViewDetailRepository(this);

	#parent?: { entityType: string; unique: string | null };

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

	protected resetState(): void {
		super.resetState();
		this.#data.setValue(undefined);
	}

	async #loadCodeEditor() {
		try {
			await loadCodeEditor();
			this.#isCodeEditorReady.setValue(true);
		} catch (error) {
			console.error(error);
		}
	}

	getUnique() {
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
		this.resetState();
		const { data } = await this.repository.requestByUnique(unique);
		if (data) {
			this.setIsNew(false);
			this.#data.setValue(data);
		}
	}

	async create(parent: { entityType: string; unique: string | null }, snippetId?: string) {
		this.resetState();
		this.#parent = parent;
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
			if (!this.#parent) throw new Error('Parent is not set');
			const { data } = await this.repository.create(this.#data.value, this.#parent.unique);

			// TODO: this might not be the right place to alert the tree, but it works for now
			const eventContext = await this.getContext(UMB_ACTION_EVENT_CONTEXT);
			const event = new UmbReloadTreeItemChildrenRequestEntityActionEvent({
				entityType: this.#parent.entityType,
				unique: this.#parent.unique,
			});

			eventContext.dispatchEvent(event);

			newData = data;
		} else {
			const { data } = await this.repository.save(this.#data.value);
			newData = data;
		}

		if (newData) {
			this.#data.setValue(newData);
			this.setIsNew(false);
			this.workspaceComplete(newData);
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
