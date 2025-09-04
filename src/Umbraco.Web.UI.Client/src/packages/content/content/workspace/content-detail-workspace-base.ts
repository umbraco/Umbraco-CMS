import type { UmbContentDetailModel, UmbElementValueModel } from '../types.js';
import { UmbContentCollectionManager } from '../collection/index.js';
import { UmbContentWorkspaceDataManager } from '../manager/index.js';
import { UmbMergeContentVariantDataController } from '../controller/merge-content-variant-data.controller.js';
import type { UmbContentVariantPickerData, UmbContentVariantPickerValue } from '../variant-picker/index.js';
import type { UmbContentPropertyDatasetContext } from '../property-dataset-context/index.js';
import type { UmbContentValidationRepository } from '../repository/content-validation-repository.interface.js';
import type { UmbContentCollectionWorkspaceContext } from '../collection/content-collection-workspace-context.interface.js';
import type { UmbContentWorkspaceContext } from './content-workspace-context.interface.js';
import { UmbContentDetailValidationPathTranslator } from './content-detail-validation-path-translator.js';
import { UmbContentValidationToHintsManager } from './content-validation-to-hints.manager.js';
import { appendToFrozenArray, mergeObservables, UmbArrayState } from '@umbraco-cms/backoffice/observable-api';
import { firstValueFrom, map } from '@umbraco-cms/backoffice/external/rxjs';
import { umbOpenModal } from '@umbraco-cms/backoffice/modal';
import { UmbContentTypeStructureManager } from '@umbraco-cms/backoffice/content-type';
import { UmbDataTypeDetailRepository, UmbDataTypeItemRepositoryManager } from '@umbraco-cms/backoffice/data-type';
import { UmbDeprecation, UmbReadOnlyVariantGuardManager } from '@umbraco-cms/backoffice/utils';
import { UmbEntityDetailWorkspaceContextBase, UmbWorkspaceSplitViewManager } from '@umbraco-cms/backoffice/workspace';
import {
	UmbEntityUpdatedEvent,
	UmbRequestReloadChildrenOfEntityEvent,
	UmbRequestReloadStructureForEntityEvent,
} from '@umbraco-cms/backoffice/entity-action';
import { UmbHintContext } from '@umbraco-cms/backoffice/hint';
import { UmbLanguageCollectionRepository } from '@umbraco-cms/backoffice/language';
import {
	UmbPropertyValuePresetVariantBuilderController,
	UmbVariantPropertyGuardManager,
} from '@umbraco-cms/backoffice/property';
import { UmbSegmentCollectionRepository } from '@umbraco-cms/backoffice/segment';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import {
	UMB_VALIDATION_CONTEXT,
	UMB_VALIDATION_EMPTY_LOCALIZATION_KEY,
	UmbDataPathVariantQuery,
	UmbServerModelValidatorContext,
	UmbValidationController,
} from '@umbraco-cms/backoffice/validation';
import type { ClassConstructor } from '@umbraco-cms/backoffice/extension-api';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import type { UmbContentTypeModel, UmbPropertyTypeModel } from '@umbraco-cms/backoffice/content-type';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbDetailRepository, UmbDetailRepositoryConstructor } from '@umbraco-cms/backoffice/repository';
import type {
	UmbEntityDetailWorkspaceContextArgs,
	UmbEntityDetailWorkspaceContextCreateArgs,
	UmbSaveableWorkspaceContext,
} from '@umbraco-cms/backoffice/workspace';
import type { UmbEntityVariantModel, UmbEntityVariantOptionModel } from '@umbraco-cms/backoffice/variant';
import type { UmbLanguageDetailModel } from '@umbraco-cms/backoffice/language';
import type { UmbPropertyTypePresetModel, UmbPropertyTypePresetModelTypeModel } from '@umbraco-cms/backoffice/property';
import type { UmbModalToken } from '@umbraco-cms/backoffice/modal';
import type { UmbSegmentCollectionItemModel } from '@umbraco-cms/backoffice/segment';
import type { UmbVariantHint } from '@umbraco-cms/backoffice/hint';

export interface UmbContentDetailWorkspaceContextArgs<
	DetailModelType extends UmbContentDetailModel<VariantModelType>,
	ContentTypeDetailModelType extends UmbContentTypeModel = UmbContentTypeModel,
	VariantModelType extends UmbEntityVariantModel = DetailModelType extends { variants: UmbEntityVariantModel[] }
		? DetailModelType['variants'][0]
		: never,
	VariantOptionModelType extends UmbEntityVariantOptionModel = UmbEntityVariantOptionModel<VariantModelType>,
> extends UmbEntityDetailWorkspaceContextArgs {
	contentTypeDetailRepository: UmbDetailRepositoryConstructor<ContentTypeDetailModelType>;
	contentValidationRepository?: ClassConstructor<UmbContentValidationRepository<DetailModelType>>;
	skipValidationOnSubmit?: boolean;
	ignoreValidationResultOnSubmit?: boolean;
	contentVariantScaffold: VariantModelType;
	contentTypePropertyName: string;
	collectionAlias?: string;
	saveModalToken?: UmbModalToken<UmbContentVariantPickerData<VariantOptionModelType>, UmbContentVariantPickerValue>;
}

