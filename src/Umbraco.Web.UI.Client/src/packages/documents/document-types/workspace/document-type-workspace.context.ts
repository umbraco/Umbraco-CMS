import { UmbDocumentTypeRepository } from '../repository/document-type.repository.js';
import { UmbContentTypePropertyStructureManager } from '@umbraco-cms/backoffice/content-type';
import { UmbWorkspaceContext, UmbSaveableWorkspaceContextInterface } from '@umbraco-cms/backoffice/workspace';
import type {
	ContentTypeCompositionModel,
	ContentTypeSortModel,
	DocumentTypeResponseModel,
} from '@umbraco-cms/backoffice/backend-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

type EntityType = DocumentTypeResponseModel;
export class UmbDocumentTypeWorkspaceContext
	extends UmbWorkspaceContext<UmbDocumentTypeRepository, EntityType>
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
	readonly variesByCulture;
	readonly variesBySegment;
	readonly isElement;
	readonly allowedContentTypes;
	readonly compositions;

	// Document type specific:
	readonly allowedTemplateIds;
	readonly defaultTemplateId;
	readonly cleanup;

	readonly structure;

	constructor(host: UmbControllerHostElement) {
		super(host, 'Umb.Workspace.DocumentType', new UmbDocumentTypeRepository(host));

		this.structure = new UmbContentTypePropertyStructureManager(this.host, this.repository);

		// General for content types:
		this.data = this.structure.ownerContentType;
		this.name = this.structure.ownerContentTypeObservablePart((data) => data?.name);
		this.alias = this.structure.ownerContentTypeObservablePart((data) => data?.alias);
		this.description = this.structure.ownerContentTypeObservablePart((data) => data?.description);
		this.icon = this.structure.ownerContentTypeObservablePart((data) => data?.icon);
		this.allowedAsRoot = this.structure.ownerContentTypeObservablePart((data) => data?.allowedAsRoot);
		this.variesByCulture = this.structure.ownerContentTypeObservablePart((data) => data?.variesByCulture);
		this.variesBySegment = this.structure.ownerContentTypeObservablePart((data) => data?.variesBySegment);
		this.isElement = this.structure.ownerContentTypeObservablePart((data) => data?.isElement);
		this.allowedContentTypes = this.structure.ownerContentTypeObservablePart((data) => data?.allowedContentTypes);
		this.compositions = this.structure.ownerContentTypeObservablePart((data) => data?.compositions);

		// Document type specific:
		this.allowedTemplateIds = this.structure.ownerContentTypeObservablePart((data) => data?.allowedTemplateIds);
		this.defaultTemplateId = this.structure.ownerContentTypeObservablePart((data) => data?.defaultTemplateId);
		this.cleanup = this.structure.ownerContentTypeObservablePart((data) => data?.defaultTemplateId);
	}

	getData() {
		return this.structure.getOwnerContentType() || {};
	}

	getEntityId() {
		return this.getData().id;
	}

	getEntityType() {
		return 'document-type';
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

	setAllowedAsRoot(allowedAsRoot: boolean) {
		this.structure.updateOwnerContentType({ allowedAsRoot });
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
	setAllowedContentTypes(allowedContentTypes: Array<ContentTypeSortModel>) {
		this.structure.updateOwnerContentType({ allowedContentTypes });
	}
	setCompositions(compositions: Array<ContentTypeCompositionModel>) {
		this.structure.updateOwnerContentType({ compositions });
	}

	// Document type specific:
	setAllowedTemplateIds(allowedTemplateIds: Array<string>) {
		this.structure.updateOwnerContentType({ allowedTemplateIds });
	}
	setDefaultTemplateId(defaultTemplateId: string) {
		this.structure.updateOwnerContentType({ defaultTemplateId });
	}

	async create(parentId: string | null) {
		const { data } = await this.structure.createScaffold(parentId);
		if (!data) return undefined;

		this.setIsNew(true);
		//this.#draft.next(data);
		return { data } || undefined;
		// TODO: Is this wrong? should we return { data }??
	}

	async load(entityId: string) {
		const { data } = await this.structure.loadType(entityId);
		if (!data) return undefined;

		this.setIsNew(false);
		//this.#draft.next(data);
		return { data } || undefined;
		// TODO: Is this wrong? should we return { data }??
	}

	/**
	 * Save or creates the document type, based on wether its a new one or existing.
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

export const UMB_DOCUMENT_TYPE_WORKSPACE_CONTEXT = new UmbContextToken<
	UmbSaveableWorkspaceContextInterface,
	UmbDocumentTypeWorkspaceContext
>(
	'UmbWorkspaceContext',
	(context): context is UmbDocumentTypeWorkspaceContext => context.getEntityType?.() === 'document-type',
);
