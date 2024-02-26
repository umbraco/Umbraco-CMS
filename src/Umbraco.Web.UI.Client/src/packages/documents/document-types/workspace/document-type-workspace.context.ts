import { UmbDocumentTypeDetailRepository } from '../repository/detail/document-type-detail.repository.js';
import type { UmbDocumentTypeDetailModel } from '../types.js';
import type { UmbContentTypeCompositionModel, UmbContentTypeSortModel } from '@umbraco-cms/backoffice/content-type';
import { UmbContentTypePropertyStructureManager } from '@umbraco-cms/backoffice/content-type';
import type { UmbSaveableWorkspaceContextInterface } from '@umbraco-cms/backoffice/workspace';
import { UmbEditableWorkspaceContextBase } from '@umbraco-cms/backoffice/workspace';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbBooleanState } from '@umbraco-cms/backoffice/observable-api';

type EntityType = UmbDocumentTypeDetailModel;
export class UmbDocumentTypeWorkspaceContext
	extends UmbEditableWorkspaceContextBase<EntityType>
	implements UmbSaveableWorkspaceContextInterface
{
	//
	readonly repository = new UmbDocumentTypeDetailRepository(this);
	// Data/Draft is located in structure manager

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
	readonly defaultTemplate;
	readonly cleanup;

	readonly structure = new UmbContentTypePropertyStructureManager<EntityType>(this, this.repository);

	#isSorting = new UmbBooleanState(undefined);
	isSorting = this.#isSorting.asObservable();

	constructor(host: UmbControllerHostElement) {
		super(host, 'Umb.Workspace.DocumentType');

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
		this.allowedTemplateIds = this.structure.ownerContentTypeObservablePart((data) => data?.allowedTemplates);
		this.defaultTemplate = this.structure.ownerContentTypeObservablePart((data) => data?.defaultTemplate);
		this.cleanup = this.structure.ownerContentTypeObservablePart((data) => data?.defaultTemplate);
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

	getEntityId() {
		return this.getData()?.unique;
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
	setAllowedContentTypes(allowedContentTypes: Array<UmbContentTypeSortModel>) {
		this.structure.updateOwnerContentType({ allowedContentTypes });
	}
	setCompositions(compositions: Array<UmbContentTypeCompositionModel>) {
		this.structure.updateOwnerContentType({ compositions });
	}

	// Document type specific:
	setAllowedTemplateIds(allowedTemplates: Array<{ id: string }>) {
		this.structure.updateOwnerContentType({ allowedTemplates });
	}
	setDefaultTemplate(defaultTemplate: { id: string }) {
		this.structure.updateOwnerContentType({ defaultTemplate });
	}

	async create(parentUnique: string | null) {
		const { data } = await this.structure.createScaffold(parentUnique);
		if (!data) return undefined;

		this.setIsNew(true);
		this.setIsSorting(false);
		//this.#draft.next(data);
		return { data } || undefined;
	}

	async load(unique: string) {
		const { data } = await this.structure.loadType(unique);
		if (!data) return undefined;

		this.setIsNew(false);
		this.setIsSorting(false);
		//this.#draft.next(data);
		return { data } || undefined;
	}

	/**
	 * Save or creates the document type, based on wether its a new one or existing.
	 */
	async save() {
		const data = this.getData();
		if (data === undefined) throw new Error('Cannot save, no data');

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
