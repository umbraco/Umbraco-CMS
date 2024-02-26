import { UmbDocumentTypeDetailRepository } from '../../document-types/repository/detail/document-type-detail.repository.js';
import { UmbDocumentPropertyDataContext } from '../property-dataset-context/document-property-dataset-context.js';
import { UMB_DOCUMENT_ENTITY_TYPE } from '../entity.js';
import { UmbDocumentDetailRepository } from '../repository/index.js';
import type { UmbDocumentDetailModel, UmbDocumentVariantModel, UmbDocumentVariantOptionModel } from '../types.js';
import { UMB_DOCUMENT_LANGUAGE_PICKER_MODAL, type UmbDocumentVariantPickerModalData } from '../modals/index.js';
import { UmbDocumentPublishingRepository } from '../repository/publishing/index.js';
import { UMB_DOCUMENT_WORKSPACE_ALIAS } from './manifests.js';
import {
	type UmbObjectWithVariantProperties,
	UmbVariantId,
	variantPropertiesObjectToString,
} from '@umbraco-cms/backoffice/variant';
import { UmbContentTypePropertyStructureManager } from '@umbraco-cms/backoffice/content-type';
import {
	UmbEditableWorkspaceContextBase,
	UmbWorkspaceSplitViewManager,
	type UmbVariantableWorkspaceContextInterface,
	type UmbPublishableWorkspaceContextInterface,
} from '@umbraco-cms/backoffice/workspace';
import {
	appendToFrozenArray,
	combineObservables,
	partialUpdateFrozenArray,
	UmbArrayState,
	UmbObjectState,
} from '@umbraco-cms/backoffice/observable-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbLanguageCollectionRepository, type UmbLanguageDetailModel } from '@umbraco-cms/backoffice/language';
import { firstValueFrom } from '@umbraco-cms/backoffice/external/rxjs';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';

