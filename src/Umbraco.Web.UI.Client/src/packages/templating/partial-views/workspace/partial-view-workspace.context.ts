import { UmbPartialViewsRepository } from '../repository/partial-views.repository.js';
import { PartialViewDetails } from '../config.js';
import { createObservablePart, UmbBooleanState, UmbDeepState } from '@umbraco-cms/backoffice/observable-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbSaveableWorkspaceContextInterface, UmbWorkspaceContext } from '@umbraco-cms/backoffice/workspace';
import { loadCodeEditor } from '@umbraco-cms/backoffice/code-editor';
import { UpdatePartialViewRequestModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export class UmbPartialViewWorkspaceContext extends UmbWorkspaceContext<
	UmbPartialViewsRepository,
	PartialViewDetails
> implements UmbSaveableWorkspaceContextInterface {
	getEntityId(): string | undefined {
		return this.getData()?.path;
	}
	getEntityType(): string {
		return 'partial-view';
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

	#data = new UmbDeepState<PartialViewDetails | undefined>(undefined);
	data = this.#data.asObservable();
	name = createObservablePart(this.#data, (data) => data?.name);
	content = createObservablePart(this.#data, (data) => data?.content);
	path = createObservablePart(this.#data, (data) => data?.path);

	#isCodeEditorReady = new UmbBooleanState(false);
	isCodeEditorReady = this.#isCodeEditorReady.asObservable();

	constructor(host: UmbControllerHostElement) {
		super(host, 'Umb.Workspace.PartialView', new UmbPartialViewsRepository(host));
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



export const UMB_PARTIAL_VIEW_WORKSPACE_CONTEXT = new UmbContextToken<UmbSaveableWorkspaceContextInterface, UmbPartialViewWorkspaceContext>(
	'UmbWorkspaceContext',
	(context): context is UmbPartialViewWorkspaceContext => context.getEntityType?.() === 'partial-view'
);
