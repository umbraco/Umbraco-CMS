import { UmbMediaTypeDetailRepository } from '../repository/detail/media-type-detail.repository.js';
import { UMB_MEDIA_TYPE_ENTITY_TYPE } from '../entity.js';
import type { UmbMediaTypeDetailModel } from '../types.js';
import { UmbEditableWorkspaceContextBase } from '@umbraco-cms/backoffice/workspace';
import { UmbContentTypePropertyStructureManager } from '@umbraco-cms/backoffice/content-type';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbBooleanState, UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import type { UmbContentTypeCompositionModel, UmbContentTypeSortModel } from '@umbraco-cms/backoffice/content-type';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbReferenceByUnique } from '@umbraco-cms/backoffice/models';
import type { UmbSaveableWorkspaceContextInterface } from '@umbraco-cms/backoffice/workspace';

type EntityType = UmbMediaTypeDetailModel;
export class UmbMediaTypeWorkspaceContext
	extends UmbEditableWorkspaceContextBase<EntityType>
	implements UmbSaveableWorkspaceContextInterface
{
	//
	public readonly repository: UmbMediaTypeDetailRepository = new UmbMediaTypeDetailRepository(this);
	// Draft is located in structure manager

	#persistedData = new UmbObjectState<EntityType | undefined>(undefined);

	// General for content types:
	readonly data;
	readonly name;
	readonly alias;
	readonly description;
	readonly icon;

	readonly allowedAtRoot;
	readonly allowedContentTypes;
	readonly compositions;
	readonly collection;

	readonly structure = new UmbContentTypePropertyStructureManager<EntityType>(this, this.repository);

	#isSorting = new UmbBooleanState(undefined);
	isSorting = this.#isSorting.asObservable();

	constructor(host: UmbControllerHost) {
		super(host, 'Umb.Workspace.MediaType');

		// General for content types:
		this.data = this.structure.ownerContentType;
		this.name = this.structure.ownerContentTypeObservablePart((data) => data?.name);
		this.alias = this.structure.ownerContentTypeObservablePart((data) => data?.alias);
		this.description = this.structure.ownerContentTypeObservablePart((data) => data?.description);
		this.icon = this.structure.ownerContentTypeObservablePart((data) => data?.icon);
		this.allowedAtRoot = this.structure.ownerContentTypeObservablePart((data) => data?.allowedAtRoot);
		this.allowedContentTypes = this.structure.ownerContentTypeObservablePart((data) => data?.allowedContentTypes);
		this.compositions = this.structure.ownerContentTypeObservablePart((data) => data?.compositions);
		this.collection = this.structure.ownerContentTypeObservablePart((data) => data?.collection);
	}

	protected resetState() {
		this.#persistedData.setValue(undefined);
		super.resetState();
	}

	getIsSorting() {
		return this.#isSorting.getValue();
	}

	setIsSorting(isSorting: boolean) {
		this.#isSorting.setValue(isSorting);
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

	setAllowedContentTypes(allowedContentTypes: Array<UmbContentTypeSortModel>) {
		this.structure.updateOwnerContentType({ allowedContentTypes });
	}

	setCompositions(compositions: Array<UmbContentTypeCompositionModel>) {
		this.structure.updateOwnerContentType({ compositions });
	}

	setCollection(collection: UmbReferenceByUnique) {
		this.structure.updateOwnerContentType({ collection });
	}

	async create(parentId: string | null) {
		this.resetState();
		const { data } = await this.structure.createScaffold(parentId);
		if (!data) return undefined;

		this.setIsNew(true);
		this.setIsSorting(false);
		this.#persistedData.setValue(data);
		return data;
	}

	async load(entityId: string) {
		this.resetState();
		const { data } = await this.structure.loadType(entityId);
		if (!data) return undefined;

		this.setIsNew(false);
		this.setIsSorting(false);
		this.#persistedData.setValue(data);
		return data;
	}

	/**
	 * Save or creates the media type, based on wether its a new one or existing.
	 */
	async save() {
		const data = this.getData();

		if (!data) {
			return Promise.reject('Something went wrong, there is no data for media type you want to save...');
		}

		if (this.getIsNew()) {
			if ((await this.structure.create()) === true) {
				this.setIsNew(false);
			}
		} else {
			await this.structure.save();
		}

		this.setIsNew(false);
		this.workspaceComplete(data);
	}

	public destroy(): void {
		this.#persistedData.destroy();
		this.structure.destroy();
		this.#isSorting.destroy();
		this.repository.destroy();
		super.destroy();
	}
}

export const UMB_MEDIA_TYPE_WORKSPACE_CONTEXT = new UmbContextToken<
	UmbSaveableWorkspaceContextInterface,
	UmbMediaTypeWorkspaceContext
>(
	'UmbWorkspaceContext',
	undefined,
	(context): context is UmbMediaTypeWorkspaceContext => context.getEntityType?.() === UMB_MEDIA_TYPE_ENTITY_TYPE,
);