type EntityType = UmbDocumentDetailModel;
export class UmbDocumentWorkspaceContext
	extends UmbEditableWorkspaceContextBase<EntityType>
	implements UmbVariantableWorkspaceContextInterface<UmbDocumentVariantModel>, UmbPublishableWorkspaceContextInterface
{
	//
	public readonly repository = new UmbDocumentDetailRepository(this);
	public readonly publishingRepository = new UmbDocumentPublishingRepository(this);

	/**
	 * The document is the current state/draft version of the document.
	 */
	#currentData = new UmbObjectState<EntityType | undefined>(undefined);
	#getDataPromise?: Promise<any>;
	// TODo: Optimize this so it uses either a App Language Context? [NL]
	#languageRepository = new UmbLanguageCollectionRepository(this);
	#languages = new UmbArrayState<UmbLanguageDetailModel>([], (x) => x.unique);
	public readonly languages = this.#languages.asObservable();

	public isLoaded() {
		return this.#getDataPromise;
	}

	readonly unique = this.#currentData.asObservablePart((data) => data?.unique);

	readonly contentTypeUnique = this.#currentData.asObservablePart((data) => data?.documentType.unique);
	readonly contentTypeHasCollection = this.#currentData.asObservablePart((data) => !!data?.documentType.collection);
	readonly variants = this.#currentData.asObservablePart((data) => data?.variants ?? []);
	readonly variantOptions = combineObservables([this.variants, this.languages], ([variants, languages]) => {
		return languages.map((language) => {
			return {
				variant: variants.find((x) => x.culture === language.unique),
				language,
				// TODO: When including segments, this should be updated to include the segment as well. [NL]
				unique: language.unique, // This must be a variantId string!
			} as UmbDocumentVariantOptionModel;
		});
	});

	readonly changedVariants = new UmbArrayState<UmbObjectWithVariantProperties>([], variantPropertiesObjectToString);
	readonly urls = this.#currentData.asObservablePart((data) => data?.urls || []);
	readonly templateId = this.#currentData.asObservablePart((data) => data?.template?.unique || null);

	readonly structure = new UmbContentTypePropertyStructureManager(this, new UmbDocumentTypeDetailRepository(this));
	readonly splitView = new UmbWorkspaceSplitViewManager();

	constructor(host: UmbControllerHost) {
		super(host, UMB_DOCUMENT_WORKSPACE_ALIAS);

		this.observe(this.contentTypeUnique, (unique) => this.structure.loadType(unique));

		this.loadLanguages();
	}

	async loadLanguages() {
		// TODO: If we don't end up having a Global Context for languages, then we should at least change this into using a asObservable which should be returned from the repository. [Nl]
		const { data } = await this.#languageRepository.requestCollection({});
		this.#languages.setValue(data?.items ?? []);
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
			documentType: {
				unique: documentTypeUnique,
			},
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

	setTemplate(templateUnique: string) {
		this.#currentData.update({ template: { unique: templateUnique } });
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
		const currentData = this.getData();
		if (currentData) {
			const newDataSet = currentData.values?.find(
				(x) => x.alias === alias && (variantId ? variantId.compare(x) : true),
			);
			return newDataSet?.value as ReturnType;
		}
		return undefined;
	}
	async setPropertyValue<UmbDocumentValueModel = unknown>(
		alias: string,
		value: UmbDocumentValueModel,
		variantId?: UmbVariantId,
	) {
		if (!variantId) throw new Error('VariantId is missing');

		const entry = { ...variantId.toObject(), alias, value };
		const currentData = this.getData();
		if (currentData) {
			const values = appendToFrozenArray(
				currentData.values || [],
				entry,
				(x) => x.alias === alias && (variantId ? variantId.compare(x) : true),
			);
			this.#currentData.update({ values });
			this.changedVariants.appendOne(variantId);
		}
	}

	async #createOrSave(type: UmbDocumentVariantPickerModalData['type']): Promise<UmbVariantId[]> {
		const data = this.getData();
		if (!data) throw new Error('Data is missing');
		if (!data.unique) throw new Error('Unique is missing');

		const activeVariants = this.splitView.getActiveVariants();

		const pickedVariants = activeVariants.map((activeVariant) => UmbVariantId.Create(activeVariant).toString());
		const allowedVariants = await firstValueFrom(this.variantOptions);

		const selectedVariants = await this.pickVariants(type, allowedVariants, pickedVariants);

		// If no variants are selected, we don't save anything.
		if (!selectedVariants.length) return [];

		if (this.getIsNew()) {
			if ((await this.repository.create(data)).data !== undefined) {
				this.setIsNew(false);
			}
		} else {
			await this.repository.save(data);
		}

		return selectedVariants;
	}

	// TODO: refactor this part so it can be utilized by others? [NL]
	async pickVariants(
		type: UmbDocumentVariantPickerModalData['type'],
		availableVariants: Array<UmbDocumentVariantOptionModel>,
		selectedVariants?: Array<string>,
	): Promise<UmbVariantId[]> {
		// If there is only one variant, we don't need to select anything.
		if (availableVariants.length === 1) {
			// TODO: we are missing a good way to make a variantId from a variantOptionModel. [NL]
			return [new UmbVariantId(availableVariants[0].language.unique, null)];
		}

		const modalManagerContext = await this.consumeContext(UMB_MODAL_MANAGER_CONTEXT, () => {}).asPromise();

		const modalData: UmbDocumentVariantPickerModalData = {
			type,
			options: availableVariants,
		};

		const modalContext = modalManagerContext.open(UMB_DOCUMENT_LANGUAGE_PICKER_MODAL, {
			data: modalData,
			value: { selection: selectedVariants?.map((x) => x.toString()) ?? [] },
		});

		const result = await modalContext.onSubmit().catch(() => undefined);

		return result?.selection.map((x) => UmbVariantId.FromString(x)) ?? [];
	}

	async save() {
		await this.#createOrSave('save');
		const data = this.getData();
		if (!data) throw new Error('Data is missing');
		this.saveComplete(data);
	}

	public async publish() {
		const variantIds = await this.#createOrSave('publish');
		const unique = this.getEntityId();
		if (variantIds.length && unique) {
			await this.publishingRepository.publish(unique, variantIds);
		}
	}

	public async saveAndPublish() {
		await this.publish();
	}

	public async unpublish() {
		const unique = this.getEntityId();

		if (!unique) throw new Error('Unique is missing');
		//if (!this.#variantManagerContext) throw new Error('Variant manager context is missing');

		//this.#variantManagerContext.unpublish(unique);
		alert('not implemented');
		throw new Error('Not implemented');
	}

	async delete() {
		const id = this.getEntityId();
		if (id) {
			await this.repository.delete(id);
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
