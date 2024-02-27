import { UmbMediaTypeDetailRepository } from '../repository/detail/media-type-detail.repository.js';
import type { UmbMediaTypeDetailModel } from '../types.js';
import { UMB_MEDIA_TYPE_ENTITY_TYPE } from '../entity.js';
import type { UmbSaveableWorkspaceContextInterface } from '@umbraco-cms/backoffice/workspace';
import { UmbEditableWorkspaceContextBase } from '@umbraco-cms/backoffice/workspace';
import { UmbContentTypePropertyStructureManager } from '@umbraco-cms/backoffice/content-type';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbBooleanState } from '@umbraco-cms/backoffice/observable-api';

type EntityType = UmbMediaTypeDetailModel;
export class UmbMediaTypeWorkspaceContext
	extends UmbEditableWorkspaceContextBase<EntityType>
	implements UmbSaveableWorkspaceContextInterface
{
	//
	public readonly repository: UmbMediaTypeDetailRepository = new UmbMediaTypeDetailRepository(this);
	// Draft is located in structure manager

	// General for content types:
	readonly data;
	readonly name;
	readonly alias;
	readonly description;
	readonly icon;

	readonly allowedAsRoot;
	readonly allowedContentTypes;
	readonly compositions;

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
		this.allowedAsRoot = this.structure.ownerContentTypeObservablePart((data) => data?.allowedAsRoot);
		this.allowedContentTypes = this.structure.ownerContentTypeObservablePart((data) => data?.allowedContentTypes);
		this.compositions = this.structure.ownerContentTypeObservablePart((data) => data?.compositions);
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

	updateProperty<PropertyName extends keyof EntityType>(propertyName: PropertyName, value: EntityType[PropertyName]) {
		this.structure.updateOwnerContentType({ [propertyName]: value });
	}

	async create(parentId: string | null) {
		const { data } = await this.structure.createScaffold(parentId);
		if (!data) return undefined;

		this.setIsNew(true);
		this.setIsSorting(false);
		//this.#draft.next(data);
		return { data } || undefined;
		// TODO: Is this wrong? should we return { data }??
	}

	async load(entityId: string) {
		const { data } = await this.structure.loadType(entityId);
		if (!data) return undefined;

		this.setIsNew(false);
		this.setIsSorting(false);
		//this.#draft.next(data);
		return { data } || undefined;
		// TODO: Is this wrong? should we return { data }??
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

		this.saveComplete(data);
	}

	public destroy(): void {
		this.structure.destroy();
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
