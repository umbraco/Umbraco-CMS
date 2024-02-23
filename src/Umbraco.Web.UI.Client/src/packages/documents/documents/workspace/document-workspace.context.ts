import { UmbDocumentTypeDetailRepository } from '../../document-types/repository/detail/document-type-detail.repository.js';
import { UmbDocumentPropertyDataContext } from '../property-dataset-context/document-property-dataset-context.js';
import { UMB_DOCUMENT_ENTITY_TYPE } from '../entity.js';
import { UmbDocumentDetailRepository } from '../repository/index.js';
import { UmbDocumentVariantState, type UmbDocumentDetailModel, type UmbDocumentVariantModel } from '../types.js';
import type { UmbDocumentVariantPickerModalData } from '../modals/index.js';
import { UmbDocumentPublishingRepository } from '../repository/publishing/index.js';
import { UMB_DOCUMENT_VARIANT_MANAGER_CONTEXT } from '../global-contexts/document-variant-manager.context.js';
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
	combineObservables,
	partialUpdateFrozenArray,
	UmbArrayState,
	UmbObjectState,
} from '@umbraco-cms/backoffice/observable-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbLanguageCollectionRepository, type UmbLanguageDetailModel } from '@umbraco-cms/backoffice/language';
import { firstValueFrom } from '@umbraco-cms/backoffice/external/rxjs';

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
	#variantManagerContext?: typeof UMB_DOCUMENT_VARIANT_MANAGER_CONTEXT.TYPE;
	#languageRepository = new UmbLanguageCollectionRepository(this);
	#languageCollection = new UmbArrayState<UmbLanguageDetailModel>([], (x) => x.unique);

	public isLoaded() {
		return this.#getDataPromise;
	}

	readonly unique = this.#currentData.asObservablePart((data) => data?.unique);

	readonly contentTypeUnique = this.#currentData.asObservablePart((data) => data?.documentType.unique);
	readonly contentTypeHasCollection = this.#currentData.asObservablePart((data) => !!data?.documentType.collection);
	readonly variants = this.#currentData.asObservablePart((data) => data?.variants ?? []);
	readonly allowedVariants = combineObservables(
		[this.variants, this.#languageCollection.asObservable()],
		([variants, languages]) => {
			const missingLanguages = languages.filter((x) => !variants.some((v) => v.culture === x.unique));
			const newVariants = variants.concat(
				missingLanguages.map<UmbDocumentVariantModel>((language) => ({
					state: UmbDocumentVariantState.NOT_CREATED,
					isMandatory: language.isMandatory,
					culture: language.unique,
					segment: null,
					name: language.name,
					createDate: '',
					publishDate: '',
					updateDate: '',
				})),
			);
			return newVariants;
		},
	);
	readonly changedVariants = new UmbArrayState<UmbVariantId>([], (x) => x.compare);
	readonly urls = this.#currentData.asObservablePart((data) => data?.urls || []);
	readonly templateId = this.#currentData.asObservablePart((data) => data?.template?.unique || null);

	readonly structure = new UmbContentTypePropertyStructureManager(this, new UmbDocumentTypeDetailRepository(this));
	readonly splitView = new UmbWorkspaceSplitViewManager();

	constructor(host: UmbControllerHost) {
		super(host, UMB_DOCUMENT_WORKSPACE_ALIAS);

		this.consumeContext(UMB_DOCUMENT_VARIANT_MANAGER_CONTEXT, (instance) => {
			this.#variantManagerContext = instance;
		});

		this.observe(this.contentTypeUnique, (unique) => this.structure.loadType(unique));

		this.loadLanguages();
	}

	async loadLanguages() {
		const { data } = await this.#languageRepository.requestCollection({});
		const languages = data?.items || [];
		this.#languageCollection.setValue(languages);
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
		if (!this.#variantManagerContext) throw new Error('Variant manager context is missing');

		const activeVariants = this.splitView.getActiveVariants();
		const activeVariant = activeVariants.length ? activeVariants[0] : undefined;

		// Calculate variants
		const pickedVariants: string[] = [];
		const availableVariants = await firstValueFrom(this.allowedVariants);
		let allowedVariants = data.variants;

		// Make sure that the active variant is in the allowed variants
		if (activeVariant) {
			const activeVariantInAvailableVariants = availableVariants.find(
				(x) => x.culture === activeVariant.culture && x.segment === activeVariant.segment,
			);
			if (activeVariantInAvailableVariants) {
				pickedVariants.push(activeVariantInAvailableVariants.culture!);
				allowedVariants = appendToFrozenArray(
					allowedVariants,
					activeVariantInAvailableVariants,
					(x) =>
						x.culture === activeVariantInAvailableVariants.culture &&
						x.segment === activeVariantInAvailableVariants.segment,
				);
			}
		}

		// Make sure the changed variants are in the allowed variants
		const changedVariants = this.changedVariants.getValue();
		if (changedVariants.length) {
			pickedVariants.push(...changedVariants.map((x) => x.culture!));
			const changedVariantsInAvailableVariants = availableVariants.filter((x) =>
				changedVariants.some((y) => y.equal(new UmbVariantId(x))),
			);
			for (const changedVariant of changedVariantsInAvailableVariants) {
				allowedVariants = appendToFrozenArray(
					allowedVariants,
					changedVariant,
					(x) => changedVariant.culture === x.culture && changedVariant.segment === x.segment,
				);
			}
		}

		const selectedVariants = await this.#variantManagerContext.pickVariants(allowedVariants, type, pickedVariants);

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
		if (!this.#variantManagerContext) throw new Error('Variant manager context is missing');

		this.#variantManagerContext.unpublish(unique);
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
