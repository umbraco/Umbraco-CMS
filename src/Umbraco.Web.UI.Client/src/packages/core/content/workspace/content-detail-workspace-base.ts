import type { UmbContentDetailModel, UmbElementValueModel } from '../types.js';
import { UmbContentWorkspaceDataManager } from '../manager/index.js';
import { UmbMergeContentVariantDataController } from '../controller/merge-content-variant-data.controller.js';
import type { UmbContentVariantPickerData, UmbContentVariantPickerValue } from '../variant-picker/index.js';
import type { UmbContentPropertyDatasetContext } from '../property-dataset-context/index.js';
import type { UmbContentValidationRepository } from '../repository/content-validation-repository.interface.js';
import type { UmbContentWorkspaceContext } from './content-workspace-context.interface.js';
import { UmbContentDetailValidationPathTranslator } from './content-detail-validation-path-translator.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbDetailRepository, UmbDetailRepositoryConstructor } from '@umbraco-cms/backoffice/repository';
import {
	UmbEntityDetailWorkspaceContextBase,
	UmbWorkspaceSplitViewManager,
	type UmbEntityDetailWorkspaceContextArgs,
	type UmbEntityDetailWorkspaceContextCreateArgs,
} from '@umbraco-cms/backoffice/workspace';
import {
	UmbContentTypeStructureManager,
	type UmbContentTypeModel,
	type UmbPropertyTypeModel,
} from '@umbraco-cms/backoffice/content-type';
import {
	UMB_INVARIANT_CULTURE,
	UmbVariantId,
	type UmbEntityVariantModel,
	type UmbEntityVariantOptionModel,
} from '@umbraco-cms/backoffice/variant';
import { UmbDeprecation, UmbReadonlyVariantGuardManager } from '@umbraco-cms/backoffice/utils';
import { UmbDataTypeDetailRepository, UmbDataTypeItemRepositoryManager } from '@umbraco-cms/backoffice/data-type';
import { appendToFrozenArray, mergeObservables, UmbArrayState } from '@umbraco-cms/backoffice/observable-api';
import { UmbLanguageCollectionRepository, type UmbLanguageDetailModel } from '@umbraco-cms/backoffice/language';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import { firstValueFrom } from '@umbraco-cms/backoffice/external/rxjs';
import {
	UMB_VALIDATION_CONTEXT,
	UMB_VALIDATION_EMPTY_LOCALIZATION_KEY,
	UmbDataPathVariantQuery,
	UmbServerModelValidatorContext,
	UmbValidationController,
} from '@umbraco-cms/backoffice/validation';
import type { UmbModalToken } from '@umbraco-cms/backoffice/modal';
import { umbOpenModal } from '@umbraco-cms/backoffice/modal';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import {
	UmbEntityUpdatedEvent,
	UmbRequestReloadChildrenOfEntityEvent,
	UmbRequestReloadStructureForEntityEvent,
} from '@umbraco-cms/backoffice/entity-action';
import type { ClassConstructor } from '@umbraco-cms/backoffice/extension-api';
import {
	UmbPropertyValuePresetVariantBuilderController,
	UmbVariantPropertyGuardManager,
	type UmbPropertyTypePresetModel,
	type UmbPropertyTypePresetModelTypeModel,
} from '@umbraco-cms/backoffice/property';

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
	contentVariantScaffold: VariantModelType;
	contentTypePropertyName: string;
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
	implements UmbContentWorkspaceContext<DetailModelType, ContentTypeDetailModelType, VariantModelType>
{
	public readonly IS_CONTENT_WORKSPACE_CONTEXT = true as const;

	public readonly readonlyGuard = new UmbReadonlyVariantGuardManager(this);

	public readonly propertyViewGuard = new UmbVariantPropertyGuardManager(this);
	public readonly propertyWriteGuard = new UmbVariantPropertyGuardManager(this);
	public readonly propertyReadonlyGuard = new UmbVariantPropertyGuardManager(this);

	/* Content Data */
	protected override readonly _data = new UmbContentWorkspaceDataManager<DetailModelType, VariantModelType>(this);

	public override readonly data = this._data.current;
	public readonly values = this._data.createObservablePartOfCurrent((data) => data?.values);
	public readonly variants = this._data.createObservablePartOfCurrent((data) => data?.variants ?? []);
	public override readonly persistedData = this._data.persisted;

	/* Content Type (Structure) Data */
	public readonly structure;
	public readonly variesByCulture;
	public readonly variesBySegment;
	public readonly varies;

	abstract readonly contentTypeUnique: Observable<string | undefined>;

	/* Data Type */
	// This dataTypeItemManager is used to load the data type items for this content type, so we have all data-types for this content type up front. [NL]
	// But once we have a propert application cache this could be solved in a way where we ask the cache for the data type items. [NL]
	// And then we do not need to store them here in a local manager, but instead just request them here up-front and then again needed(which would get them from the cache, which as well could be update while this runs) [NL]
	readonly #dataTypeItemManager = new UmbDataTypeItemRepositoryManager(this);

	#varies?: boolean;
	#variesByCulture?: boolean;
	#variesBySegment?: boolean;

	/* Split View */
	readonly splitView = new UmbWorkspaceSplitViewManager();

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

	// eslint-disable-next-line @typescript-eslint/ban-ts-comment
	// @ts-ignore
	// TODO: fix type error
	public readonly variantOptions;

	#variantValidationContexts: Array<UmbValidationController> = [];
	getVariantValidationContext(variantId: UmbVariantId): UmbValidationController | undefined {
		return this.#variantValidationContexts.find((x) => x.getVariantId()?.compare(variantId));
	}

	#validateOnSubmit: boolean;
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

		this.#serverValidation.addPathTranslator(UmbContentDetailValidationPathTranslator);

		this._data.setVariantScaffold(args.contentVariantScaffold);
		this.#saveModalToken = args.saveModalToken;
		this.#contentTypePropertyName = args.contentTypePropertyName;

		const contentTypeDetailRepository = new args.contentTypeDetailRepository(this);
		this.#validationRepositoryClass = args.contentValidationRepository;
		this.#validateOnSubmit = args.skipValidationOnSubmit ? !args.skipValidationOnSubmit : true;
		this.structure = new UmbContentTypeStructureManager<ContentTypeDetailModelType>(this, contentTypeDetailRepository);
		this.variesByCulture = this.structure.ownerContentTypeObservablePart((x) => x?.variesByCulture);
		this.variesBySegment = this.structure.ownerContentTypeObservablePart((x) => x?.variesBySegment);
		this.varies = this.structure.ownerContentTypeObservablePart((x) =>
			x ? x.variesByCulture || x.variesBySegment : undefined,
		);

		this.variantOptions = mergeObservables(
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
						} as VariantOptionModelType;
					});
				} else if (varies === false) {
					return [
						{
							variant: variants.find((x) => x.culture === null),
							language: languages.find((x) => x.isDefault),
							culture: null,
							segment: null,
							unique: UMB_INVARIANT_CULTURE, // This must be a variantId string!
						} as VariantOptionModelType,
					];
				}
				return [] as Array<VariantOptionModelType>;
			},
		);

		this.observe(
			this.variantOptions,
			(variantOptions) => {
				variantOptions.forEach((variantOption) => {
					const missingThis = this.#variantValidationContexts.filter((x) => {
						const variantId = x.getVariantId();
						if (!variantId) return;
						return variantId.culture === variantOption.culture && variantId.segment === variantOption.segment;
					});
					if (missingThis) {
						const context = new UmbValidationController(this);
						context.inheritFrom(this.validationContext, '$');
						context.autoReport();
						context.setVariantId(UmbVariantId.Create(variantOption));
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
	}

	public async loadLanguages() {
		// TODO: If we don't end up having a Global Context for languages, then we should at least change this into using a asObservable which should be returned from the repository. [Nl]
		const { data } = await this.#languageRepository.requestCollection({});
		this.#languages.setValue(data?.items ?? []);
	}

	protected override async _scaffoldProcessData(data: DetailModelType): Promise<DetailModelType> {
		// Load the content type structure, usually this comes from the data, but in this case we are making the data, and we need this to be able to complete the data. [NL]
		await this.structure.loadType((data as any)[this.#contentTypePropertyName].unique);

		// Set culture and segment for all values:
		const cultures = this.#languages.getValue().map((x) => x.unique);

		if (this.structure.variesBySegment) {
			console.warn('Segments are not yet implemented for preset');
		}
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
		const options = await firstValueFrom(this.variantOptions);

		const activeVariants = this.splitView.getActiveVariants();
		const activeVariantIds = activeVariants.map((activeVariant) => UmbVariantId.Create(activeVariant));
		const changedVariantIds = this._data.getChangedVariants();
		const selectedVariantIds = activeVariantIds.concat(changedVariantIds);

		const writableSelectedVariantIds = selectedVariantIds.filter(
			(x) => this.readonlyGuard.getPermittedForVariant(x) === false,
		);

		// Selected can contain entries that are not part of the options, therefor the modal filters selection based on options.
		let selected = writableSelectedVariantIds.map((x) => x.toString()).filter((v, i, a) => a.indexOf(v) === i);

		return {
			options,
			selected,
		};
	}

	protected _saveableVariantsFilter = (option: VariantOptionModelType) => {
		return this.readonlyGuard.getPermittedForVariant(UmbVariantId.Create(option)) === false;
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
			throw new Error('All variants must have a name');
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
				const parent = this.getParent();
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
	 * Get the data to save
	 * @param {Array<UmbVariantId>} variantIds - The variant ids to save
	 * @returns {Promise<DetailModelType>}  {Promise<DetailModelType>}
	 * @memberof UmbContentDetailWorkspaceContextBase
	 */
	public constructSaveData(variantIds: Array<UmbVariantId>): Promise<DetailModelType> {
		return this._data.constructData(variantIds);
	}

	protected async _handleSubmit() {
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
			throw new Error('No variant picker modal token is set. There are multiple variants to save. Cannot proceed.');
		}

		const saveData = await this.constructSaveData(variantIds);
		await this.runMandatoryValidationForSaveData(saveData, variantIds);
		if (this.#validateOnSubmit) {
			await this.askServerToValidate(saveData, variantIds);
			return this.validateAndSubmit(
				async () => {
					return this.performCreateOrUpdate(variantIds, saveData);
				},
				async (reason?: any) => {
					return this.invalidSubmit(reason);
				},
			);
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

		const parent = this.getParent();
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

		const eventContext = await this.getContext(UMB_ACTION_EVENT_CONTEXT);
		if (!eventContext) {
			throw new Error('Event context is missing');
		}
		const event = new UmbRequestReloadChildrenOfEntityEvent({
			entityType: parent.entityType,
			unique: parent.unique,
		});
		eventContext.dispatchEvent(event);
		this.setIsNew(false);

		this._closeModal();
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

		this._closeModal();
	}

	override resetState() {
		super.resetState();
		this.readonlyGuard.clear();
		this.propertyViewGuard.clear();
		this.propertyWriteGuard.clear();
		this.propertyReadonlyGuard.clear();
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
