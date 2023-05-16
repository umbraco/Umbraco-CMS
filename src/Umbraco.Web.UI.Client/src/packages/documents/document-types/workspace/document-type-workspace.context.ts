import { UmbContentTypePropertyStructureManager } from '@umbraco-cms/backoffice/content-type';
import { UmbDocumentTypeRepository } from '../repository/document-type.repository';
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
		this.data = this.structure.rootDocumentType;
		this.name = this.structure.rootDocumentTypeObservablePart((data) => data?.name);
		this.alias = this.structure.rootDocumentTypeObservablePart((data) => data?.alias);
		this.description = this.structure.rootDocumentTypeObservablePart((data) => data?.description);
		this.icon = this.structure.rootDocumentTypeObservablePart((data) => data?.icon);
		this.allowedAsRoot = this.structure.rootDocumentTypeObservablePart((data) => data?.allowedAsRoot);
		this.variesByCulture = this.structure.rootDocumentTypeObservablePart((data) => data?.variesByCulture);
		this.variesBySegment = this.structure.rootDocumentTypeObservablePart((data) => data?.variesBySegment);
		this.isElement = this.structure.rootDocumentTypeObservablePart((data) => data?.isElement);
		this.allowedContentTypes = this.structure.rootDocumentTypeObservablePart((data) => data?.allowedContentTypes);
		this.compositions = this.structure.rootDocumentTypeObservablePart((data) => data?.compositions);

		// Document type specific:
		this.allowedTemplateIds = this.structure.rootDocumentTypeObservablePart((data) => data?.allowedTemplateIds);
		this.defaultTemplateId = this.structure.rootDocumentTypeObservablePart((data) => data?.defaultTemplateId);
		this.cleanup = this.structure.rootDocumentTypeObservablePart((data) => data?.defaultTemplateId);
	}

	getData() {
		return this.structure.getRootDocumentType() || {};
	}

	getEntityId() {
		return this.getData().id;
	}

	getEntityType() {
		return 'document-type';
	}

	setName(name: string) {
		this.structure.updateRootDocumentType({ name });
	}
	setAlias(alias: string) {
		this.structure.updateRootDocumentType({ alias });
	}
	setDescription(description: string) {
		this.structure.updateRootDocumentType({ description });
	}

	// TODO: manage setting icon color alias?
	setIcon(icon: string) {
		this.structure.updateRootDocumentType({ icon });
	}

	setAllowedAsRoot(allowedAsRoot: boolean) {
		this.structure.updateRootDocumentType({ allowedAsRoot });
	}
	setVariesByCulture(variesByCulture: boolean) {
		this.structure.updateRootDocumentType({ variesByCulture });
	}
	setVariesBySegment(variesBySegment: boolean) {
		this.structure.updateRootDocumentType({ variesBySegment });
	}
	setIsElement(isElement: boolean) {
		this.structure.updateRootDocumentType({ isElement });
	}
	setAllowedContentTypes(allowedContentTypes: Array<ContentTypeSortModel>) {
		this.structure.updateRootDocumentType({ allowedContentTypes });
	}
	setCompositions(compositions: Array<ContentTypeCompositionModel>) {
		this.structure.updateRootDocumentType({ compositions });
	}

	// Document type specific:
	setAllowedTemplateIds(allowedTemplateIds: Array<string>) {
		this.structure.updateRootDocumentType({ allowedTemplateIds });
	}
	setDefaultTemplateId(defaultTemplateId: string) {
		this.structure.updateRootDocumentType({ defaultTemplateId });
	}

	async createScaffold(parentId: string) {
		const { data } = await this.structure.createScaffold(parentId);
		if (!data) return undefined;

		this.setIsNew(true);
		//this.#draft.next(data);
		return data || undefined;
	}

	async load(entityId: string) {
		const { data } = await this.structure.loadType(entityId);
		if (!data) return undefined;

		this.setIsNew(false);
		//this.#draft.next(data);
		return data || undefined;
	}

	async save() {
		const id = this.getEntityId();
		if (!id) throw new Error('Cannot save entity without id');
		this.repository.save(id, this.getData());
	}

	public destroy(): void {
		this.structure.destroy();
	}
}
