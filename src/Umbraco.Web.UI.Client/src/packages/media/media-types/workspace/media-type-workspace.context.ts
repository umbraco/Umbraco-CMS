import { UmbMediaTypeDetailRepository } from '../repository/detail/media-type-detail.repository.js';
import { UMB_MEDIA_TYPE_ENTITY_TYPE } from '../entity.js';
import type { UmbMediaTypeDetailModel } from '../types.js';
import { UmbMediaTypeWorkspaceEditorElement } from './media-type-workspace-editor.element.js';
import {
	UmbSubmittableWorkspaceContextBase,
	type UmbRoutableWorkspaceContext,
	UmbWorkspaceIsNewRedirectController,
} from '@umbraco-cms/backoffice/workspace';
import { UmbContentTypeStructureManager } from '@umbraco-cms/backoffice/content-type';
import { UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import type {
	UmbContentTypeCompositionModel,
	UmbContentTypeSortModel,
	UmbContentTypeWorkspaceContext,
} from '@umbraco-cms/backoffice/content-type';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbReferenceByUnique } from '@umbraco-cms/backoffice/models';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import {
	UmbRequestReloadChildrenOfEntityEvent,
	UmbRequestReloadStructureForEntityEvent,
} from '@umbraco-cms/backoffice/entity-action';

