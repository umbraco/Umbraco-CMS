import { UmbScriptDetailRepository } from '../repository/index.js';
import type { UmbScriptDetailModel } from '../types.js';
import { UMB_SCRIPT_ENTITY_TYPE } from '../entity.js';
import { UMB_SCRIPT_WORKSPACE_ALIAS } from './manifests.js';
import { UmbScriptWorkspaceEditorElement } from './script-workspace-editor.element.js';
import { UmbBooleanState, UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import {
	UmbSaveableWorkspaceContextBase,
	type UmbRoutableWorkspaceContext,
	type UmbSaveableWorkspaceContextInterface,
	UmbWorkspaceIsNewRedirectController,
	UmbWorkspaceRouteManager,
} from '@umbraco-cms/backoffice/workspace';
import { loadCodeEditor } from '@umbraco-cms/backoffice/code-editor';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import { UmbReloadTreeItemChildrenRequestEntityActionEvent } from '@umbraco-cms/backoffice/tree';
import { UmbRequestReloadStructureForEntityEvent } from '@umbraco-cms/backoffice/event';
import type { IRoutingInfo, PageComponent } from '@umbraco-cms/backoffice/router';

export class UmbScriptWorkspaceContext
	extends UmbSaveableWorkspaceContextBase<UmbScriptDetailModel>
	implements UmbSaveableWorkspaceContextInterface, UmbRoutableWorkspaceContext
{
	public readonly repository = new UmbScriptDetailRepository(this);

	#parent?: { entityType: string; unique: string | null };

	#data = new UmbObjectState<UmbScriptDetailModel | undefined>(undefined);
	readonly data = this.#data.asObservable();
	readonly unique = this.#data.asObservablePart((data) => data?.unique);
	readonly name = this.#data.asObservablePart((data) => data?.name);
	readonly content = this.#data.asObservablePart((data) => data?.content);
	readonly path = this.#data.asObservablePart((data) => data?.path);

	#isCodeEditorReady = new UmbBooleanState(false);
	readonly isCodeEditorReady = this.#isCodeEditorReady.asObservable();

	readonly routes = new UmbWorkspaceRouteManager(this);

	constructor(host: UmbControllerHost) {
		super(host, UMB_SCRIPT_WORKSPACE_ALIAS);
		this.#loadCodeEditor();

		this.routes.setRoutes([
			{
				path: 'create/parent/:entityType/:parentUnique',
				component: UmbScriptWorkspaceEditorElement,
				setup: async (component: PageComponent, info: IRoutingInfo) => {
					const parentEntityType = info.match.params.entityType;
					const parentUnique = info.match.params.parentUnique === 'null' ? null : info.match.params.parentUnique;
					this.create({ entityType: parentEntityType, unique: parentUnique });

					new UmbWorkspaceIsNewRedirectController(
						this,
						this,
						this.getHostElement().shadowRoot!.querySelector('umb-router-slot')!,
					);
				},
			},
			{
				path: 'edit/:unique',
				component: UmbScriptWorkspaceEditorElement,
				setup: (component: PageComponent, info: IRoutingInfo) => {
					const unique = info.match.params.unique;
					this.load(unique);
				},
			},
		]);
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

	getEntityType(): string {
		return UMB_SCRIPT_ENTITY_TYPE;
	}

	getUnique() {
		const data = this.getData();
		if (!data) throw new Error('Data is missing');
		return data.unique;
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

	async create(parent: { entityType: string; unique: string | null }) {
		this.resetState();
		this.#parent = parent;
		const { data } = await this.repository.createScaffold();

		if (data) {
			this.setIsNew(true);
			this.#data.setValue(data);
		}
	}

	async save() {
		if (!this.#data.value) throw new Error('Data is missing');

		let newData = undefined;

		if (this.getIsNew()) {
			if (!this.#parent) throw new Error('Parent is not set');
			const { data } = await this.repository.create(this.#data.value, this.#parent.unique);
			newData = data;

			// TODO: this might not be the right place to alert the tree, but it works for now
			const eventContext = await this.getContext(UMB_ACTION_EVENT_CONTEXT);
			const event = new UmbReloadTreeItemChildrenRequestEntityActionEvent({
				entityType: this.#parent.entityType,
				unique: this.#parent.unique,
			});
			eventContext.dispatchEvent(event);
		} else {
			const { data } = await this.repository.save(this.#data.value);
			newData = data;

			const actionEventContext = await this.getContext(UMB_ACTION_EVENT_CONTEXT);
			const event = new UmbRequestReloadStructureForEntityEvent({
				unique: this.getUnique()!,
				entityType: this.getEntityType(),
			});

			actionEventContext.dispatchEvent(event);
		}

		if (newData) {
			this.#data.setValue(newData);

			this.setIsNew(false);
			this.workspaceComplete(newData);
		}
	}

	destroy(): void {
		super.destroy();
		this.#data.destroy();
	}
}

export { UmbScriptWorkspaceContext as api };
