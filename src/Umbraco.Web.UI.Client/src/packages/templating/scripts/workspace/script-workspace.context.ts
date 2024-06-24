import { UmbScriptDetailRepository } from '../repository/index.js';
import type { UmbScriptDetailModel } from '../types.js';
import { UMB_SCRIPT_ENTITY_TYPE } from '../entity.js';
import { UMB_SCRIPT_WORKSPACE_ALIAS } from './manifests.js';
import { UmbScriptWorkspaceEditorElement } from './script-workspace-editor.element.js';
import { UmbBooleanState, UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import {
	UmbSubmittableWorkspaceContextBase,
	type UmbRoutableWorkspaceContext,
	type UmbSubmittableWorkspaceContext,
	UmbWorkspaceIsNewRedirectController,
	UmbWorkspaceRouteManager,
} from '@umbraco-cms/backoffice/workspace';
import { loadCodeEditor } from '@umbraco-cms/backoffice/code-editor';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import {
	UmbRequestReloadChildrenOfEntityEvent,
	UmbRequestReloadStructureForEntityEvent,
} from '@umbraco-cms/backoffice/entity-action';
import type { IRoutingInfo, PageComponent } from '@umbraco-cms/backoffice/router';

export class UmbScriptWorkspaceContext
	extends UmbSubmittableWorkspaceContextBase<UmbScriptDetailModel>
	implements UmbSubmittableWorkspaceContext, UmbRoutableWorkspaceContext
{
	public readonly repository = new UmbScriptDetailRepository(this);

	#parent = new UmbObjectState<{ entityType: string; unique: string | null } | undefined>(undefined);
	readonly parentUnique = this.#parent.asObservablePart((parent) => (parent ? parent.unique : undefined));
	readonly parentEntityType = this.#parent.asObservablePart((parent) => (parent ? parent.entityType : undefined));

	#data = new UmbObjectState<UmbScriptDetailModel | undefined>(undefined);

	readonly data = this.#data.asObservable();
	readonly unique = this.#data.asObservablePart((data) => data?.unique);
	readonly entityType = this.#data.asObservablePart((data) => data?.entityType);
	readonly name = this.#data.asObservablePart((data) => data?.name);
	readonly content = this.#data.asObservablePart((data) => data?.content);

	#isCodeEditorReady = new UmbBooleanState(false);
	readonly isCodeEditorReady = this.#isCodeEditorReady.asObservable();

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

	protected override resetState(): void {
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
		const { data, asObservable } = await this.repository.requestByUnique(unique);

		if (data) {
			this.setIsNew(false);
			this.#data.setValue(data);

			this.observe(asObservable(), (data) => this.onDetailStoreChanges(data), 'umbDetailStoreObserver');
		}
	}

	onDetailStoreChanges(data: UmbScriptDetailModel | undefined) {
		// Data is removed from the store
		// TODO: revisit. We need to handle what should happen when the data is removed from the store
		if (data === undefined) {
			this.#data.setValue(undefined);
		}
	}

	async create(parent: { entityType: string; unique: string | null }) {
		this.resetState();
		this.#parent.setValue(parent);
		const { data } = await this.repository.createScaffold();

		if (data) {
			this.setIsNew(true);
			this.#data.setValue(data);
		}
	}

	async submit() {
		if (!this.#data.value) throw new Error('Data is missing');

		if (this.getIsNew()) {
			const parent = this.#parent.getValue();
			if (!parent) throw new Error('Parent is not set');
			const { error, data } = await this.repository.create(this.#data.value, parent.unique);
			if (error) {
				throw new Error(error.message);
			}
			this.#data.setValue(data);
			this.setIsNew(false);

			// TODO: this might not be the right place to alert the tree, but it works for now
			const eventContext = await this.getContext(UMB_ACTION_EVENT_CONTEXT);
			const event = new UmbRequestReloadChildrenOfEntityEvent({
				entityType: parent.entityType,
				unique: parent.unique,
			});
			eventContext.dispatchEvent(event);
		} else {
			const { error, data } = await this.repository.save(this.#data.value);
			if (error) {
				throw new Error(error.message);
			}
			this.#data.setValue(data);

			const actionEventContext = await this.getContext(UMB_ACTION_EVENT_CONTEXT);
			const event = new UmbRequestReloadStructureForEntityEvent({
				unique: this.getUnique()!,
				entityType: this.getEntityType(),
			});

			actionEventContext.dispatchEvent(event);
		}
	}

	override destroy(): void {
		super.destroy();
		this.#data.destroy();
	}
}

export { UmbScriptWorkspaceContext as api };