type EntityType = UmbMediaTypeDetailModel;
export class UmbMediaTypeWorkspaceContext
	extends UmbSubmittableWorkspaceContextBase<EntityType>
	implements UmbContentTypeWorkspaceContext<EntityType>, UmbRoutableWorkspaceContext
{
	readonly IS_CONTENT_TYPE_WORKSPACE_CONTEXT = true;
	//
	public readonly repository: UmbMediaTypeDetailRepository = new UmbMediaTypeDetailRepository(this);
	// Draft is located in structure manager

	#parent = new UmbObjectState<{ entityType: string; unique: string | null } | undefined>(undefined);
	readonly parentUnique = this.#parent.asObservablePart((parent) => (parent ? parent.unique : undefined));
	readonly parentEntityType = this.#parent.asObservablePart((parent) => (parent ? parent.entityType : undefined));

	#persistedData = new UmbObjectState<EntityType | undefined>(undefined);

	// General for content types:
	readonly data;
	readonly unique;
	readonly entityType;
	readonly name;
	getName(): string | undefined {
		return this.structure.getOwnerContentType()?.name;
	}
	readonly alias;
	readonly description;
	readonly icon;

	readonly allowedAtRoot;
	readonly variesByCulture;
	readonly variesBySegment;
	readonly allowedContentTypes;
	readonly compositions;
	readonly collection;

	readonly structure = new UmbContentTypeStructureManager<EntityType>(this, this.repository);

	constructor(host: UmbControllerHost) {
		super(host, 'Umb.Workspace.MediaType');

		// General for content types:
		this.data = this.structure.ownerContentType;
		this.unique = this.structure.ownerContentTypeObservablePart((data) => data?.unique);
		this.entityType = this.structure.ownerContentTypeObservablePart((data) => data?.entityType);
		this.name = this.structure.ownerContentTypeObservablePart((data) => data?.name);
		this.alias = this.structure.ownerContentTypeObservablePart((data) => data?.alias);
		this.description = this.structure.ownerContentTypeObservablePart((data) => data?.description);
		this.icon = this.structure.ownerContentTypeObservablePart((data) => data?.icon);
		this.allowedAtRoot = this.structure.ownerContentTypeObservablePart((data) => data?.allowedAtRoot);
		this.variesByCulture = this.structure.ownerContentTypeObservablePart((data) => data?.variesByCulture);
		this.variesBySegment = this.structure.ownerContentTypeObservablePart((data) => data?.variesBySegment);
		this.allowedContentTypes = this.structure.ownerContentTypeObservablePart((data) => data?.allowedContentTypes);
		this.compositions = this.structure.ownerContentTypeObservablePart((data) => data?.compositions);
		this.collection = this.structure.ownerContentTypeObservablePart((data) => data?.collection);

		this.routes.setRoutes([
			{
				path: 'create/parent/:entityType/:parentUnique',
				component: UmbMediaTypeWorkspaceEditorElement,
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
				path: 'edit/:id',
				component: UmbMediaTypeWorkspaceEditorElement,
				setup: (_component, info) => {
					const id = info.match.params.id;
					this.load(id);
				},
			},
		]);
	}

	protected override resetState(): void {
		super.resetState();
		this.#persistedData.setValue(undefined);
	}

	getData() {
		return this.structure.getOwnerContentType();
	}

	getUnique() {
		return this.getData()?.unique;
	}

	getEntityType() {
		return UMB_MEDIA_TYPE_ENTITY_TYPE;
	}

	setName(name: string) {
		this.structure.updateOwnerContentType({ name });
	}

	setAlias(alias: string) {
		this.structure.updateOwnerContentType({ alias });
	}

	setDescription(description: string) {
		this.structure.updateOwnerContentType({ description });
	}

	// TODO: manage setting icon color alias?
	setIcon(icon: string) {
		this.structure.updateOwnerContentType({ icon });
	}

	setAllowedAtRoot(allowedAtRoot: boolean) {
		this.structure.updateOwnerContentType({ allowedAtRoot });
	}

	setVariesByCulture(variesByCulture: boolean) {
		this.structure.updateOwnerContentType({ variesByCulture });
	}

	setVariesBySegment(variesBySegment: boolean) {
		this.structure.updateOwnerContentType({ variesBySegment });
	}

	setIsElement(isElement: boolean) {
		this.structure.updateOwnerContentType({ isElement });
	}

	setAllowedContentTypes(allowedContentTypes: Array<UmbContentTypeSortModel>) {
		this.structure.updateOwnerContentType({ allowedContentTypes });
	}

	setCompositions(compositions: Array<UmbContentTypeCompositionModel>) {
		this.structure.updateOwnerContentType({ compositions });
	}

	setCollection(collection: UmbReferenceByUnique) {
		this.structure.updateOwnerContentType({ collection });
	}

	async create(parent: { entityType: string; unique: string | null }) {
		this.resetState();
		this.#parent.setValue(parent);
		const { data } = await this.structure.createScaffold();
		if (!data) return undefined;

		this.setIsNew(true);
		this.#persistedData.setValue(data);
		return data;
	}

	async load(unique: string) {
		this.resetState();
		const { data, asObservable } = await this.structure.loadType(unique);

		if (data) {
			this.setIsNew(false);
			this.#persistedData.update(data);
		}

		if (asObservable) {
			this.observe(asObservable(), (entity) => this.#onStoreChange(entity), 'umbMediaTypeStoreObserver');
		}
	}

	#onStoreChange(entity: EntityType | undefined) {
		if (!entity) {
			//TODO: This solution is alright for now. But reconsider when we introduce signal-r
			history.pushState(null, '', 'section/settings/workspace/media-type-root');
		}
	}

	/**
	 * Save or creates the media type, based on wether its a new one or existing.
	 */
	async submit() {
		const data = this.getData();
		if (!data) {
			throw new Error('Something went wrong, there is no data for media type you want to save...');
		}

		if (this.getIsNew()) {
			const parent = this.#parent.getValue();
			if (!parent) throw new Error('Parent is not set');

			await this.structure.create(parent.unique);

			const eventContext = await this.getContext(UMB_ACTION_EVENT_CONTEXT);
			const event = new UmbRequestReloadChildrenOfEntityEvent({
				entityType: parent.entityType,
				unique: parent.unique,
			});
			eventContext.dispatchEvent(event);
			this.setIsNew(false);
		} else {
			await this.structure.save();

			const actionEventContext = await this.getContext(UMB_ACTION_EVENT_CONTEXT);
			const event = new UmbRequestReloadStructureForEntityEvent({
				unique: this.getUnique()!,
				entityType: this.getEntityType(),
			});

			actionEventContext.dispatchEvent(event);
		}
	}

	public override destroy(): void {
		this.#persistedData.destroy();
		this.structure.destroy();
		this.repository.destroy();
		super.destroy();
	}
}

export { UmbMediaTypeWorkspaceContext as api };
