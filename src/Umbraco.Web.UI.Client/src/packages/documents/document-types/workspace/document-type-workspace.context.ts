import { UmbDocumentTypeRepository } from '../repository/document-type.repository.js';
import { UmbContentTypePropertyStructureManager } from '@umbraco-cms/backoffice/content-type';
import { UmbWorkspaceContext, UmbEntityWorkspaceContextInterface } from '@umbraco-cms/backoffice/workspace';
import type {
	ContentTypeCompositionModel,
	ContentTypeSortModel,
	DocumentTypeResponseModel,
} from '@umbraco-cms/backoffice/backend-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';

type EntityType = DocumentTypeResponseModel;
export class UmbDocumentTypeWorkspaceContext
	extends UmbWorkspaceContext<UmbDocumentTypeRepository, EntityType>
	implements UmbEntityWorkspaceContextInterface<EntityType | undefined>
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
		super(host, new UmbDocumentTypeRepository(host));

		this.structure = new UmbContentTypePropertyStructureManager(this.host, this.repository);

		// General for content types:
		this.data = this.structure.ownerDocumentType;
		this.name = this.structure.ownerDocumentTypeObservablePart((data) => data?.name);
		this.alias = this.structure.ownerDocumentTypeObservablePart((data) => data?.alias);
		this.description = this.structure.ownerDocumentTypeObservablePart((data) => data?.description);
		this.icon = this.structure.ownerDocumentTypeObservablePart((data) => data?.icon);
		this.allowedAsRoot = this.structure.ownerDocumentTypeObservablePart((data) => data?.allowedAsRoot);
		this.variesByCulture = this.structure.ownerDocumentTypeObservablePart((data) => data?.variesByCulture);
		this.variesBySegment = this.structure.ownerDocumentTypeObservablePart((data) => data?.variesBySegment);
		this.isElement = this.structure.ownerDocumentTypeObservablePart((data) => data?.isElement);
		this.allowedContentTypes = this.structure.ownerDocumentTypeObservablePart((data) => data?.allowedContentTypes);
		this.compositions = this.structure.ownerDocumentTypeObservablePart((data) => data?.compositions);

		// Document type specific:
		this.allowedTemplateIds = this.structure.ownerDocumentTypeObservablePart((data) => data?.allowedTemplateIds);
		this.defaultTemplateId = this.structure.ownerDocumentTypeObservablePart((data) => data?.defaultTemplateId);
		this.cleanup = this.structure.ownerDocumentTypeObservablePart((data) => data?.defaultTemplateId);
	}

	getData() {
		return this.structure.getOwnerDocumentType() || {};
	}

	getEntityId() {
		return this.getData().id;
	}

	getEntityType() {
		return 'document-type';
	}

	setName(name: string) {
		this.structure.updateOwnerDocumentType({ name });
	}
	setAlias(alias: string) {
		this.structure.updateOwnerDocumentType({ alias });
	}
	setDescription(description: string) {
		this.structure.updateOwnerDocumentType({ description });
	}

	// TODO: manage setting icon color alias?
	setIcon(icon: string) {
		this.structure.updateOwnerDocumentType({ icon });
	}

	setAllowedAsRoot(allowedAsRoot: boolean) {
		this.structure.updateOwnerDocumentType({ allowedAsRoot });
	}
	setVariesByCulture(variesByCulture: boolean) {
		this.structure.updateOwnerDocumentType({ variesByCulture });
	}
	setVariesBySegment(variesBySegment: boolean) {
		this.structure.updateOwnerDocumentType({ variesBySegment });
	}
	setIsElement(isElement: boolean) {
		this.structure.updateOwnerDocumentType({ isElement });
	}
	setAllowedContentTypes(allowedContentTypes: Array<ContentTypeSortModel>) {
		this.structure.updateOwnerDocumentType({ allowedContentTypes });
	}
	setCompositions(compositions: Array<ContentTypeCompositionModel>) {
		this.structure.updateOwnerDocumentType({ compositions });
	}

	// Document type specific:
	setAllowedTemplateIds(allowedTemplateIds: Array<string>) {
		this.structure.updateOwnerDocumentType({ allowedTemplateIds });
	}
	setDefaultTemplateId(defaultTemplateId: string) {
		this.structure.updateOwnerDocumentType({ defaultTemplateId });
	}

	async create(parentId: string | null) {

		const { data } = await this.structure.createScaffold(parentId);
		if (!data) return undefined;

		this.setIsNew(true);
		//this.#draft.next(data);
		return data || undefined;
		// TODO: Is this wrong? should we return { data }??
	}

	async load(entityId: string) {
		const { data } = await this.structure.loadType(entityId);
		if (!data) return undefined;

		this.setIsNew(false);
		//this.#draft.next(data);
		return data || undefined;
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
	}

	public destroy(): void {
		this.structure.destroy();
	}
}
