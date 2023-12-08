import { UmbPartialViewRepository } from '../repository/partial-view.repository.js';
import type { UmbPartialViewDetailModel } from '../types.js';
import { UMB_PARTIAL_VIEW_ENTITY_TYPE } from '../entity.js';
import { UmbBooleanState, UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import {
	UmbSaveableWorkspaceContextInterface,
	UmbEditableWorkspaceContextBase,
} from '@umbraco-cms/backoffice/workspace';
import { loadCodeEditor } from '@umbraco-cms/backoffice/code-editor';
import type { UpdatePartialViewRequestModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export class UmbPartialViewWorkspaceContext
	extends UmbEditableWorkspaceContextBase<UmbPartialViewRepository, UmbPartialViewDetailModel>
	implements UmbSaveableWorkspaceContextInterface
{
	getEntityId(): string | undefined {
		return this.getData()?.path;
	}
	getEntityType(): string {
		return UMB_PARTIAL_VIEW_ENTITY_TYPE;
	}
	save(): Promise<void> {
		const partialView = this.getData();

		if (!partialView)
			return Promise.reject('Something went wrong, there is no data for partial view you want to save...');
		if (this.getIsNew()) {
			const createRequestBody = {
				name: partialView.name,
				content: partialView.content,
				parentPath: partialView.path + '/',
			};

			this.repository.create(createRequestBody);
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

	#data = new UmbObjectState<UmbPartialViewDetailModel | undefined>(undefined);
	readonly data = this.#data.asObservable();
	readonly name = this.#data.asObservablePart((data) => data?.name);
	readonly content = this.#data.asObservablePart((data) => data?.content);
	readonly path = this.#data.asObservablePart((data) => data?.path);

	#isCodeEditorReady = new UmbBooleanState(false);
	readonly isCodeEditorReady = this.#isCodeEditorReady.asObservable();

	constructor(host: UmbControllerHostElement) {
		super(host, 'Umb.Workspace.PartialView', new UmbPartialViewRepository(host));
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

export const UMB_PARTIAL_VIEW_WORKSPACE_CONTEXT = new UmbContextToken<
	UmbSaveableWorkspaceContextInterface,
	UmbPartialViewWorkspaceContext
>(
	'UmbWorkspaceContext',
	undefined,
	(context): context is UmbPartialViewWorkspaceContext => context.getEntityType?.() === UMB_PARTIAL_VIEW_ENTITY_TYPE,
);
