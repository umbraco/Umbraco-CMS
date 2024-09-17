import { UmbDocumentTypeDetailRepository } from '../../document-types/repository/detail/document-type-detail.repository.js';
import { UmbDocumentPropertyDatasetContext } from '../property-dataset-context/document-property-dataset-context.js';
import { UMB_DOCUMENT_ENTITY_TYPE } from '../entity.js';
import { UmbDocumentDetailRepository } from '../repository/index.js';
import type {
	UmbDocumentVariantPublishModel,
	UmbDocumentDetailModel,
	UmbDocumentValueModel,
	UmbDocumentVariantModel,
	UmbDocumentVariantOptionModel,
} from '../types.js';
import {
	UMB_DOCUMENT_PUBLISH_MODAL,
	UMB_DOCUMENT_PUBLISH_WITH_DESCENDANTS_MODAL,
	UMB_DOCUMENT_SCHEDULE_MODAL,
	UMB_DOCUMENT_SAVE_MODAL,
} from '../modals/index.js';
import { UmbDocumentPublishingRepository } from '../repository/publishing/index.js';
import { UmbUnpublishDocumentEntityAction } from '../entity-actions/unpublish.action.js';
import { UmbDocumentValidationRepository } from '../repository/validation/document-validation.repository.js';
import {
	UMB_CREATE_DOCUMENT_WORKSPACE_PATH_PATTERN,
	UMB_CREATE_FROM_BLUEPRINT_DOCUMENT_WORKSPACE_PATH_PATTERN,
	UMB_EDIT_DOCUMENT_WORKSPACE_PATH_PATTERN,
} from '../paths.js';
import { UMB_DOCUMENTS_SECTION_PATH } from '../../section/paths.js';
import { UmbDocumentPreviewRepository } from '../repository/preview/index.js';
import { sortVariants } from '../utils.js';
import { UMB_DOCUMENT_WORKSPACE_ALIAS } from './constants.js';
import { UmbEntityContext } from '@umbraco-cms/backoffice/entity';
import { UMB_INVARIANT_CULTURE, UmbVariantId } from '@umbraco-cms/backoffice/variant';
import { UmbContentTypeStructureManager } from '@umbraco-cms/backoffice/content-type';
import {
	type UmbCollectionWorkspaceContext,
	type UmbPublishableWorkspaceContext,
	UmbSubmittableWorkspaceContextBase,
	UmbWorkspaceIsNewRedirectController,
	UmbWorkspaceSplitViewManager,
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
import { type Observable, firstValueFrom, map } from '@umbraco-cms/backoffice/external/rxjs';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import {
	UmbRequestReloadChildrenOfEntityEvent,
	UmbRequestReloadStructureForEntityEvent,
} from '@umbraco-cms/backoffice/entity-action';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';
import {
	UMB_VALIDATION_CONTEXT,
	UMB_VALIDATION_EMPTY_LOCALIZATION_KEY,
	UmbDataPathVariantQuery,
	UmbServerModelValidatorContext,
	UmbValidationContext,
	UmbVariantValuesValidationPathTranslator,
	UmbVariantsValidationPathTranslator,
} from '@umbraco-cms/backoffice/validation';
import { UmbDocumentBlueprintDetailRepository } from '@umbraco-cms/backoffice/document-blueprint';
import { UMB_NOTIFICATION_CONTEXT } from '@umbraco-cms/backoffice/notification';
import type { UmbContentWorkspaceContext } from '@umbraco-cms/backoffice/content';
import type { UmbDocumentTypeDetailModel } from '@umbraco-cms/backoffice/document-type';
import { UmbIsTrashedEntityContext } from '@umbraco-cms/backoffice/recycle-bin';
import { UmbReadOnlyVariantStateManager } from '@umbraco-cms/backoffice/utils';

type EntityType = UmbDocumentDetailModel;
export class UmbDocumentWorkspaceContext
	extends UmbSubmittableWorkspaceContextBase<EntityType>
	implements
		UmbContentWorkspaceContext<UmbDocumentTypeDetailModel, UmbDocumentVariantModel>,
		UmbPublishableWorkspaceContext,
		UmbCollectionWorkspaceContext<UmbDocumentTypeDetailModel>
{
	public readonly IS_CONTENT_WORKSPACE_CONTEXT = true as const;

	public readonly repository = new UmbDocumentDetailRepository(this);
	public readonly publishingRepository = new UmbDocumentPublishingRepository(this);

	#parent = new UmbObjectState<{ entityType: string; unique: string | null } | undefined>(undefined);
	readonly parentUnique = this.#parent.asObservablePart((parent) => (parent ? parent.unique : undefined));
	readonly parentEntityType = this.#parent.asObservablePart((parent) => (parent ? parent.entityType : undefined));

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

	#serverValidation = new UmbServerModelValidatorContext(this);
	#validationRepository?: UmbDocumentValidationRepository;

	#blueprintRepository = new UmbDocumentBlueprintDetailRepository(this);
	/*#blueprint = new UmbObjectState<UmbDocumentBlueprintDetailModel | undefined>(undefined);
	public readonly blueprint = this.#blueprint.asObservable();*/

	public readOnlyState = new UmbReadOnlyVariantStateManager(this);

	public isLoaded() {
		return this.#getDataPromise;
	}

	readonly unique = this.#currentData.asObservablePart((data) => data?.unique);
	readonly entityType = this.#currentData.asObservablePart((data) => data?.entityType);
	readonly isTrashed = this.#currentData.asObservablePart((data) => data?.isTrashed);

	readonly contentTypeUnique = this.#currentData.asObservablePart((data) => data?.documentType.unique);
	readonly contentTypeHasCollection = this.#currentData.asObservablePart((data) => !!data?.documentType.collection);

	readonly variants = this.#currentData.asObservablePart((data) => data?.variants ?? []);

	readonly urls = this.#currentData.asObservablePart((data) => data?.urls || []);
	readonly templateId = this.#currentData.asObservablePart((data) => data?.template?.unique || null);

	readonly structure = new UmbContentTypeStructureManager(this, new UmbDocumentTypeDetailRepository(this));
	readonly variesByCulture = this.structure.ownerContentTypePart((x) => x?.variesByCulture);
	readonly variesBySegment = this.structure.ownerContentTypePart((x) => x?.variesBySegment);
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
	).pipe(map((results) => results.sort(sortVariants)));

	// TODO: this should be set up for all entity workspace contexts in a base class
	#entityContext = new UmbEntityContext(this);
	// TODO: this might not be the correct place to spin this up
	#isTrashedContext = new UmbIsTrashedEntityContext(this);

	constructor(host: UmbControllerHost) {
		super(host, UMB_DOCUMENT_WORKSPACE_ALIAS);

		this.addValidationContext(new UmbValidationContext(this).provide());

		new UmbVariantValuesValidationPathTranslator(this);
		new UmbVariantsValidationPathTranslator(this);

		this.observe(this.contentTypeUnique, (unique) => this.structure.loadType(unique));
		this.observe(this.varies, (varies) => (this.#varies = varies));

		this.loadLanguages();

		this.routes.setRoutes([
			{
				path: UMB_CREATE_FROM_BLUEPRINT_DOCUMENT_WORKSPACE_PATH_PATTERN.toString(),
				component: () => import('./document-workspace-editor.element.js'),
				setup: async (_component, info) => {
					const parentEntityType = info.match.params.parentEntityType;
					const parentUnique: string | null =
						info.match.params.parentUnique === 'null' ? null : info.match.params.parentUnique;
					const documentTypeUnique = info.match.params.documentTypeUnique;
					const blueprintUnique = info.match.params.blueprintUnique;

					this.create({ entityType: parentEntityType, unique: parentUnique }, documentTypeUnique, blueprintUnique);
					new UmbWorkspaceIsNewRedirectController(
						this,
						this,
						this.getHostElement().shadowRoot!.querySelector('umb-router-slot')!,
					);
				},
			},
			{
				path: UMB_CREATE_DOCUMENT_WORKSPACE_PATH_PATTERN.toString(),
				component: () => import('./document-workspace-editor.element.js'),
				setup: async (_component, info) => {
					const parentEntityType = info.match.params.parentEntityType;
					const parentUnique = info.match.params.parentUnique === 'null' ? null : info.match.params.parentUnique;
					const documentTypeUnique = info.match.params.documentTypeUnique;
					this.create({ entityType: parentEntityType, unique: parentUnique }, documentTypeUnique);

					new UmbWorkspaceIsNewRedirectController(
						this,
						this,
						this.getHostElement().shadowRoot!.querySelector('umb-router-slot')!,
					);
				},
			},
			{
				path: UMB_EDIT_DOCUMENT_WORKSPACE_PATH_PATTERN.toString(),
				component: () => import('./document-workspace-editor.element.js'),
				setup: (_component, info) => {
					const unique = info.match.params.unique;
					this.load(unique);
				},
			},
		]);
	}

	override resetState() {
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
		type GetDataType = Awaited<ReturnType<UmbDocumentDetailRepository['requestByUnique']>>;
		const { data, asObservable } = (await this.#getDataPromise) as GetDataType;

		if (data) {
			this.#entityContext.setEntityType(UMB_DOCUMENT_ENTITY_TYPE);
			this.#entityContext.setUnique(unique);
			this.#isTrashedContext.setIsTrashed(data.isTrashed);
			this.setIsNew(false);
			this.#persistedData.setValue(data);
			this.#currentData.setValue(data);
		}

		this.observe(asObservable(), (entity) => this.#onStoreChange(entity), 'umbDocumentStoreObserver');
	}

	#onStoreChange(entity: EntityType | undefined) {
		if (!entity) {
			//TODO: This solution is alright for now. But reconsider when we introduce signal-r
			history.pushState(null, '', UMB_DOCUMENTS_SECTION_PATH);
		}
	}

	async create(
		parent: { entityType: string; unique: string | null },
		documentTypeUnique: string,
		blueprintUnique?: string,
	) {
		this.resetState();
		this.#parent.setValue(parent);

		if (blueprintUnique) {
			const { data } = await this.#blueprintRepository.requestByUnique(blueprintUnique);

			this.#getDataPromise = this.repository.createScaffold({
				documentType: data?.documentType,
				values: data?.values,
				variants: data?.variants as Array<UmbDocumentVariantModel>,
			});
		} else {
			this.#getDataPromise = this.repository.createScaffold({
				documentType: {
					unique: documentTypeUnique,
					collection: null,
				},
			});
		}

		const { data } = await this.#getDataPromise;
		if (!data) return undefined;

		this.#entityContext.setEntityType(UMB_DOCUMENT_ENTITY_TYPE);
		this.#entityContext.setUnique(data.unique);
		this.#isTrashedContext.setIsTrashed(data.isTrashed);
		this.setIsNew(true);
		this.#persistedData.setValue(undefined);
		this.#currentData.setValue(data);
		return data;
	}

	getCollectionAlias() {
		return 'Umb.Collection.Document';
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
		// TODO: We should move this type of logic to the act of saving [NL]
		this.#updateVariantData(variantId ?? UmbVariantId.CreateInvariant(), { name });
	}

	name(variantId?: UmbVariantId) {
		return this.#currentData.asObservablePart((data) => data?.variants?.find((x) => variantId?.compare(x))?.name ?? '');
	}

	setTemplate(templateUnique: string) {
		this.#currentData.update({ template: { unique: templateUnique } });
	}

	async propertyStructureById(propertyId: string) {
		return this.structure.propertyStructureById(propertyId);
	}

	/**
	 * @function propertyValueByAlias
	 * @param {string} propertyAlias
	 * @param {UmbVariantId} variantId
	 * @returns {Promise<Observable<ReturnType | undefined> | undefined>}
	 * @description Get an Observable for the value of this property.
	 */
	async propertyValueByAlias<PropertyValueType = unknown>(
		propertyAlias: string,
		variantId?: UmbVariantId,
	): Promise<Observable<PropertyValueType | undefined> | undefined> {
		return this.#currentData.asObservablePart(
			(data) =>
				data?.values?.find((x) => x?.alias === propertyAlias && (variantId ? variantId.compare(x) : true))
					?.value as PropertyValueType,
		);
	}
	// TODO: Re-evaluate if this is begin used, i wrote this as part of a POC... [NL]
	/*
	async propertyIndexByAlias(
		propertyAlias: string,
		variantId?: UmbVariantId,
	): Promise<Observable<number | undefined> | undefined> {
		return this.#currentData.asObservablePart((data) =>
			data?.values?.findIndex((x) => x?.alias === propertyAlias && (variantId ? variantId.compare(x) : true)),
		);
	}
	*/

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
	async setPropertyValue<ValueType = unknown>(alias: string, value: ValueType, variantId?: UmbVariantId) {
		variantId ??= UmbVariantId.CreateInvariant();

		const entry = { ...variantId.toObject(), alias, value } as UmbDocumentValueModel<ValueType>;
		const currentData = this.getData();
		if (currentData) {
			const values = appendToFrozenArray(
				currentData.values ?? [],
				entry,
				(x) => x.alias === alias && variantId!.compare(x),
			);
			this.#currentData.update({ values });

			// TODO: We should move this type of logic to the act of saving [NL]
			this.#updateVariantData(variantId);
		}
	}

	#updateLock = 0;
	initiatePropertyValueChange() {
		this.#updateLock++;
		this.#currentData.mute();
		// TODO: When ready enable this code will enable handling a finish automatically by this implementation 'using myState.initiatePropertyValueChange()' (Relies on TS support of Using) [NL]
		/*return {
			[Symbol.dispose]: this.finishPropertyValueChange,
		};*/
	}
	finishPropertyValueChange = () => {
		this.#updateLock--;
		this.#triggerPropertyValueChanges();
	};
	#triggerPropertyValueChanges() {
		if (this.#updateLock === 0) {
			this.#currentData.unmute();
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

	async #determineVariantOptions() {
		const activeVariants = this.splitView.getActiveVariants();

		const activeVariantIds = activeVariants.map((activeVariant) => UmbVariantId.Create(activeVariant));
		// TODO: We need to filter the selected array, so it only contains one of each variantId. [NL]
		const changedVariantIds = this.#calculateChangedVariants();
		const selected = activeVariantIds.concat(changedVariantIds);
		// Selected can contain entries that are not part of the options, therefor the modal filters selection based on options.

		const readOnlyCultures = this.readOnlyState.getStates().map((s) => s.variantId.culture);
		const selectedCultures = selected.map((x) => x.toString()).filter((v, i, a) => a.indexOf(v) === i);
		const writable = selectedCultures.filter((x) => readOnlyCultures.includes(x) === false);

		const options = await firstValueFrom(this.variantOptions);

		return {
			options,
			selected: writable,
		};
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
						// If not, then we will find the value in the persisted data and use that instead.
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

	async #performSaveOrCreate(saveData: UmbDocumentDetailModel): Promise<void> {
		if (this.getIsNew()) {
			// Create:
			const parent = this.#parent.getValue();
			if (!parent) throw new Error('Parent is not set');

			const { data, error } = await this.repository.create(saveData, parent.unique);
			if (!data || error) {
				console.error('Error creating document', error);
				throw new Error('Error creating document');
			}

			this.setIsNew(false);
			this.#persistedData.setValue(data);
			this.#currentData.setValue(data);

			const eventContext = await this.getContext(UMB_ACTION_EVENT_CONTEXT);
			const event = new UmbRequestReloadChildrenOfEntityEvent({
				entityType: parent.entityType,
				unique: parent.unique,
			});
			eventContext.dispatchEvent(event);
		} else {
			// Save:
			const { data, error } = await this.repository.save(saveData);
			if (!data || error) {
				console.error('Error saving document', error);
				throw new Error('Error saving document');
			}

			this.#persistedData.setValue(data);
			this.#currentData.setValue(data);

			const eventContext = await this.getContext(UMB_ACTION_EVENT_CONTEXT);
			const event = new UmbRequestReloadStructureForEntityEvent({
				entityType: this.getEntityType(),
				unique: this.getUnique()!,
			});

			eventContext.dispatchEvent(event);
		}
	}

	#readOnlyLanguageVariantsFilter = (option: UmbDocumentVariantOptionModel) => {
		const readOnlyCultures = this.readOnlyState.getStates().map((s) => s.variantId.culture);
		return readOnlyCultures.includes(option.culture) === false;
	};

	async #handleSaveAndPreview() {
		const unique = this.getUnique();
		if (!unique) throw new Error('Unique is missing');

		let culture = UMB_INVARIANT_CULTURE;

		// Save document (the active variant) before previewing.
		const { selected } = await this.#determineVariantOptions();
		if (selected.length > 0) {
			culture = selected[0];
			const variantId = UmbVariantId.FromString(culture);
			const saveData = this.#buildSaveData([variantId]);
			await this.#runMandatoryValidationForSaveData(saveData);
			await this.#performSaveOrCreate(saveData);
		}

		// Tell the server that we're entering preview mode.
		await new UmbDocumentPreviewRepository(this).enter();

		const preview = window.open(`preview?id=${unique}&culture=${culture}`, 'umbpreview');
		preview?.focus();
	}

	async #handleSaveAndPublish() {
		const unique = this.getUnique();
		if (!unique) throw new Error('Unique is missing');

		let variantIds: Array<UmbVariantId> = [];

		const { options, selected } = await this.#determineVariantOptions();

		// If there is only one variant, we don't need to open the modal.
		if (options.length === 0) {
			throw new Error('No variants are available');
		} else if (options.length === 1) {
			// If only one option we will skip ahead and save the document with the only variant available:
			variantIds.push(UmbVariantId.Create(options[0]));
		} else {
			// If there are multiple variants, we will open the modal to let the user pick which variants to publish.
			const modalManagerContext = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
			const result = await modalManagerContext
				.open(this, UMB_DOCUMENT_PUBLISH_MODAL, {
					data: {
						options,
						pickableFilter: this.#readOnlyLanguageVariantsFilter,
					},
					value: { selection: selected },
				})
				.onSubmit()
				.catch(() => undefined);

			if (!result?.selection.length || !unique) return;

			variantIds = result?.selection.map((x) => UmbVariantId.FromString(x)) ?? [];
		}

		const saveData = this.#buildSaveData(variantIds);
		await this.#runMandatoryValidationForSaveData(saveData);

		// Create the validation repository if it does not exist. (we first create this here when we need it) [NL]
		this.#validationRepository ??= new UmbDocumentValidationRepository(this);

		// We ask the server first to get a concatenated set of validation messages. So we see both front-end and back-end validation messages [NL]
		if (this.getIsNew()) {
			const parent = this.#parent.getValue();
			if (!parent) throw new Error('Parent is not set');
			this.#serverValidation.askServerForValidation(
				saveData,
				this.#validationRepository.validateCreate(saveData, parent.unique),
			);
		} else {
			this.#serverValidation.askServerForValidation(saveData, this.#validationRepository.validateSave(saveData));
		}

		// TODO: Only validate the specified selection.. [NL]
		return this.validateAndSubmit(
			async () => {
				return this.#performSaveAndPublish(variantIds, saveData);
			},
			async () => {
				// If data of the selection is not valid Then just save:
				await this.#performSaveOrCreate(saveData);
				// Notifying that the save was successful, but we did not publish, which is what we want to symbolize here. [NL]
				const notificationContext = await this.getContext(UMB_NOTIFICATION_CONTEXT);
				// TODO: Get rid of the save notification.
				// TODO: Translate this message [NL]
				notificationContext.peek('danger', {
					data: { message: 'Document was not published, but we saved it for you.' },
				});
				// Reject even thought the save was successful, but we did not publish, which is what we want to symbolize here. [NL]
				return await Promise.reject();
			},
		);
	}

	async #runMandatoryValidationForSaveData(saveData: UmbDocumentDetailModel) {
		// Check that the data is valid before we save it.
		// Check variants have a name:
		const variantsWithoutAName = saveData.variants.filter((x) => !x.name);
		if (variantsWithoutAName.length > 0) {
			const validationContext = await this.getContext(UMB_VALIDATION_CONTEXT);
			variantsWithoutAName.forEach((variant) => {
				validationContext.messages.addMessage(
					'client',
					`$.variants[${UmbDataPathVariantQuery(variant)}].name`,
					UMB_VALIDATION_EMPTY_LOCALIZATION_KEY,
				);
			});
			throw new Error('All variants must have a name');
		}
	}

	async #performSaveAndPublish(variantIds: Array<UmbVariantId>, saveData: UmbDocumentDetailModel): Promise<void> {
		const unique = this.getUnique();
		if (!unique) throw new Error('Unique is missing');

		await this.#performSaveOrCreate(saveData);

		await this.publishingRepository.publish(
			unique,
			variantIds.map((variantId) => ({ variantId })),
		);

		const eventContext = await this.getContext(UMB_ACTION_EVENT_CONTEXT);
		const event = new UmbRequestReloadStructureForEntityEvent({
			unique: this.getUnique()!,
			entityType: this.getEntityType(),
		});

		eventContext.dispatchEvent(event);
	}

	async #handleSave() {
		const { options, selected } = await this.#determineVariantOptions();

		let variantIds: Array<UmbVariantId> = [];

		// If there is only one variant, we don't need to open the modal.
		if (options.length === 0) {
			throw new Error('No variants are available');
		} else if (options.length === 1) {
			// If only one option we will skip ahead and save the document with the only variant available:
			variantIds.push(UmbVariantId.Create(options[0]));
		} else {
			// If there are multiple variants, we will open the modal to let the user pick which variants to save.
			const modalManagerContext = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
			const result = await modalManagerContext
				.open(this, UMB_DOCUMENT_SAVE_MODAL, {
					data: {
						options,
						pickableFilter: this.#readOnlyLanguageVariantsFilter,
					},
					value: { selection: selected },
				})
				.onSubmit()
				.catch(() => undefined);

			if (!result?.selection.length) return;

			variantIds = result?.selection.map((x) => UmbVariantId.FromString(x)) ?? [];
		}

		const saveData = this.#buildSaveData(variantIds);
		await this.#runMandatoryValidationForSaveData(saveData);
		return await this.#performSaveOrCreate(saveData);
	}

	public override requestSubmit() {
		return this.#handleSave();
	}

	public submit() {
		return this.#handleSave();
	}
	public override invalidSubmit() {
		return this.#handleSave();
	}

	public async publish() {
		throw new Error('Method not implemented.');
	}

	public async saveAndPreview(): Promise<void> {
		return this.#handleSaveAndPreview();
	}

	public async saveAndPublish(): Promise<void> {
		return this.#handleSaveAndPublish();
	}

	public async schedule() {
		const { options, selected } = await this.#determineVariantOptions();

		const modalManagerContext = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
		const result = await modalManagerContext
			.open(this, UMB_DOCUMENT_SCHEDULE_MODAL, {
				data: {
					options,
					pickableFilter: this.#readOnlyLanguageVariantsFilter,
				},
				value: { selection: selected.map((unique) => ({ unique, schedule: {} })) },
			})
			.onSubmit()
			.catch(() => undefined);

		if (!result?.selection.length) return;

		// Map to the correct format for the API (UmbDocumentVariantPublishModel)
		const variants =
			result?.selection.map<UmbDocumentVariantPublishModel>((x) => ({
				variantId: UmbVariantId.FromString(x.unique),
				schedule: x.schedule,
			})) ?? [];

		if (!variants.length) return;

		const unique = this.getUnique();
		if (!unique) throw new Error('Unique is missing');
		await this.publishingRepository.publish(unique, variants);
	}

	public async unpublish() {
		const unique = this.getUnique();
		const entityType = this.getEntityType();
		if (!unique) throw new Error('Unique is missing');
		if (!entityType) throw new Error('Entity type is missing');

		// TODO: remove meta
		new UmbUnpublishDocumentEntityAction(this, { unique, entityType, meta: {} as never }).execute();
	}

	public async publishWithDescendants() {
		const { options, selected } = await this.#determineVariantOptions();

		const modalManagerContext = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
		const result = await modalManagerContext
			.open(this, UMB_DOCUMENT_PUBLISH_WITH_DESCENDANTS_MODAL, {
				data: {
					options,
					pickableFilter: this.#readOnlyLanguageVariantsFilter,
				},
				value: { selection: selected },
			})
			.onSubmit()
			.catch(() => undefined);

		if (!result?.selection.length) return;

		// Map to variantIds
		const variantIds = result?.selection.map((x) => UmbVariantId.FromString(x)) ?? [];

		if (!variantIds.length) return;

		const unique = this.getUnique();
		if (!unique) throw new Error('Unique is missing');
		await this.publishingRepository.publishWithDescendants(
			unique,
			variantIds,
			result.includeUnpublishedDescendants ?? false,
		);
	}

	async delete() {
		const id = this.getUnique();
		if (id) {
			await this.repository.delete(id);
		}
	}

	public createPropertyDatasetContext(
		host: UmbControllerHost,
		variantId: UmbVariantId,
	): UmbDocumentPropertyDatasetContext {
		return new UmbDocumentPropertyDatasetContext(host, this, variantId);
	}

	public override destroy(): void {
		this.#persistedData.destroy();
		this.#currentData.destroy();
		this.structure.destroy();
		this.#languageRepository.destroy();
		super.destroy();
	}
}

export default UmbDocumentWorkspaceContext;
