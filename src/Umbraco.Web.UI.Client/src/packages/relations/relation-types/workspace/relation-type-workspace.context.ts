import { UmbRelationTypeRepository } from '../repository/relation-type.repository.js';
import { UmbRelationTypeWorkspaceEditorElement } from './relation-type-workspace-editor.element.js';
import {
	type UmbSaveableWorkspaceContextInterface,
	UmbEditableWorkspaceContextBase,
	type UmbRoutableWorkspaceContext,
	UmbWorkspaceRouteManager,
	UmbWorkspaceIsNewRedirectController,
} from '@umbraco-cms/backoffice/workspace';
import type { RelationTypeBaseModel, RelationTypeResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbRelationTypeWorkspaceContext
	extends UmbEditableWorkspaceContextBase<RelationTypeResponseModel>
	implements UmbSaveableWorkspaceContextInterface, UmbRoutableWorkspaceContext
{
	//
	public readonly repository: UmbRelationTypeRepository = new UmbRelationTypeRepository(this);

	#parent?: { entityType: string; unique: string | null };

	#data = new UmbObjectState<RelationTypeResponseModel | undefined>(undefined);
	readonly data = this.#data.asObservable();
	readonly name = this.#data.asObservablePart((data) => data?.name);
	readonly id = this.#data.asObservablePart((data) => data?.id);

	readonly routes = new UmbWorkspaceRouteManager(this);

	constructor(host: UmbControllerHost) {
		super(host, 'Umb.Workspace.RelationType');

		this.routes.setRoutes([
			{
				path: 'create/parent/:entityType/:parentUnique',
				component: UmbRelationTypeWorkspaceEditorElement,
				setup: (_component, info) => {
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
				component: UmbRelationTypeWorkspaceEditorElement,
				setup: (_component, info) => {
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

	async load(id: string) {
		this.resetState();
		const { data } = await this.repository.requestById(id);

		if (data) {
			this.setIsNew(false);
			this.#data.update(data);
		}
	}

	async create(parent: { entityType: string; unique: string | null }) {
		this.resetState();
		this.#parent = parent;
		const { data } = await this.repository.createScaffold();
		if (!data) return;
		this.setIsNew(true);
		this.#data.setValue(data);
	}

	async getRelations() {
		//TODO: How do we test this?
		return await this.repository.requestRelationsById(this.getUnique());
	}

	getData() {
		return this.#data.getValue();
	}

	getUnique() {
		return this.getData()?.id || '';
	}

	getEntityType() {
		return 'relation-type';
	}

	setName(name: string) {
		this.#data.update({ name });
	}

	async save() {
		if (!this.#data.value) return;
		if (!this.#data.value.id) return;

		let response = undefined;

		if (this.getIsNew()) {
			response = await this.repository.create(this.#data.value);
		} else {
			response = await this.repository.save(this.#data.value.id, this.#data.value);
		}

		if (response.error) return;

		// If it went well, then its not new anymore?.
		this.setIsNew(false);
	}

	update<K extends keyof RelationTypeBaseModel>(id: K, value: RelationTypeBaseModel[K]) {
		this.#data.update({ [id]: value });
	}

	async delete(id: string) {
		await this.repository.delete(id);
	}

	public destroy(): void {
		this.#data.destroy();
		super.destroy();
	}
}

export { UmbRelationTypeWorkspaceContext as api };
