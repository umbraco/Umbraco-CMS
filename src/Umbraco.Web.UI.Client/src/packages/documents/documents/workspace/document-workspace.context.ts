import { UmbDocumentTypeDetailRepository } from '../../document-types/repository/detail/document-type-detail.repository.js';
import { UmbDocumentPropertyDataContext } from '../property-dataset-context/document-property-dataset-context.js';
import { UMB_DOCUMENT_ENTITY_TYPE } from '../entity.js';
import { UmbDocumentDetailRepository } from '../repository/index.js';
import type {
	UmbDocumentDetailModel,
	UmbDocumentValueModel,
	UmbDocumentVariantModel,
	UmbDocumentVariantOptionModel,
} from '../types.js';
import { umbPickDocumentVariantModal, type UmbDocumentVariantPickerModalType } from '../modals/index.js';
import { UmbDocumentPublishingRepository } from '../repository/publishing/index.js';
import { UmbUnpublishDocumentEntityAction } from '../entity-actions/unpublish.action.js';
import { UMB_DOCUMENT_WORKSPACE_ALIAS } from './manifests.js';
import { UMB_INVARIANT_CULTURE, UmbVariantId } from '@umbraco-cms/backoffice/variant';
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
	jsonStringComparison,
	UmbArrayState,
	UmbObjectState,
} from '@umbraco-cms/backoffice/observable-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbLanguageCollectionRepository, type UmbLanguageDetailModel } from '@umbraco-cms/backoffice/language';
import { firstValueFrom } from '@umbraco-cms/backoffice/external/rxjs';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import { UmbReloadTreeItemChildrenRequestEntityActionEvent } from '@umbraco-cms/backoffice/tree';

