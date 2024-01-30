import { UmbDocumentTypeDetailRepository } from '../../document-types/repository/detail/document-type-detail.repository.js';
import { UmbDocumentPropertyDataContext } from '../property-dataset-context/document-property-dataset-context.js';
import { UMB_DOCUMENT_ENTITY_TYPE } from '../entity.js';
import { UmbDocumentDetailRepository } from '../repository/index.js';
import type { UmbDocumentDetailModel } from '../types.js';
import { UmbDocumentPublishingRepository } from '../repository/publishing/index.js';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import { UmbContentTypePropertyStructureManager } from '@umbraco-cms/backoffice/content-type';
import {
	UmbEditableWorkspaceContextBase,
	UmbWorkspaceSplitViewManager,
	type UmbVariantableWorkspaceContextInterface,
	type UmbPublishableWorkspaceContextInterface,
} from '@umbraco-cms/backoffice/workspace';
import { appendToFrozenArray, partialUpdateFrozenArray, UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

type EntityType = UmbDocumentDetailModel;
export class UmbDocumentWorkspaceContext
	extends UmbEditableWorkspaceContextBase<EntityType>
	implements UmbVariantableWorkspaceContextInterface, UmbPublishableWorkspaceContextInterface
{
	//
	public readonly repository = new UmbDocumentDetailRepository(this);
	public readonly publishingRepository = new UmbDocumentPublishingRepository(this);

	/**
	 * The document is the current state/draft version of the document.
	 */
	#currentData = new UmbObjectState<EntityType | undefined>(undefined);
	#getDataPromise?: Promise<any>;
	public isLoaded() {
		return this.#getDataPromise;
	}

	readonly unique = this.#currentData.asObservablePart((data) => data?.unique);
	readonly contentTypeUnique = this.#currentData.asObservablePart((data) => data?.documentType.unique);

	readonly variants = this.#currentData.asObservablePart((data) => data?.variants || []);
	readonly urls = this.#currentData.asObservablePart((data) => data?.urls || []);
	readonly templateId = this.#currentData.asObservablePart((data) => data?.template?.id || null);

	readonly structure = new UmbContentTypePropertyStructureManager(this, new UmbDocumentTypeDetailRepository(this));
	readonly splitView = new UmbWorkspaceSplitViewManager();

	constructor(host: UmbControllerHost) {
		// TODO: Get Workspace Alias via Manifest.
		super(host, 'Umb.Workspace.Document');

		this.observe(this.contentTypeUnique, (unique) => this.structure.loadType(unique));

		/*
		TODO: Make something to ensure all variants are present in data? Seems like a good idea?.
		*/
	}

	async load(unique: string) {
		this.#getDataPromise = this.repository.requestByUnique(unique);
		const { data } = await this.#getDataPromise;
		if (!data) return undefined;

		this.setIsNew(false);
		//this.#persisted.next(data);
		this.#currentData.setValue(data);
		return data || undefined;
	}

	async create(parentUnique: string | null, documentTypeUnique: string) {
		this.#getDataPromise = this.repository.createScaffold(parentUnique, {
			documentType: { unique: documentTypeUnique },
		});
		const { data } = await this.#getDataPromise;
		if (!data) return undefined;

		this.setIsNew(true);
		this.#currentData.setValue(data);
		return data || undefined;
	}

	getData() {
		return this.#currentData.getValue();
	}

	getEntityId() {
		return this.getData()?.unique;
	}

	getEntityType() {
		return UMB_DOCUMENT_ENTITY_TYPE;
	}

	getContentTypeId() {
		return this.getData()?.documentType.unique;
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
			variantId ? (x) => variantId.compare(x) : () => true,
		);
		this.#currentData.update({ variants });
	}

	async propertyStructureById(propertyId: string) {
		return this.structure.propertyStructureById(propertyId);
	}

	async propertyValueByAlias<PropertyValueType = unknown>(propertyAlias: string, variantId?: UmbVariantId) {
		return this.#currentData.asObservablePart(
			(data) =>
				data?.values?.find((x) => x?.alias === propertyAlias && (variantId ? variantId.compare(x) : true))
					?.value as PropertyValueType,
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
				(x) => x.alias === alias && (variantId ? variantId.compare(x) : true),
			);
			return newDataSet?.value as ReturnType;
		}
		return undefined;
	}
	async setPropertyValue<PropertyValueType = unknown>(
		alias: string,
		value: PropertyValueType,
		variantId?: UmbVariantId,
	) {
		const entry = { ...variantId?.toObject(), alias, value };
		const currentData = this.#currentData.value;
		if (currentData) {
			const values = appendToFrozenArray(
				currentData.values || [],
				entry,
				(x) => x.alias === alias && (variantId ? variantId.compare(x) : true),
			);
			this.#currentData.update({ values });
		}
	}

	async #createOrSave() {
		if (!this.#currentData.value?.unique) throw new Error('Unique is missing');

		if (this.getIsNew()) {
			const value = this.#currentData.value;

			if ((await this.repository.create(value)).data !== undefined) {
				this.setIsNew(false);
			}
		} else {
			await this.repository.save(this.#currentData.value);
		}
	}

	async save() {
		const data = this.getData();
		if (!data) throw new Error('Data is missing');
		await this.#createOrSave();
		this.saveComplete(data);
	}

	async delete() {
		const id = this.getEntityId();
		if (id) {
			await this.repository.delete(id);
		}
	}

	public async saveAndPublish() {
		await this.#createOrSave();
		// TODO: This might be right to publish all, but we need a method that just saves and publishes a declared range of variants.
		const currentData = this.#currentData.value;
		if (currentData) {
			const variantIds = currentData.variants?.map((x) => UmbVariantId.Create(x));
			const unique = currentData.unique;
			if (variantIds && unique) {
				await this.publishingRepository.publish(unique, variantIds);
			}
		}
	}

	public async publish() {
		// TODO: This might be right to publish all, but we need a method that just publishes a declared range of variants.
		const currentData = this.#currentData.value;
		if (currentData) {
			const variantIds = currentData.variants?.map((x) => UmbVariantId.Create(x));
			const unique = this.getEntityId();
			if (variantIds && unique) {
				await this.publishingRepository.publish(unique, variantIds);
			}
		}
	}

	public async unpublish() {
		// TODO: This might be right to unpublish all, but we need a method that just publishes a declared range of variants.
		const currentData = this.#currentData.value;
		if (currentData) {
			const variantIds = currentData.variants?.map((x) => UmbVariantId.Create(x));
			const unique = this.getEntityId();
			if (variantIds && unique) {
				await this.publishingRepository.unpublish(unique, variantIds);
			}
		}
	}

	/*
	concept notes:

	public saveAndPreview() {

	}
	*/

	public createPropertyDatasetContext(host: UmbControllerHost, variantId: UmbVariantId) {
		return new UmbDocumentPropertyDataContext(host, this, variantId);
	}

	public destroy(): void {
		this.#currentData.destroy();
		this.structure.destroy();
		super.destroy();
	}
}

export default UmbDocumentWorkspaceContext;
