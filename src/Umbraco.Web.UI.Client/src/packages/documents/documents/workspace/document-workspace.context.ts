import { UmbDocumentRepository } from '../repository/document.repository.js';
import { UmbDocumentTypeRepository } from '../../document-types/repository/document-type.repository.js';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import { UmbContentTypePropertyStructureManager } from '@umbraco-cms/backoffice/content-type';
import {
	UmbWorkspaceContext,
	UmbWorkspaceSplitViewManager,
	UmbWorkspaceVariableEntityContextInterface,
} from '@umbraco-cms/backoffice/workspace';
import type { CreateDocumentRequestModel, DocumentResponseModel } from '@umbraco-cms/backoffice/backend-api';
import {
	partialUpdateFrozenArray,
	UmbObjectState,
	UmbObserverController,
} from '@umbraco-cms/backoffice/observable-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';

// TODO: should this context be called DocumentDraft instead of workspace? or should the draft be part of this?
// TODO: Should we have a DocumentStructureContext and maybe even a DocumentDraftContext?

type EntityType = DocumentResponseModel;
export class UmbDocumentWorkspaceContext
	extends UmbWorkspaceContext<UmbDocumentRepository, EntityType>
	implements UmbWorkspaceVariableEntityContextInterface<EntityType | undefined>
{
	/**
	 * The document is the current stored version of the document.
	 * For now lets not share this publicly as it can become confusing.
	 * TODO: Use this to compare, for variants with changes.
	 */
	#document = new UmbObjectState<EntityType | undefined>(undefined);

	/**
	 * The document is the current state/draft version of the document.
	 */
	#draft = new UmbObjectState<EntityType | undefined>(undefined);
	readonly unique = this.#draft.getObservablePart((data) => data?.id);
	readonly documentTypeKey = this.#draft.getObservablePart((data) => data?.contentTypeId);

	readonly variants = this.#draft.getObservablePart((data) => data?.variants || []);
	readonly urls = this.#draft.getObservablePart((data) => data?.urls || []);
	readonly templateId = this.#draft.getObservablePart((data) => data?.templateId || null);

	readonly structure;
	readonly splitView;

	constructor(host: UmbControllerHostElement) {
		super(host, new UmbDocumentRepository(host));

		this.structure = new UmbContentTypePropertyStructureManager(this.host, new UmbDocumentTypeRepository(this.host));
		this.splitView = new UmbWorkspaceSplitViewManager(this.host);

		new UmbObserverController(this.host, this.documentTypeKey, (id) => this.structure.loadType(id));

		/*
		TODO: Concept for ensure variant values:
		new UmbObserverController(this.host, this.variants, (variants) => {
			if (!variants) return;
			const draft = this.#draft.getValue();
			if (!draft) return;

			// Gather all properties from all document types.
			// Loop through all properties for each variant and insert missing value objects.
		}
		*/
	}

	async load(entityId: string) {
		const { data } = await this.repository.requestById(entityId);
		if (!data) return undefined;

		this.setIsNew(false);
		this.#document.next(data);
		this.#draft.next(data);
		return data || undefined;
	}

	async create(documentTypeKey: string, parentId: string | null) {
		const { data } = await this.repository.createScaffold(documentTypeKey, {parentId});
		if (!data) return undefined;

		this.setIsNew(true);
		this.#document.next(data);
		this.#draft.next(data);
		return data || undefined;
	}

	getData() {
		return this.#draft.getValue() || {};
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

	getVariant(variantId: UmbVariantId) {
		return this.#draft.getValue()?.variants?.find((x) => variantId.compare(x));
	}

	getName(variantId?: UmbVariantId) {
		const variants = this.#draft.getValue()?.variants;
		if (!variants) return;
		if (variantId) {
			return variants.find((x) => variantId.compare(x))?.name;
		} else {
			return variants[0]?.name;
		}
	}

	setName(name: string, variantId?: UmbVariantId) {
		console.log("setname", name, variantId)
		const oldVariants = this.#draft.getValue()?.variants || [];
		const variants = partialUpdateFrozenArray(
			oldVariants,
			{ name },
			variantId ? (x) => variantId.compare(x) : () => true
		);
		console.log(variants);
		this.#draft.update({ variants });
	}

	propertyValuesOf(variantId?: UmbVariantId) {
		return this.#draft.getObservablePart((data) =>
			variantId ? data?.values?.filter((x) => variantId.compare(x)) : data?.values
		);
	}

	propertyDataByAlias(propertyAlias: string, variantId?: UmbVariantId) {
		return this.#draft.getObservablePart((data) =>
			data?.values?.find((x) => x?.alias === propertyAlias && (variantId ? variantId.compare(x) : true))
		);
	}
	propertyValueByAlias(propertyAlias: string, variantId?: UmbVariantId) {
		return this.#draft.getObservablePart(
			(data) =>
				data?.values?.find((x) => x?.alias === propertyAlias && (variantId ? variantId.compare(x) : true))?.value
		);
	}

	getPropertyValue(alias: string, variantId?: UmbVariantId): void {
		const currentData = this.#draft.value;
		if (currentData) {
			const newDataSet = currentData.values?.find(
				(x) => x.alias === alias && (variantId ? variantId.compare(x) : true)
			);
			return newDataSet?.value;
		}
	}
	setPropertyValue(alias: string, value: unknown, variantId?: UmbVariantId) {
		const partialEntry = { value };
		const currentData = this.#draft.value;
		if (currentData) {
			const values = partialUpdateFrozenArray(
				currentData.values || [],
				partialEntry,
				(x) => x.alias === alias && (variantId ? variantId.compare(x) : true)
			);
			this.#draft.update({ values });
		}
	}

	async save() {
		if (!this.#draft.value) return;
		if (!this.#draft.value.id) return;

		if (this.getIsNew()) {
			// TODO: typescript hack until we get the create type
			const value = this.#draft.value as CreateDocumentRequestModel & { id: string };
			await this.repository.create(value);
		} else {
			await this.repository.save(this.#draft.value.id, this.#draft.value);
		}
		// If it went well, then its not new anymore?.
		this.setIsNew(false);
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

	public destroy(): void {
		this.#draft.complete();
		this.structure.destroy();
	}
}

export default UmbDocumentWorkspaceContext;
