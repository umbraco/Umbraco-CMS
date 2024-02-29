import { UmbDocumentTypeDetailRepository } from '../../document-types/repository/detail/document-type-detail.repository.js';
import { UmbDocumentPropertyDataContext } from '../property-dataset-context/document-property-dataset-context.js';
import { UMB_DOCUMENT_ENTITY_TYPE } from '../entity.js';
import { UmbDocumentDetailRepository } from '../repository/index.js';
import type { UmbDocumentDetailModel, UmbDocumentVariantModel, UmbDocumentVariantOptionModel } from '../types.js';
import { umbPickDocumentVariantModal, type UmbDocumentVariantPickerModalType } from '../modals/index.js';
import { UmbDocumentPublishingRepository } from '../repository/publishing/index.js';
import { UmbUnpublishDocumentEntityAction } from '../entity-actions/unpublish.action.js';
import { UMB_DOCUMENT_WORKSPACE_ALIAS } from './manifests.js';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import { UmbContentTypePropertyStructureManager } from '@umbraco-cms/backoffice/content-type';
import {
	UmbEditableWorkspaceContextBase,
	UmbWorkspaceSplitViewManager,
	type UmbVariantableWorkspaceContextInterface,
	type UmbPublishableWorkspaceContextInterface,
} from '@umbraco-cms/backoffice/workspace';
import {
	appendToFrozenArray,
	mergeObservables,
	naiveObjectComparison,
	UmbArrayState,
	UmbObjectState,
} from '@umbraco-cms/backoffice/observable-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbLanguageCollectionRepository, type UmbLanguageDetailModel } from '@umbraco-cms/backoffice/language';
import { firstValueFrom } from '@umbraco-cms/backoffice/external/rxjs';

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
	#persistedData = new UmbObjectState<EntityType | undefined>(undefined);
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
	readonly variantOptions = mergeObservables([this.variants, this.languages], ([variants, languages]) => {
		return languages.map((language) => {
			return {
				variant: variants.find((x) => x.culture === language.unique),
				language,
				// TODO: When including segments, this should be updated to include the segment as well. [NL]
				unique: language.unique, // This must be a variantId string!
			} as UmbDocumentVariantOptionModel;
		});
	});

	readonly urls = this.#currentData.asObservablePart((data) => data?.urls || []);
	readonly templateId = this.#currentData.asObservablePart((data) => data?.template?.unique || null);

	readonly structure = new UmbContentTypePropertyStructureManager(this, new UmbDocumentTypeDetailRepository(this));
	readonly variesByCulture = this.structure.ownerContentTypePart((x) => x?.variesByCulture);
	#variesByCulture?: boolean;

	readonly splitView = new UmbWorkspaceSplitViewManager();

	constructor(host: UmbControllerHost) {
		super(host, UMB_DOCUMENT_WORKSPACE_ALIAS);

		this.observe(this.contentTypeUnique, (unique) => this.structure.loadType(unique));
		this.observe(this.variesByCulture, (variesByCulture) => (this.#variesByCulture = variesByCulture));

		this.loadLanguages();
	}

	resetState() {
		super.resetState();
		this.#persistedData.setValue(undefined);
		this.#currentData.setValue(undefined);
	}

	async loadLanguages() {
		// TODO: If we don't end up having a Global Context for languages, then we should at least change this into using a asObservable which should be returned from the repository. [Nl]
		const { data } = await this.#languageRepository.requestCollection({});
		this.#languages.setValue(data?.items ?? []);
	}

	async load(unique: string) {
		this.resetState();
		this.#getDataPromise = this.repository.requestByUnique(unique);
		const { data } = await this.#getDataPromise;
		if (!data) return undefined;

		this.setIsNew(false);
		this.#persistedData.setValue(data);
		this.#currentData.setValue(data);
		return data || undefined;
	}

	async create(parentUnique: string | null, documentTypeUnique: string) {
		this.resetState();
		this.#getDataPromise = this.repository.createScaffold(parentUnique, {
			documentType: {
				unique: documentTypeUnique,
				collection: null,
			},
		});
		const { data } = await this.#getDataPromise;
		if (!data) return undefined;

		this.setIsNew(true);
		this.#persistedData.setValue(undefined);
		this.#currentData.setValue(data);
		return data;
	}

	getData() {
		return this.#currentData.getValue();
	}

	getUnique() {
		return this.getData()?.unique;
	}

	getEntityType() {
		return UMB_DOCUMENT_ENTITY_TYPE;
	}

	getContentTypeId() {
		return this.getData()?.documentType.unique;
	}

	// TODO: Check if this is used:
	getVariesByCulture() {
		return this.#variesByCulture;
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
		/*
		const oldVariants = this.#currentData.getValue()?.variants || [];
		const variants = partialUpdateFrozenArray(
			oldVariants,
			{ name },
			variantId ? (x) => variantId.compare(x) : () => true,
		);
		this.#currentData.update({ variants });
		*/
		// TODO: We should move this type of logic to the act of saving [NL]
		this.#updateVariantData(variantId ?? UmbVariantId.CreateInvariant(), { name });
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
		variantId ??= UmbVariantId.CreateInvariant();

		const entry = { ...variantId.toObject(), alias, value };
		const currentData = this.getData();
		if (currentData) {
			const values = appendToFrozenArray(
				currentData.values || [],
				entry,
				(x) => x.alias === alias && (variantId ? variantId.compare(x) : true),
			);
			this.#currentData.update({ values });

			// TODO: We should move this type of logic to the act of saving [NL]
			this.#updateVariantData(variantId);
		}
	}

	#calculateChangedVariants() {
		const persisted = this.#persistedData.getValue();
		const current = this.#currentData.getValue();
		if (!current) throw new Error('Current data is missing');

		const changedVariants = current?.variants.map((variant) => {
			const persistedVariant = persisted?.variants.find((x) => UmbVariantId.Create(variant).compare(x));
			return {
				culture: variant.culture,
				segment: variant.segment,
				equal: persistedVariant ? naiveObjectComparison(variant, persistedVariant) : false,
			};
		});

		const changedProperties = current?.values.map((value) => {
			const persistedValues = persisted?.values.find((x) => UmbVariantId.Create(value).compare(x));
			return {
				culture: value.culture,
				segment: value.segment,
				equal: persistedValues ? naiveObjectComparison(value, persistedValues) : false,
			};
		});

		// calculate the variantIds of those who either have a change in properties or in variants:
		return (
			changedVariants
				?.concat(changedProperties ?? [])
				.filter((x) => x.equal === false)
				.map((x) => new UmbVariantId(x.culture, x.segment)) ?? []
		);
	}

	#updateVariantData(variantId: UmbVariantId, update?: Partial<UmbDocumentVariantModel>) {
		const currentData = this.getData();
		if (!currentData) throw new Error('Data is missing');
		if (this.#variesByCulture === true) {
			// If variant Id is invariant, we don't to have the variant appended to our data.
			if (variantId.isInvariant()) return;
			const variant = currentData.variants.find((x) => variantId.compare(x));
			const newVariants = appendToFrozenArray(
				currentData.variants,
				{
					state: null,
					name: '',
					publishDate: null,
					createDate: null,
					updateDate: null,
					...variantId.toObject(),
					...variant,
					...update,
				},
				(x) => variantId.compare(x),
			);
			this.#currentData.update({ variants: newVariants });
		} else if (this.#variesByCulture === false) {
			// TODO: Beware about segments, in this case we need to also consider segments, if its allowed to vary by segments.
			const invariantVariantId = UmbVariantId.CreateInvariant();
			const variant = currentData.variants.find((x) => invariantVariantId.compare(x));
			// Cause we are invariant, we will just overwrite all variants with this one:
			const newVariants = [
				{
					state: null,
					name: '',
					publishDate: null,
					createDate: null,
					updateDate: null,
					...variantId.toObject(),
					...variant,
					...update,
				},
			];
			this.#currentData.update({ variants: newVariants });
		} else {
			throw new Error('Varies by culture is missing');
		}
	}

	async #pickVariantsForAction(type: UmbDocumentVariantPickerModalType): Promise<UmbVariantId[]> {
		const activeVariants = this.splitView.getActiveVariants();

		// TODO: Picked variants should include the ones that has been changed (but not jet saved) this requires some more awareness about the state of runtime data. [NL]
		const activeVariantIds = activeVariants.map((activeVariant) => UmbVariantId.Create(activeVariant));
		const selected = activeVariantIds.concat(this.#calculateChangedVariants());
		const options = await firstValueFrom(this.variantOptions);

		// If there is only one variant, we don't need to open the modal.
		if (options.length === 0) {
			throw new Error('No variants are available');
		} else if (options.length === 1) {
			// If only one option we will skip ahead and save the document with the only variant available:
			const firstVariant = new UmbVariantId(options[0].language.unique, null);
			return await this.#performSaveOrCreate([firstVariant]);
		}

		const selectedVariants = await umbPickDocumentVariantModal(this, { type, options, selected });

		// If no variants are selected, we don't save anything.
		if (!selectedVariants.length) return [];

		return await this.#performSaveOrCreate(selectedVariants);
	}

	async #performSaveOrCreate(selectedVariants: Array<UmbVariantId>) {
		const data = this.getData();
		if (!data) throw new Error('Data is missing');
		if (!data.unique) throw new Error('Unique is missing');

		if (this.getIsNew()) {
			if ((await this.repository.create(data)).data !== undefined) {
				this.setIsNew(false);
			}
		} else {
			await this.repository.save(data);
		}

		return selectedVariants;
	}

	async save() {
		await this.#pickVariantsForAction('save');
		const data = this.getData();
		if (!data) throw new Error('Data is missing');

		this.#persistedData.setValue(data);
		this.#currentData.setValue(data);

		this.saveComplete(data);
	}

	public async publish() {
		const variantIds = await this.#pickVariantsForAction('publish');
		const unique = this.getUnique();
		if (variantIds.length && unique) {
			await this.publishingRepository.publish(unique, variantIds);
		}
	}

	public async saveAndPublish() {
		await this.publish();
	}

	public async unpublish() {
		const unique = this.getUnique();

		if (!unique) throw new Error('Unique is missing');
		new UmbUnpublishDocumentEntityAction(this, '', unique, '').execute();
	}

	async delete() {
		const id = this.getUnique();
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
		this.#persistedData.destroy();
		this.#currentData.destroy();
		this.structure.destroy();
		this.#languageRepository.destroy();
		super.destroy();
	}
}

export default UmbDocumentWorkspaceContext;