type EntityType = UmbDocumentDetailModel;
export class UmbDocumentWorkspaceContext
	extends UmbEditableWorkspaceContextBase<EntityType>
	implements UmbVariantableWorkspaceContextInterface<UmbDocumentVariantModel>, UmbPublishableWorkspaceContextInterface
{
	//
	public readonly repository = new UmbDocumentDetailRepository(this);
	public readonly publishingRepository = new UmbDocumentPublishingRepository(this);

	#parent?: { entityType: string; unique: string | null };

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

	readonly urls = this.#currentData.asObservablePart((data) => data?.urls || []);
	readonly templateId = this.#currentData.asObservablePart((data) => data?.template?.unique || null);

	readonly structure = new UmbContentTypePropertyStructureManager(this, new UmbDocumentTypeDetailRepository(this));
	readonly variesByCulture = this.structure.ownerContentTypePart((x) => x?.variesByCulture);
	//#variesByCulture?: boolean;
	readonly variesBySegment = this.structure.ownerContentTypePart((x) => x?.variesBySegment);
	//#variesBySegment?: boolean;
	readonly varies = this.structure.ownerContentTypePart((x) =>
		x ? x.variesByCulture || x.variesBySegment : undefined,
	);
	#varies?: boolean;

	readonly splitView = new UmbWorkspaceSplitViewManager();

	readonly variantOptions = mergeObservables(
		[this.varies, this.variants, this.languages],
		([varies, variants, languages]) => {
			// TODO: When including segments, when be aware about the case of segment varying when not culture varying. [NL]
			if (varies === true) {
				return languages.map((language) => {
					return {
						variant: variants.find((x) => x.culture === language.unique),
						language,
						// TODO: When including segments, this object should be updated to include a object for the segment. [NL]
						// TODO: When including segments, the unique should be updated to include the segment as well. [NL]
						unique: language.unique, // This must be a variantId string!
						culture: language.unique,
						segment: null,
					} as UmbDocumentVariantOptionModel;
				});
			} else if (varies === false) {
				return [
					{
						variant: variants.find((x) => x.culture === null),
						language: languages.find((x) => x.isDefault),
						culture: null,
						segment: null,
						unique: UMB_INVARIANT_CULTURE, // This must be a variantId string!
					} as UmbDocumentVariantOptionModel,
				];
			}
			return [] as Array<UmbDocumentVariantOptionModel>;
		},
	);

	constructor(host: UmbControllerHost) {
		super(host, UMB_DOCUMENT_WORKSPACE_ALIAS);

		this.observe(this.contentTypeUnique, (unique) => this.structure.loadType(unique));
		this.observe(this.varies, (varies) => (this.#varies = varies));

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

	async create(parent: { entityType: string; unique: string | null }, documentTypeUnique: string) {
		this.resetState();
		this.#parent = parent;
		this.#getDataPromise = this.repository.createScaffold({
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
	getVaries() {
		return this.#varies;
	}
	/*
	getVariesByCulture() {
		return this.#variesByCulture;
	}
	getVariesBySegment() {
		return this.#variesBySegment;
	}*/

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
				equal: persistedVariant ? jsonStringComparison(variant, persistedVariant) : false,
			};
		});

		const changedProperties = current?.values.map((value) => {
			const persistedValues = persisted?.values.find((x) => UmbVariantId.Create(value).compare(x));
			return {
				culture: value.culture,
				segment: value.segment,
				equal: persistedValues ? jsonStringComparison(value, persistedValues) : false,
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
		if (this.#varies === true) {
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
		} else if (this.#varies === false) {
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
					...invariantVariantId.toObject(),
					...variant,
					...update,
				},
			];
			this.#currentData.update({ variants: newVariants });
		} else {
			throw new Error('Varies by culture is missing');
		}
	}

	async #runUserFlorFor(type: UmbDocumentVariantPickerModalType): Promise<UmbVariantId[]> {
		const activeVariants = this.splitView.getActiveVariants();

		const activeVariantIds = activeVariants.map((activeVariant) => UmbVariantId.Create(activeVariant));
		// TODO: We need to filter the selected array, so it only contains one of each variantId. [NL]
		const changedVariantIds = this.#calculateChangedVariants();
		const selected = activeVariantIds.concat(changedVariantIds);
		// Selected can contain entries that are not part of the options, therefor the modal filters selection based on options.

		const options = await firstValueFrom(this.variantOptions);

		// If there is only one variant, we don't need to open the modal.
		if (options.length === 0) {
			throw new Error('No variants are available');
		} else if (options.length === 1) {
			// If only one option we will skip ahead and save the document with the only variant available:
			const firstVariant = UmbVariantId.Create(options[0]);
			return this.#performSaveOrCreate([firstVariant]);
		}

		const selectedVariants = await umbPickDocumentVariantModal(this, { type, options, selected });

		// If no variants are selected, we don't save anything.
		if (!selectedVariants.length) return [];

		return this.#performSaveOrCreate(selectedVariants);
	}

	#buildSaveData(selectedVariants: Array<UmbVariantId>): UmbDocumentDetailModel {
		const data = this.getData();
		if (!data) throw new Error('Data is missing');
		if (!data.unique) throw new Error('Unique is missing');
		const invariantVariantId = UmbVariantId.CreateInvariant();
		if (this.#varies === false) {
			// If we do not vary, we wil just do this for the invariant variant id.
			selectedVariants = [invariantVariantId];
		}

		const persistedData = this.#persistedData.getValue();

		const variantIdsToParseForValues = [...selectedVariants];
		if (this.#varies === true) {
			// If we vary then We need to include the invariant variant id for invariant values to be saved, as we always want to save the invariant values.
			variantIdsToParseForValues.push(invariantVariantId);
		}

		// Combine data and persisted data depending on the selectedVariants. Always use the invariant values from the data.
		// loops over each entry in values, determine wether the value should be from the data or the persisted data, depending on wether its a selectedVariant or an invariant value.
		// loops over each entry in variants, determine wether the variant should be from the data or the persisted data, depending on the selectedVariants.
		return {
			...data,
			values: data.values
				.map((value) => {
					// Should this value be saved?
					if (variantIdsToParseForValues.some((x) => x.compare(value))) {
						return value;
					} else {
						// If not we will find the value in the persisted data and use that instead.
						return persistedData?.values.find(
							(x) => x.alias === value.alias && x.culture === value.culture && x.segment === value.segment,
						);
					}
				})
				.filter((x) => x !== undefined) as Array<UmbDocumentValueModel<unknown>>,
			variants: data.variants
				.map((variant) => {
					// Should this value be saved?
					if (selectedVariants.some((x) => x.compare(variant))) {
						return variant;
					} else {
						// If not we will find the value in the persisted data and use that instead.
						return persistedData?.variants.find((x) => x.culture === variant.culture && x.segment === variant.segment);
					}
				})
				.filter((x) => x !== undefined) as Array<UmbDocumentVariantModel>,
		};
	}

	async #performSaveOrCreate(selectedVariants: Array<UmbVariantId>) {
		const saveData = this.#buildSaveData(selectedVariants);

		if (this.getIsNew()) {
			if (!this.#parent) throw new Error('Parent is not set');

			const { data: create, error } = await this.repository.create(saveData, this.#parent.unique);
			if (!create || error) {
				console.error('Error creating document', error);
				throw new Error('Error creating document');
			}

			this.setIsNew(false);

			// TODO: this might not be the right place to alert the tree, but it works for now
			const eventContext = await this.getContext(UMB_ACTION_EVENT_CONTEXT);
			const event = new UmbReloadTreeItemChildrenRequestEntityActionEvent({
				entityType: this.#parent.entityType,
				unique: this.#parent.unique,
			});
			eventContext.dispatchEvent(event);
		} else {
			const { data: save, error } = await this.repository.save(saveData);
			if (!save || error) {
				console.error('Error saving document', error);
				throw new Error('Error saving document');
			}
		}

		return selectedVariants;
	}

	async save() {
		await this.#runUserFlorFor('save');
		const data = this.getData();
		if (!data) throw new Error('Data is missing');

		this.#persistedData.setValue(data);
		this.#currentData.setValue(data);

		this.workspaceComplete(data);
	}

	public async publish() {
		const variantIds = await this.#runUserFlorFor('publish');
		const unique = this.getUnique();
		if (variantIds.length && unique) {
			await this.publishingRepository.publish(unique, variantIds);
			this.workspaceComplete(this.#currentData.getValue());
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
