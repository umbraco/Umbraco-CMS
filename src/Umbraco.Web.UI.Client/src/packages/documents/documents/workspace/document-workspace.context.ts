import { UmbDocumentRepository } from '../repository/document.repository.js';
import { UmbDocumentTypeRepository } from '../../document-types/repository/document-type.repository.js';
import { UmbDocumentDatasetContext } from '../dataset-context/document-dataset-context.js';
import { type UmbVariantId } from '@umbraco-cms/backoffice/variant';
import { UmbContentTypePropertyStructureManager } from '@umbraco-cms/backoffice/content-type';
import {
	UmbSaveableWorkspaceContextInterface,
	UmbWorkspaceContext,
	UmbWorkspaceSplitViewManager,
	UmbVariableWorkspaceContextInterface,
	type UmbDatasetContext,
} from '@umbraco-cms/backoffice/workspace';
import type { CreateDocumentRequestModel, DocumentResponseModel } from '@umbraco-cms/backoffice/backend-api';
import {
	appendToFrozenArray,
	partialUpdateFrozenArray,
	UmbObjectState,
	UmbObserverController,
} from '@umbraco-cms/backoffice/observable-api';
import { UmbControllerHost, UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { Observable } from '@umbraco-cms/backoffice/external/rxjs';

// TODO: should this context be called DocumentDraft instead of workspace? or should the draft be part of this?
// TODO: Should we have a DocumentStructureContext and maybe even a DocumentDraftContext?

type EntityType = DocumentResponseModel;
export class UmbDocumentWorkspaceContext
	extends UmbWorkspaceContext<UmbDocumentRepository, EntityType>
	implements UmbVariableWorkspaceContextInterface<EntityType | undefined>
{
	/**
	 * The document is the current stored version of the document.
	 * For now lets not share this publicly as it can become confusing.
	 * TODO: This concept is to be able to compare if there is changes since the saved one.
	 */
	//#persistedData = new UmbObjectState<EntityType | undefined>(undefined);

	/**
	 * The document is the current state/draft version of the document.
	 */
	#currentData = new UmbObjectState<EntityType | undefined>(undefined);
	readonly unique = this.#currentData.asObservablePart((data) => data?.id);
	readonly documentTypeKey = this.#currentData.asObservablePart((data) => data?.contentTypeId);

	readonly variants = this.#currentData.asObservablePart((data) => data?.variants || []);
	readonly urls = this.#currentData.asObservablePart((data) => data?.urls || []);
	readonly templateId = this.#currentData.asObservablePart((data) => data?.templateId || null);

	readonly structure;
	readonly splitView;

	constructor(host: UmbControllerHostElement) {
		super(host, 'Umb.Workspace.Document', new UmbDocumentRepository(host));

		this.structure = new UmbContentTypePropertyStructureManager(this.host, new UmbDocumentTypeRepository(this.host));
		this.splitView = new UmbWorkspaceSplitViewManager(this.host);

		new UmbObserverController(this.host, this.documentTypeKey, (id) => this.structure.loadType(id));

		/*
		TODO: Make something to ensure all variants are present in data? Seems like a good idea?.
		*/
	}

	async load(entityId: string) {
		const { data } = await this.repository.requestById(entityId);
		if (!data) return undefined;

		this.setIsNew(false);
		//this.#persisted.next(data);
		this.#currentData.next(data);
		return data || undefined;
	}

	async create(documentTypeKey: string, parentId: string | null) {
		const { data } = await this.repository.createScaffold(documentTypeKey, { parentId });
		if (!data) return undefined;

		this.setIsNew(true);
		this.#currentData.next(data);
		return data || undefined;
	}

	getData() {
		return this.#currentData.getValue() || {};
	}

	/*
	getUnique() {
		return this.#data.getKey();
	}
	*/

	getEntityId() {
		return this.getData().id;
	}

	getEntityType() {
		return 'document';
	}

	getContentTypeId() {
		return this.getData().contentTypeId;
	}

	variantById(variantId: UmbVariantId) {
		return this.#currentData.asObservablePart((data) => data?.variants?.find((x) => variantId.compare(x)));
	}

	getVariant(variantId: UmbVariantId) {
		return this.#currentData.getValue()?.variants?.find((x) => variantId.compare(x));
	}

	getName(variantId?: UmbVariantId) {
		const variants = this.#currentData.getValue()?.variants;
		if (!variants) return;
		if (variantId) {
			return variants.find((x) => variantId.compare(x))?.name;
		} else {
			return variants[0]?.name;
		}
	}

	setName(name: string, variantId?: UmbVariantId) {
		const oldVariants = this.#currentData.getValue()?.variants || [];
		const variants = partialUpdateFrozenArray(
			oldVariants,
			{ name },
			variantId ? (x) => variantId.compare(x) : () => true
		);
		this.#currentData.update({ variants });
	}

	propertyDataById(propertyId: string) {
		return this.structure.propertyStructureById(propertyId);
	}

	propertyValueByAlias<PropertyValueType = unknown>(propertyAlias: string, variantId?: UmbVariantId): Observable<PropertyValueType> {
		return this.#currentData.asObservablePart(
			(data) =>
				data?.values?.find((x) => x?.alias === propertyAlias && (variantId ? variantId.compare(x) : true))?.value
		);
	}

	getPropertyValue<PropertyValueType = unknown>(alias: string, variantId?: UmbVariantId): PropertyValueType | undefined {
		const currentData = this.#currentData.value;
		if (currentData) {
			const newDataSet = currentData.values?.find(
				(x) => x.alias === alias && (variantId ? variantId.compare(x) : true)
			);
			return newDataSet?.value;
		}
		return undefined;
	}
	setPropertyValue<PropertyValueType = unknown>(alias: string, value: PropertyValueType, variantId?: UmbVariantId) {
		const entry = { ...variantId?.toObject(), alias, value };
		const currentData = this.#currentData.value;
		if (currentData) {
			const values = appendToFrozenArray(
				currentData.values || [],
				entry,
				(x) => x.alias === alias && (variantId ? variantId.compare(x) : true)
			);
			this.#currentData.update({ values });
		}
	}

	async save() {
		if (!this.#currentData.value) return;
		if (!this.#currentData.value.id) return;

		if (this.getIsNew()) {
			// TODO: typescript hack until we get the create type
			const value = this.#currentData.value as CreateDocumentRequestModel & { id: string };
			if ((await this.repository.create(value)).data !== undefined) {
				this.setIsNew(false);
			}
		} else {
			await this.repository.save(this.#currentData.value.id, this.#currentData.value);
		}

		this.saveComplete(this.getData());
	}

	async delete(id: string) {
		await this.repository.delete(id);
	}

	/*
	concept notes:

	public saveAndPublish() {

	}

	public saveAndPreview() {

	}
	*/

	public createVariableDatasetContext(host: UmbControllerHost, variantId: UmbVariantId): UmbDatasetContext {
		return new UmbDocumentDatasetContext(host, this, variantId);
	}

	public destroy(): void {
		this.#currentData.complete();
		this.structure.destroy();
		super.destroy();
	}
}

export default UmbDocumentWorkspaceContext;


export const UMB_DOCUMENT_WORKSPACE_CONTEXT = new UmbContextToken<UmbSaveableWorkspaceContextInterface, UmbDocumentWorkspaceContext>(
	'UmbWorkspaceContext',
	// TODO: Refactor: make a better generic way to identify workspaces, maybe workspaceType or workspaceAlias?.
	(context): context is UmbDocumentWorkspaceContext => context.getEntityType?.() === 'document'
);
