import { UmbMediaTypeDetailRepository } from '../repository/detail/media-type-detail.repository.js';
import { UMB_MEDIA_TYPE_ENTITY_TYPE } from '../index.js';
import {
	UmbSaveableWorkspaceContextInterface,
	UmbEditableWorkspaceContextBase,
} from '@umbraco-cms/backoffice/workspace';
import { UmbContentTypePropertyStructureManager } from '@umbraco-cms/backoffice/content-type';
import { type MediaTypeResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbBooleanState } from '@umbraco-cms/backoffice/observable-api';

type EntityType = MediaTypeResponseModel;
export class UmbMediaTypeWorkspaceContext
	extends UmbEditableWorkspaceContextBase<UmbMediaTypeDetailRepository, EntityType>
	implements UmbSaveableWorkspaceContextInterface<EntityType | undefined>
{
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

	readonly structure;

	#isSorting = new UmbBooleanState(undefined);
	isSorting = this.#isSorting.asObservable();

	constructor(host: UmbControllerHostElement) {
		super(host, 'Umb.Workspace.MediaType', new UmbMediaTypeDetailRepository(host));

		this.structure = new UmbContentTypePropertyStructureManager(this.host, this.repository);

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
		this.#isSorting.next(isSorting);
	}

	getData() {
		return this.structure.getOwnerContentType() || {};
	}

	getEntityId() {
		return this.getData().id;
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
		if (this.getIsNew()) {
			if ((await this.structure.create()) === true) {
				this.setIsNew(false);
			}
		} else {
			await this.structure.save();
		}

		this.saveComplete(this.getData());
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
