import { UmbDocumentRepository } from '../repository/document.repository.js';
import { UmbDocumentTypeRepository } from '../../document-types/repository/document-type.repository.js';
import { UmbDocumentVariantContext } from '../variant-context/document-variant-context.js';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import { UmbContentTypePropertyStructureManager } from '@umbraco-cms/backoffice/content-type';
import {
	UmbSaveableWorkspaceContextInterface,
	UmbWorkspaceContext,
	UmbWorkspaceSplitViewManager,
	UmbVariantableWorkspaceContextInterface,
	type UmbVariantContext,
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

type EntityType = DocumentResponseModel;
export class UmbDocumentWorkspaceContext
	extends UmbWorkspaceContext<UmbDocumentRepository, EntityType>
	implements UmbVariantableWorkspaceContextInterface<EntityType | undefined>
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
	#getDataPromise?: Promise<any>;
	public isLoaded() {
		return this.#getDataPromise;
	}

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
		this.#getDataPromise = this.repository.requestById(entityId);
		const { data } = await this.#getDataPromise;
		if (!data) return undefined;

		this.setIsNew(false);
		//this.#persisted.next(data);
		this.#currentData.next(data);
		return data || undefined;
	}

	async create(documentTypeKey: string, parentId: string | null) {
		this.#getDataPromise = this.repository.createScaffold(documentTypeKey, { parentId });
		const { data } = await this.#getDataPromise;
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

	async propertyDataById(propertyId: string) {
		return this.structure.propertyStructureById(propertyId);
	}

	async propertyValueByAlias<PropertyValueType = unknown>(propertyAlias: string, variantId?: UmbVariantId) {
		return this.#currentData.asObservablePart(
			(data) =>
				data?.values?.find((x) => x?.alias === propertyAlias && (variantId ? variantId.compare(x) : true))?.value as PropertyValueType
		);
	}

	/**
	 * Get the current value of the property with the given alias and variantId.
	 * @param alias
	 * @param variantId
	 * @returns The value or undefined if not set or found.
	 */
	getPropertyValue<ReturnType = unknown>(alias: string, variantId?: UmbVariantId) {
		const currentData = this.#currentData.value;
		if (currentData) {
			const newDataSet = currentData.values?.find(
				(x) => x.alias === alias && (variantId ? variantId.compare(x) : true)
			);
			return newDataSet?.value as ReturnType;
		}
		return undefined;
	}
	async setPropertyValue<PropertyValueType = unknown>(alias: string, value: PropertyValueType, variantId?: UmbVariantId) {
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

	public createVariantVariantContext(host: UmbControllerHost, variantId: UmbVariantId) {
		return new UmbDocumentVariantContext(host, this, variantId);
	}

	public createDatasetContext(host: UmbControllerHost) {
		return new UmbDocumentVariantContext(host, this, UmbVariantId.Create({}));
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