/**
 * The base class for a content detail workspace context.
 * @exports
 * @abstract
 * @class UmbContentDetailWorkspaceContextBase
 * @augments {UmbEntityDetailWorkspaceContextBase<DetailModelType, DetailRepositoryType, CreateArgsType>}
 * @implements {UmbContentWorkspaceContext<DetailModelType, ContentTypeDetailModelType, VariantModelType>}
 * @template DetailModelType
 * @template DetailRepositoryType
 * @template ContentTypeDetailModelType
 * @template VariantModelType
 * @template VariantOptionModelType
 * @template CreateArgsType
 */
export abstract class UmbContentDetailWorkspaceContextBase<
		DetailModelType extends UmbContentDetailModel<VariantModelType>,
		DetailRepositoryType extends UmbDetailRepository<DetailModelType> = UmbDetailRepository<DetailModelType>,
		ContentTypeDetailModelType extends UmbContentTypeModel = UmbContentTypeModel,
		VariantModelType extends UmbEntityVariantModel = DetailModelType extends { variants: UmbEntityVariantModel[] }
			? DetailModelType['variants'][0]
			: never,
		VariantOptionModelType extends UmbEntityVariantOptionModel = UmbEntityVariantOptionModel<VariantModelType>,
		CreateArgsType extends
			UmbEntityDetailWorkspaceContextCreateArgs<DetailModelType> = UmbEntityDetailWorkspaceContextCreateArgs<DetailModelType>,
	>
	extends UmbEntityDetailWorkspaceContextBase<DetailModelType, DetailRepositoryType, CreateArgsType>
	implements
		UmbContentWorkspaceContext<DetailModelType, ContentTypeDetailModelType, VariantModelType>,
		UmbSaveableWorkspaceContext,
		UmbContentCollectionWorkspaceContext<ContentTypeDetailModelType>
{
	public readonly IS_CONTENT_WORKSPACE_CONTEXT = true as const;

	public readonly readOnlyGuard = new UmbReadOnlyVariantGuardManager(this);

	public readonly propertyViewGuard = new UmbVariantPropertyGuardManager(this);
	public readonly propertyWriteGuard = new UmbVariantPropertyGuardManager(this);

	/* Content Data */
	protected override readonly _data = new UmbContentWorkspaceDataManager<DetailModelType, VariantModelType>(this);

	public override readonly data = this._data.current;
	public readonly values = this._data.createObservablePartOfCurrent((data) => data?.values);
	public readonly variants = this._data.createObservablePartOfCurrent((data) => data?.variants ?? []);
	public override readonly persistedData = this._data.persisted;

	/* Content Type (Structure) Data */
	public readonly structure;
	public readonly variesByCulture: Observable<boolean | undefined>;
	public readonly variesBySegment: Observable<boolean | undefined>;
	public readonly varies: Observable<boolean | undefined>;

	abstract readonly contentTypeUnique: Observable<string | undefined>;

	/* Data Type */
	// This dataTypeItemManager is used to load the data type items for this content type, so we have all data-types for this content type up front. [NL]
	// But once we have a proper application cache this could be solved in a way where we ask the cache for the data type items. [NL]
	// And then we do not need to store them here in a local manager, but instead just request them here up-front and then again needed(which would get them from the cache, which as well could be update while this runs) [NL]
	readonly #dataTypeItemManager = new UmbDataTypeItemRepositoryManager(this);

	#varies?: boolean;
	#variesByCulture?: boolean;
	#variesBySegment?: boolean;

	/* Split View */
	readonly splitView = new UmbWorkspaceSplitViewManager();

	readonly collection: UmbContentCollectionManager;

	/* Hints */
	readonly hints = new UmbHintContext<UmbVariantHint>(this);

	/* Variant Options */
	// TODO: Optimize this so it uses either a App Language Context? [NL]
	#languageRepository = new UmbLanguageCollectionRepository(this);
	#languages = new UmbArrayState<UmbLanguageDetailModel>([], (x) => x.unique);
	/**
	 * @private
	 * @description - Should not be used by external code.
	 * @internal
	 */
	public readonly languages = this.#languages.asObservable();

	#segmentRepository = new UmbSegmentCollectionRepository(this);
	#segments = new UmbArrayState<UmbSegmentCollectionItemModel>([], (x) => x.unique);
	protected readonly _segments = this.#segments.asObservable();

	// eslint-disable-next-line @typescript-eslint/ban-ts-comment
	// @ts-ignore
	// TODO: fix type error
	public readonly variantOptions;
	// eslint-disable-next-line @typescript-eslint/no-unused-vars
	protected _variantOptionsFilter = (variantOption: VariantOptionModelType) => true;

	#variantValidationContexts: Array<UmbValidationController> = [];
	getVariantValidationContext(variantId: UmbVariantId): UmbValidationController | undefined {
		return this.#variantValidationContexts.find((x) => x.getVariantId()?.compare(variantId));
	}

	#validateOnSubmit: boolean;
	#ignoreValidationResultOnSubmit: boolean;
	#serverValidation = new UmbServerModelValidatorContext(this);
	#validationRepositoryClass?: ClassConstructor<UmbContentValidationRepository<DetailModelType>>;
	#validationRepository?: UmbContentValidationRepository<DetailModelType>;

	#saveModalToken?: UmbModalToken<UmbContentVariantPickerData<VariantOptionModelType>, UmbContentVariantPickerValue>;
	#contentTypePropertyName: string;

	constructor(
		host: UmbControllerHost,
		args: UmbContentDetailWorkspaceContextArgs<
			DetailModelType,
			ContentTypeDetailModelType,
			VariantModelType,
			VariantOptionModelType
		>,
	) {
		super(host, args);

		this.propertyViewGuard.fallbackToPermitted();
		this.propertyWriteGuard.fallbackToPermitted();

		this.#serverValidation.addPathTranslator(UmbContentDetailValidationPathTranslator);

		this._data.setVariantScaffold(args.contentVariantScaffold);
		this.#saveModalToken = args.saveModalToken;
		this.#contentTypePropertyName = args.contentTypePropertyName;

		const contentTypeDetailRepository = new args.contentTypeDetailRepository(this);
		this.#validationRepositoryClass = args.contentValidationRepository;
		this.#validateOnSubmit = args.skipValidationOnSubmit ? !args.skipValidationOnSubmit : true;
		this.#ignoreValidationResultOnSubmit = args.ignoreValidationResultOnSubmit ?? false;
		this.structure = new UmbContentTypeStructureManager<ContentTypeDetailModelType>(this, contentTypeDetailRepository);
		this.variesByCulture = this.structure.ownerContentTypeObservablePart((x) => x?.variesByCulture);
		this.variesBySegment = this.structure.ownerContentTypeObservablePart((x) => x?.variesBySegment);
		this.varies = this.structure.ownerContentTypeObservablePart((x) =>
			x ? x.variesByCulture || x.variesBySegment : undefined,
		);

		this.collection = new UmbContentCollectionManager<ContentTypeDetailModelType>(
			this,
			this.structure,
			args.collectionAlias,
		);

		new UmbContentValidationToHintsManager<ContentTypeDetailModelType>(
			this,
			this.structure,
			this.validationContext,
			this.hints,
		);

		this.variantOptions = mergeObservables(
			[this.variesByCulture, this.variesBySegment, this.variants, this.languages, this._segments],
			([variesByCulture, variesBySegment, variants, languages, segments]) => {
				if ((variesByCulture || variesBySegment) === undefined) {
					return [];
				}

				const varies = variesByCulture || variesBySegment;

				// No variation
				if (!varies) {
					return [
						{
							variant: variants.find((x) => new UmbVariantId(x.culture, x.segment).isInvariant()),
							language: languages.find((x) => x.isDefault),
							culture: null,
							segment: null,
							unique: new UmbVariantId().toString(),
						} as VariantOptionModelType,
					];
				}

				// Only culture variation
				if (variesByCulture && !variesBySegment) {
					return languages.map((language) => {
						return {
							variant: variants.find((x) => x.culture === language.unique),
							language,
							culture: language.unique,
							segment: null,
							unique: new UmbVariantId(language.unique).toString(),
						} as VariantOptionModelType;
					});
				}

				// Only segment variation
				if (!variesByCulture && variesBySegment) {
					const invariantCulture = {
						variant: variants.find((x) => new UmbVariantId(x.culture, x.segment).isInvariant()),
						language: languages.find((x) => x.isDefault),
						culture: null,
						segment: null,
						unique: new UmbVariantId().toString(),
					} as VariantOptionModelType;

					const segmentsForInvariantCulture = segments.map((segment) => {
						return {
							variant: variants.find((x) => x.culture === null && x.segment === segment.unique),
							language: languages.find((x) => x.isDefault),
							segmentInfo: segment,
							culture: null,
							segment: segment.unique,
							unique: new UmbVariantId(null, segment.unique).toString(),
						} as VariantOptionModelType;
					});

					return [invariantCulture, ...segmentsForInvariantCulture] as Array<VariantOptionModelType>;
				}

				// Culture and segment variation
				if (variesByCulture && variesBySegment) {
					return languages.flatMap((language) => {
						const culture = {
							variant: variants.find((x) => x.culture === language.unique),
							language,
							culture: language.unique,
							segment: null,
							unique: new UmbVariantId(language.unique).toString(),
						} as VariantOptionModelType;

						const segmentsForCulture = segments.map((segment) => {
							return {
								variant: variants.find((x) => x.culture === language.unique && x.segment === segment.unique),
								language,
								segmentInfo: segment,
								culture: language.unique,
								segment: segment.unique,
								unique: new UmbVariantId(language.unique, segment.unique).toString(),
							} as VariantOptionModelType;
						});

						return [culture, ...segmentsForCulture] as Array<VariantOptionModelType>;
					});
				}

				return [] as Array<VariantOptionModelType>;
			},
		).pipe(map((options) => options.filter((option) => this._variantOptionsFilter(option))));

		this.observe(
			this.variantOptions,
			(variantOptions) => {
				variantOptions.forEach((variantOption) => {
					const missingThis = !this.#variantValidationContexts.some((x) => {
						const variantId = x.getVariantId();
						if (!variantId) return;
						return variantId.culture === variantOption.culture && variantId.segment === variantOption.segment;
					});
					if (missingThis) {
						const context = new UmbValidationController(this);
						context.inheritFrom(this.validationContext, '$');
						context.setVariantId(UmbVariantId.Create(variantOption));
						context.autoReport();
						this.#variantValidationContexts.push(context);
					}
				});
			},
			null,
		);

		this.observe(
			this.varies,
			(varies) => {
				this._data.setVaries(varies);
				this.#varies = varies;
			},
			null,
		);
		this.observe(
			this.variesByCulture,
			(varies) => {
				this._data.setVariesByCulture(varies);
				this.#variesByCulture = varies;
			},
			null,
		);
		this.observe(
			this.variesBySegment,
			(varies) => {
				this._data.setVariesBySegment(varies);
				this.#variesBySegment = varies;
			},
			null,
		);
		this.observe(
			this.structure.contentTypeDataTypeUniques,
			(dataTypeUniques: Array<string>) => {
				this.#dataTypeItemManager.setUniques(dataTypeUniques);
			},
			null,
		);

		this.loadLanguages();
		this.#loadSegments();
	}

	public async loadLanguages() {
		// TODO: If we don't end up having a Global Context for languages, then we should at least change this into using a asObservable which should be returned from the repository. [Nl]
		const { data } = await this.#languageRepository.requestCollection({});
		this.#languages.setValue(data?.items ?? []);
	}

	async #loadSegments() {
		const { data } = await this.#segmentRepository.requestCollection({});
		this.#segments.setValue(data?.items ?? []);
	}

	protected override async _scaffoldProcessData(data: DetailModelType): Promise<DetailModelType> {
		// Load the content type structure, usually this comes from the data, but in this case we are making the data, and we need this to be able to complete the data. [NL]
		await this.structure.loadType((data as any)[this.#contentTypePropertyName].unique);

		/**
		 * TODO: Should we also set Preset Values when loading Content, because maybe content contains uncreated Cultures or Segments.
		 */

		// Set culture and segment for all values:
		const cultures = this.#languages.getValue().map((x) => x.unique);

		if (this.structure.variesBySegment) {
			console.warn('Segments are not yet implemented for preset');
		}
		// TODO: Add Segments for Presets:
		const segments: Array<string> | undefined = this.structure.variesBySegment ? [] : undefined;

		const repo = new UmbDataTypeDetailRepository(this);

		const propertyTypes = await this.structure.getContentTypeProperties();
		const valueDefinitions = await Promise.all(
			propertyTypes.map(async (property) => {
				// TODO: Implement caching for data-type requests. [NL]
				const dataType = (await repo.requestByUnique(property.dataType.unique)).data;
				// This means if its not loaded this will never resolve and the error below will never happen.
				if (!dataType) {
					throw new Error(`DataType of "${property.dataType.unique}" not found.`);
				}
				if (!dataType.editorUiAlias) {
					throw new Error(`DataType of "${property.dataType.unique}" did not have a editorUiAlias.`);
				}

				return {
					alias: property.alias,
					propertyEditorUiAlias: dataType.editorUiAlias,
					propertyEditorSchemaAlias: dataType.editorAlias,
					config: dataType.values,
					typeArgs: {
						variesByCulture: property.variesByCulture,
						variesBySegment: property.variesBySegment,
					} as UmbPropertyTypePresetModelTypeModel,
				} as UmbPropertyTypePresetModel;
			}),
		);

		const controller = new UmbPropertyValuePresetVariantBuilderController(this);
		controller.setCultures(cultures);
		if (segments) {
			controller.setSegments(segments);
		}

		const presetValues = await controller.create(valueDefinitions);

		// Don't just set the values, as we could have some already populated from a blueprint.
		// If we have a value from both a blueprint and a preset, use the latter as priority.
		const dataValues = [...data.values];
		for (let index = 0; index < presetValues.length; index++) {
			const presetValue = presetValues[index];
			const variantId = UmbVariantId.Create(presetValue);
			const matchingDataValueIndex = dataValues.findIndex((v) => v.alias === presetValue.alias && variantId.compare(v));
			if (matchingDataValueIndex > -1) {
				dataValues[matchingDataValueIndex] = presetValue;
			} else {
				dataValues.push(presetValue);
			}
		}

		data.values = dataValues;

		return data;
	}

	/**
	 * Get the name of a variant
	 * @param {UmbVariantId } [variantId] - The variant id
	 * @returns { string | undefined} - The name of the variant
	 * @memberof UmbContentDetailWorkspaceContextBase
	 */
	public getName(variantId?: UmbVariantId): string | undefined {
		const variants = this._data.getCurrent()?.variants;
		if (!variants) return;
		if (variantId) {
			return variants.find((x) => variantId.compare(x))?.name;
		} else {
			return variants[0]?.name;
		}
	}

	/**
	 * Set the name of a variant
	 * @param {string} name - The name of the variant
	 * @param {UmbVariantId} [variantId] - The variant id
	 * @memberof UmbContentDetailWorkspaceContextBase
	 */
	public setName(name: string, variantId?: UmbVariantId): void {
		// eslint-disable-next-line @typescript-eslint/ban-ts-comment
		// @ts-ignore
		// TODO: fix type error
		this._data.updateVariantData(variantId ?? UmbVariantId.CreateInvariant(), { name });
	}

	/**
	 * Get an observable for the name of a variant
	 * @param {UmbVariantId} [variantId] - The variant id
	 * @returns {Observable<string>} - The name of the variant
	 * @memberof UmbContentDetailWorkspaceContextBase
	 */
	public name(variantId?: UmbVariantId): Observable<string> {
		return this._data.createObservablePartOfCurrent(
			(data) => data?.variants?.find((x) => variantId?.compare(x))?.name ?? '',
		);
	}

	/* Variants */

	/**
	 * Get whether the content varies by culture
	 * @returns { boolean | undefined } - If the content varies by culture
	 * @memberof UmbContentDetailWorkspaceContextBase
	 */
	public getVariesByCulture(): boolean | undefined {
		return this.#variesByCulture;
	}

	/**
	 * Get whether the content varies by segment
	 * @returns {boolean | undefined} - If the content varies by segment
	 * @memberof UmbContentDetailWorkspaceContextBase
	 */
	public getVariesBySegment(): boolean | undefined {
		return this.#variesBySegment;
	}

	/**
	 * Get whether the content varies
	 * @returns { boolean | undefined } - If the content varies
	 * @memberof UmbContentDetailWorkspaceContextBase
	 */
	public getVaries(): boolean | undefined {
		return this.#varies;
	}

	/**
	 * Get the variant by the given variantId
	 * @param {UmbVariantId} variantId - The variant id
	 * @returns { Observable<VariantModelType | undefined> } - The variant or undefined if not found
	 * @memberof UmbContentDetailWorkspaceContextBase
	 */
	public variantById(variantId: UmbVariantId): Observable<VariantModelType | undefined> {
		return this._data.createObservablePartOfCurrent((data) => data?.variants?.find((x) => variantId.compare(x)));
	}

	/**
	 * Get the variant by the given variantId
	 * @param {UmbVariantId} variantId - The variant id
	 * @returns { VariantModelType | undefined } - The variant or undefined if not found
	 * @memberof UmbContentDetailWorkspaceContextBase
	 */
	public getVariant(variantId: UmbVariantId): VariantModelType | undefined {
		return this._data.getCurrent()?.variants?.find((x) => variantId.compare(x));
	}

	public getVariants(): Array<VariantModelType> | undefined {
		return this._data.getCurrent()?.variants;
	}

	/**
	 * Observe the property type
	 * @param {string} propertyId - The id of the property
	 * @returns {Promise<Observable<UmbPropertyTypeModel | undefined>>} - An observable for the property type
	 * @memberof UmbContentDetailWorkspaceContextBase
	 */
	public async propertyStructureById(propertyId: string): Promise<Observable<UmbPropertyTypeModel | undefined>> {
		return this.structure.propertyStructureById(propertyId);
	}

	/* Values */

	/**
	 * Get the values of the content
	 * @returns {Array<UmbElementValueModel> | undefined} - The values of the content
	 * @memberof UmbContentDetailWorkspaceContextBase
	 */
	public getValues(): Array<UmbElementValueModel> | undefined {
		return this._data.getCurrent()?.values;
	}

	/**
	 * @function propertyValueByAlias
	 * @param {string} propertyAlias - The alias of the property
	 * @param {UmbVariantId} variantId - The variant
	 * @returns {Promise<Observable<ReturnType | undefined> | undefined>} - An observable for the value of the property
	 * @description Get an Observable for the value of this property.
	 */
	public async propertyValueByAlias<PropertyValueType = unknown>(
		propertyAlias: string,
		variantId?: UmbVariantId,
	): Promise<Observable<PropertyValueType | undefined> | undefined> {
		return this._data.createObservablePartOfCurrent(
			(data) =>
				data?.values?.find((x) => x?.alias === propertyAlias && (variantId ? variantId.compare(x) : true))
					?.value as PropertyValueType,
		);
	}

	/**
	 * Get the current value of the property with the given alias and variantId.
	 * @param {string} alias - The alias of the property
	 * @param {UmbVariantId | undefined} variantId - The variant id of the property
	 * @returns {ReturnType | undefined} The value or undefined if not set or found.
	 */
	public getPropertyValue<ReturnType = unknown>(alias: string, variantId?: UmbVariantId) {
		const currentData = this._data.getCurrent();
		if (currentData) {
			const newDataSet = currentData.values?.find(
				(x) => x.alias === alias && (variantId ? variantId.compare(x) : true),
			);
			return newDataSet?.value as ReturnType;
		}
		return undefined;
	}

	/**
	 * Set the value of the property with the given alias and variantId.
	 * @template ValueType
	 * @param {string} alias - The alias of the property
	 * @param {ValueType} value - The value to set
	 * @param {UmbVariantId} [variantId] - The variant id of the property
	 * @memberof UmbContentDetailWorkspaceContextBase
	 */
	public async setPropertyValue<ValueType = unknown>(alias: string, value: ValueType, variantId?: UmbVariantId) {
		this.initiatePropertyValueChange();
		variantId ??= UmbVariantId.CreateInvariant();
		const property = await this.structure.getPropertyStructureByAlias(alias);

		if (!property) {
			throw new Error(`Property alias "${alias}" not found.`);
		}

		// the getItemByUnique is a async method that first resolves once the item is loaded.
		const editorAlias = (await this.#dataTypeItemManager.getItemByUnique(property.dataType.unique))
			.propertyEditorSchemaAlias;
		// This means if its not loaded this will never resolve and the error below will never happen.
		if (!editorAlias) {
			throw new Error(`Editor Alias of "${property.dataType.unique}" not found.`);
		}

		// Notice the order of the properties is important for our JSON String Compare function. [NL]
		const entry: UmbElementValueModel = {
			editorAlias,
			// Be aware that this solution is a bit magical, and based on a naming convention.
			// We might want to make this more flexible at some point and get the entityType from somewhere instead of constructing it here.
			entityType: `${this.getEntityType()}-property-value`,
			...variantId.toObject(),
			alias,
			value,
		};

		const currentData = this.getData();
		if (currentData) {
			const values = appendToFrozenArray(
				currentData.values ?? [],
				entry,
				(x) => x.alias === alias && variantId!.compare(x),
			);

			// eslint-disable-next-line @typescript-eslint/ban-ts-comment
			// @ts-ignore
			// TODO: fix type error
			this._data.updateCurrent({ values });

			// TODO: Ideally we should move this type of logic to the act of saving [NL]
			this._data.ensureVariantData(variantId);
		}
		this.finishPropertyValueChange();
	}

	public initiatePropertyValueChange() {
		this._data.initiatePropertyValueChange();
	}

	public finishPropertyValueChange = () => {
		this._data.finishPropertyValueChange();
	};

	/**
	 * Gets the changed variant ids
	 * @returns {Array<UmbVariantId>} - The changed variant ids
	 * @memberof UmbContentDetailWorkspaceContextBase
	 */
	public getChangedVariants(): Array<UmbVariantId> {
		return this._data.getChangedVariants();
	}

	protected async _determineVariantOptions(): Promise<{
		options: VariantOptionModelType[];
		selected: string[];
	}> {
		const options = (await firstValueFrom(this.variantOptions)).filter((option) => option.segment === null);

		const activeVariants = this.splitView.getActiveVariants();
		const activeVariantIds = activeVariants.map((activeVariant) => UmbVariantId.Create(activeVariant));
		const changedVariantIds = this._data.getChangedVariants();
		const activeAndChangedVariantIds = [...activeVariantIds, ...changedVariantIds];

		// if a segment has been changed, we select the "parent" culture variant as it is currently only possible to select between cultures in the dialogs
		const changedParentCultureVariantIds = activeAndChangedVariantIds
			.filter((x) => x.segment !== null)
			.map((x) => x.toSegmentInvariant());

		const selectedVariantIds = [...activeAndChangedVariantIds, ...changedParentCultureVariantIds];

		const writableSelectedVariantIds = selectedVariantIds.filter(
			(x) => this.readOnlyGuard.getIsPermittedForVariant(x) === false,
		);

		// Selected can contain entries that are not part of the options, therefor the modal filters selection based on options.
		const selected = writableSelectedVariantIds
			.map((variantId) => variantId.toString())
			.filter((variantId, index, all) => all.indexOf(variantId) === index);

		const uniqueSelected = [...new Set(selected)];

		return {
			options,
			selected: uniqueSelected,
		};
	}

	protected _saveableVariantsFilter = (option: VariantOptionModelType) => {
		return this.readOnlyGuard.getIsPermittedForVariant(UmbVariantId.Create(option)) === false;
	};

	/* validation */
	/**
	 * Run the mandatory validation for the save data
	 * @deprecated Use the public runMandatoryValidationForSaveData instead. Will be removed in v. 17.
	 * @protected
	 * @param {DetailModelType} saveData - The data to validate
	 * @memberof UmbContentDetailWorkspaceContextBase
	 */
	protected async _runMandatoryValidationForSaveData(saveData: DetailModelType, variantIds: Array<UmbVariantId> = []) {
		new UmbDeprecation({
			removeInVersion: '17',
			deprecated: '_runMandatoryValidationForSaveData',
			solution: 'Use the public runMandatoryValidationForSaveData instead.',
		}).warn();
		this.runMandatoryValidationForSaveData(saveData, variantIds);
	}

	/**
	 * Run the mandatory validation for the save data
	 * @param {DetailModelType} saveData - The data to validate
	 * @memberof UmbContentDetailWorkspaceContextBase
	 */
	public async runMandatoryValidationForSaveData(saveData: DetailModelType, variantIds: Array<UmbVariantId> = []) {
		// Check that the data is valid before we save it.
		// If we vary by culture then we do not want to validate the invariant variant.
		if (this.getVariesByCulture()) {
			variantIds = variantIds.filter((variant) => !variant.isCultureInvariant());
		}
		const missingVariants = variantIds.filter((variant) => {
			return !saveData.variants.some((y) => variant.compare(y));
		});
		if (missingVariants.length > 0) {
			throw new Error('One or more selected variants have not been created');
		}
		// Check variants have a name:
		const variantsWithoutAName = saveData.variants.filter((x) => !x.name);
		if (variantsWithoutAName.length > 0) {
			const validationContext = await this.getContext(UMB_VALIDATION_CONTEXT);
			if (!validationContext) {
				throw new Error('Validation context is missing');
			}
			variantsWithoutAName.forEach((variant) => {
				validationContext.messages.addMessage(
					'client',
					`$.variants[${UmbDataPathVariantQuery(variant)}].name`,
					UMB_VALIDATION_EMPTY_LOCALIZATION_KEY,
				);
			});
			throw new Error(
				'All variants must have a name, these variants are missing a name: ' +
					variantsWithoutAName.map((x) => (x.culture ?? 'invariant') + '_' + (x.segment ?? '')).join(', '),
			);
		}
	}

	/**
	 * Ask the server to validate the save data
	 * @param {DetailModelType} saveData - The data to validate
	 * @param {Array<UmbVariantId>} variantIds - The variant ids to validate
	 * @memberof UmbContentDetailWorkspaceContextBase
	 */
	public async askServerToValidate(saveData: DetailModelType, variantIds: Array<UmbVariantId>) {
		if (this.#validationRepositoryClass) {
			// Create the validation repository if it does not exist. (we first create this here when we need it) [NL]
			this.#validationRepository ??= new this.#validationRepositoryClass(this);

			// We ask the server first to get a concatenated set of validation messages. So we see both front-end and back-end validation messages [NL]
			if (this.getIsNew()) {
				const parent = this._internal_getCreateUnderParent();
				if (!parent) throw new Error('Parent is not set');
				await this.#serverValidation.askServerForValidation(
					saveData,
					this.#validationRepository.validateCreate(saveData, parent.unique),
				);
			} else {
				await this.#serverValidation.askServerForValidation(
					saveData,
					this.#validationRepository.validateSave(saveData, variantIds),
				);
			}
		}
	}

	/**
	 * Request a submit of the workspace, in the case of Document Workspaces the validation does not need to be valid for this to be submitted.
	 * @returns {Promise<void>} a promise which resolves once it has been completed.
	 */
	public override requestSubmit() {
		return this._handleSubmit();
	}

	public override submit() {
		return this._handleSubmit();
	}

	/**
	 * Request a save of the workspace, in the case of Document Workspaces the validation does not need to be valid for this to be saved.
	 * @returns {Promise<void>} a promise which resolves once it has been completed.
	 */
	public requestSave() {
		return this._handleSave();
	}

	/**
	 * Get the data to save
	 * @param {Array<UmbVariantId>} variantIds - The variant ids to save
	 * @returns {Promise<DetailModelType>}  {Promise<DetailModelType>}
	 * @memberof UmbContentDetailWorkspaceContextBase
	 */
	public constructSaveData(variantIds: Array<UmbVariantId>): Promise<DetailModelType> {
		return this._data.constructData(variantIds);
	}

	protected async _handleSubmit() {
		await this._handleSave();
		this._closeModal();
	}
	protected async _handleSave() {
		const data = this.getData();
		if (!data) {
			throw new Error('Data is missing');
		}

		const { options, selected } = await this._determineVariantOptions();

		let variantIds: Array<UmbVariantId> = [];

		// If there is only one variant, we don't need to open the modal.
		if (options.length === 0) {
			throw new Error('No variants are available');
		} else if (options.length === 1) {
			// If only one option we will skip ahead and save the content with the only variant available:
			variantIds.push(UmbVariantId.Create(options[0]));
		} else if (this.#saveModalToken) {
			// If there are multiple variants, we will open the modal to let the user pick which variants to save.
			const result = await umbOpenModal(this, this.#saveModalToken, {
				data: {
					options,
					pickableFilter: this._saveableVariantsFilter,
				},
				value: { selection: selected },
			}).catch(() => undefined);

			if (!result?.selection.length) return;

			variantIds = result?.selection.map((x) => UmbVariantId.FromString(x)) ?? [];
		} else {
			/* If there are multiple variants but no modal token is set
			we will save the variants that would have been preselected in the modal.
			These are based on the variants that have been edited */
			variantIds = selected.map((x) => UmbVariantId.FromString(x));
		}

		const saveData = await this.constructSaveData(variantIds);

		await this.runMandatoryValidationForSaveData(saveData, variantIds);
		if (this.#validateOnSubmit) {
			await this.askServerToValidate(saveData, variantIds);
			const valid = await this._validateAndLog().then(
				() => true,
				() => false,
			);
			if (valid || this.#ignoreValidationResultOnSubmit) {
				return this.performCreateOrUpdate(variantIds, saveData);
			}
		} else {
			await this.performCreateOrUpdate(variantIds, saveData);
		}
	}

	/**
	 * Perform the create or update of the content
	 * @deprecated Use the public performCreateOrUpdate instead. Will be removed in v. 17.
	 * @protected
	 * @param {Array<UmbVariantId>} variantIds
	 * @param {DetailModelType} saveData
	 * @memberof UmbContentDetailWorkspaceContextBase
	 */
	protected async _performCreateOrUpdate(variantIds: Array<UmbVariantId>, saveData: DetailModelType) {
		await this.performCreateOrUpdate(variantIds, saveData);
	}

	/**
	 * Perform the create or update of the content
	 * @param {Array<UmbVariantId>} variantIds - The variant ids to save
	 * @param {DetailModelType} saveData - The data to save
	 * @memberof UmbContentDetailWorkspaceContextBase
	 */
	public async performCreateOrUpdate(variantIds: Array<UmbVariantId>, saveData: DetailModelType) {
		if (this.getIsNew()) {
			await this.#create(variantIds, saveData);
		} else {
			await this.#update(variantIds, saveData);
		}
	}

	async #create(variantIds: Array<UmbVariantId>, saveData: DetailModelType) {
		if (!this._detailRepository) throw new Error('Detail repository is not set');

		const parent = this._internal_getCreateUnderParent();
		if (!parent) throw new Error('Parent is not set');

		const { data, error } = await this._detailRepository.create(saveData, parent.unique);
		if (!data || error) {
			throw new Error('Error creating content');
		}

		const variantIdsIncludingInvariant = [...variantIds, UmbVariantId.CreateInvariant()];

		// Only update the variants that was chosen to be saved:
		const persistedData = this._data.getCurrent();
		const newPersistedData = await new UmbMergeContentVariantDataController(this).process(
			persistedData,
			data,
			variantIds,
			variantIdsIncludingInvariant,
		);
		this._data.setPersisted(newPersistedData);

		// Only update the variants that was chosen to be saved:
		const currentData = this._data.getCurrent();
		const newCurrentData = await new UmbMergeContentVariantDataController(this).process(
			currentData,
			data,
			variantIds,
			variantIdsIncludingInvariant,
		);
		this._data.setCurrent(newCurrentData);
		this.setIsNew(false);

		const eventContext = await this.getContext(UMB_ACTION_EVENT_CONTEXT);
		if (!eventContext) {
			throw new Error('Event context is missing');
		}

		const reloadStructureEvent = new UmbRequestReloadStructureForEntityEvent({
			entityType: parent.entityType,
			unique: parent.unique,
		});

		eventContext.dispatchEvent(reloadStructureEvent);

		const reloadChildrenEvent = new UmbRequestReloadChildrenOfEntityEvent({
			entityType: parent.entityType,
			unique: parent.unique,
		});

		eventContext.dispatchEvent(reloadChildrenEvent);
	}

	async #update(variantIds: Array<UmbVariantId>, saveData: DetailModelType) {
		if (!this._detailRepository) throw new Error('Detail repository is not set');

		const { data, error } = await this._detailRepository.save(saveData);
		if (!data || error) {
			throw new Error('Error saving content');
		}

		const variantIdsIncludingInvariant = [...variantIds, UmbVariantId.CreateInvariant()];

		// Only update the variants that was chosen to be saved:
		const persistedData = this._data.getCurrent();
		const newPersistedData = await new UmbMergeContentVariantDataController(this).process(
			persistedData,
			data,
			variantIds,
			variantIdsIncludingInvariant,
		);
		this._data.setPersisted(newPersistedData);

		// Only update the variants that was chosen to be saved:
		const currentData = this._data.getCurrent();
		const newCurrentData = await new UmbMergeContentVariantDataController(this).process(
			currentData,
			data,
			variantIds,
			variantIdsIncludingInvariant,
		);
		this._data.setCurrent(newCurrentData);

		const unique = this.getUnique()!;
		const entityType = this.getEntityType();

		const eventContext = await this.getContext(UMB_ACTION_EVENT_CONTEXT);
		if (!eventContext) {
			throw new Error('Event context is missing');
		}
		const structureEvent = new UmbRequestReloadStructureForEntityEvent({ unique, entityType });
		eventContext.dispatchEvent(structureEvent);

		const updatedEvent = new UmbEntityUpdatedEvent({
			unique,
			entityType,
			eventUnique: this._workspaceEventUnique,
		});

		eventContext.dispatchEvent(updatedEvent);
	}

	override resetState() {
		super.resetState();
		this.structure.clear();
		this.readOnlyGuard.clearRules();
		this.propertyViewGuard.clearRules();
		this.propertyWriteGuard.clearRules();
		// default:
		this.propertyViewGuard.fallbackToPermitted();
		this.propertyWriteGuard.fallbackToPermitted();
	}

	abstract getContentTypeUnique(): string | undefined;

	abstract createPropertyDatasetContext(
		host: UmbControllerHost,
		variantId: UmbVariantId,
	): UmbContentPropertyDatasetContext<DetailModelType, ContentTypeDetailModelType, VariantModelType>;

	public override destroy(): void {
		this.structure.destroy();
		this.#languageRepository.destroy();
		super.destroy();
	}
}
