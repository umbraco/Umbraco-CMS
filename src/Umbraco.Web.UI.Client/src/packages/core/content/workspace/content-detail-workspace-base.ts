import type { UmbContentDetailModel } from '../types.js';
import { UmbContentWorkspaceDataManager } from '../manager/index.js';
import type { UmbContentWorkspaceContext } from './content-workspace-context.interface.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbDetailRepository } from '@umbraco-cms/backoffice/repository';
import {
	UmbEntityDetailWorkspaceContextBase,
	UmbWorkspaceSplitViewManager,
	type UmbEntityDetailWorkspaceContextArgs,
	type UmbEntityDetailWorkspaceContextCreateArgs,
} from '@umbraco-cms/backoffice/workspace';
import { UmbContentTypeStructureManager, type UmbContentTypeModel } from '@umbraco-cms/backoffice/content-type';
import { UMB_INVARIANT_CULTURE, UmbVariantId, type UmbEntityVariantModel } from '@umbraco-cms/backoffice/variant';
import { UmbReadOnlyVariantStateManager } from '@umbraco-cms/backoffice/utils';
import { UmbDataTypeItemRepositoryManager } from '@umbraco-cms/backoffice/data-type';
import { appendToFrozenArray, mergeObservables, UmbArrayState } from '@umbraco-cms/backoffice/observable-api';
import { UmbLanguageCollectionRepository, type UmbLanguageDetailModel } from '@umbraco-cms/backoffice/language';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import { firstValueFrom, map } from '@umbraco-cms/backoffice/external/rxjs';
import {
	UmbValidationContext,
	UmbVariantsValidationPathTranslator,
	UmbVariantValuesValidationPathTranslator,
} from '@umbraco-cms/backoffice/validation';

export interface UmbContentDetailWorkspaceContextArgs extends UmbEntityDetailWorkspaceContextArgs {
	contentTypeDetailRepository: UmbDetailRepository;
}

export abstract class UmbContentDetailWorkspaceBase<
		DetailModelType extends UmbContentDetailModel,
		DetailRepositoryType extends UmbDetailRepository<DetailModelType> = UmbDetailRepository<DetailModelType>,
		ContentTypeDetailModel extends UmbContentTypeModel = UmbContentTypeModel,
		VariantModelType extends UmbEntityVariantModel = UmbEntityVariantModel,
		CreateArgsType extends
			UmbEntityDetailWorkspaceContextCreateArgs<DetailModelType> = UmbEntityDetailWorkspaceContextCreateArgs<DetailModelType>,
	>
	extends UmbEntityDetailWorkspaceContextBase<DetailModelType, DetailRepositoryType, CreateArgsType>
	implements UmbContentWorkspaceContext<DetailModelType, ContentTypeDetailModel, VariantModelType>
{
	public readonly IS_CONTENT_WORKSPACE_CONTEXT = true as const;

	protected override readonly _data = new UmbContentWorkspaceDataManager<DetailModelType>(
		this,
		UMB_DOCUMENT_DETAIL_MODEL_VARIANT_SCAFFOLD,
	);

	public readonly readOnlyState = new UmbReadOnlyVariantStateManager(this);

	/* Content Data */
	public readonly values = this._data.createObservablePartOfCurrent((data) => data?.values);
	public readonly variants = this._data.createObservablePartOfCurrent((data) => data?.variants ?? []);

	/* Content Type (Structure) Data */
	public readonly structure: UmbContentTypeStructureManager;

	public readonly variesByCulture = this.structure.ownerContentTypeObservablePart((x) => x?.variesByCulture);
	public readonly variesBySegment = this.structure.ownerContentTypeObservablePart((x) => x?.variesBySegment);
	public readonly varies = this.structure.ownerContentTypeObservablePart((x) =>
		x ? x.variesByCulture || x.variesBySegment : undefined,
	);

	/* Data Type */
	readonly #dataTypeItemManager = new UmbDataTypeItemRepositoryManager(this);
	#dataTypeSchemaAliasMap = new Map<string, string>();

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
	 */
	public readonly languages = this.#languages.asObservable();

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

	constructor(host: UmbControllerHost, args: UmbContentDetailWorkspaceContextArgs) {
		super(host, args);

		const contentTypeDetailRepository = new args.contentTypeDetailRepository(this);
		this.structure = new UmbContentTypeStructureManager(this, contentTypeDetailRepository);

		this.addValidationContext(new UmbValidationContext(this));
		new UmbVariantValuesValidationPathTranslator(this);
		new UmbVariantsValidationPathTranslator(this);

		this.observe(this.contentTypeUnique, (unique) => this.structure.loadType(unique), null);

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
		this.observe(
			this.#dataTypeItemManager.items,
			(dataTypes) => {
				// Make a map of the data type unique and editorAlias:
				this.#dataTypeSchemaAliasMap = new Map(
					dataTypes.map((dataType) => {
						return [dataType.unique, dataType.propertyEditorSchemaAlias];
					}),
				);
			},
			null,
		);

		this.loadLanguages();
	}

	async loadLanguages() {
		// TODO: If we don't end up having a Global Context for languages, then we should at least change this into using a asObservable which should be returned from the repository. [Nl]
		const { data } = await this.#languageRepository.requestCollection({});
		this.#languages.setValue(data?.items ?? []);
	}

	getName(variantId?: UmbVariantId) {
		const variants = this._data.getCurrent()?.variants;
		if (!variants) return;
		if (variantId) {
			return variants.find((x) => variantId.compare(x))?.name;
		} else {
			return variants[0]?.name;
		}
	}

	setName(name: string, variantId?: UmbVariantId) {
		this._data.updateVariantData(variantId ?? UmbVariantId.CreateInvariant(), { name });
	}

	name(variantId?: UmbVariantId) {
		return this._data.createObservablePartOfCurrent(
			(data) => data?.variants?.find((x) => variantId?.compare(x))?.name ?? '',
		);
	}

	/* Variants */

	getVariesByCulture() {
		return this.#variesByCulture;
	}

	getVariesBySegment() {
		return this.#variesBySegment;
	}

	getVaries() {
		return this.#varies;
	}

	variantById(variantId: UmbVariantId) {
		return this._data.createObservablePartOfCurrent((data) => data?.variants?.find((x) => variantId.compare(x)));
	}

	getVariant(variantId: UmbVariantId) {
		return this._data.getCurrent()?.variants?.find((x) => variantId.compare(x));
	}

	async propertyStructureById(propertyId: string) {
		return this.structure.propertyStructureById(propertyId);
	}

	/* Values */

	getValues() {
		return this._data.getCurrent()?.values;
	}

	/**
	 * @function propertyValueByAlias
	 * @param {string} propertyAlias - The alias of the property
	 * @param {UmbVariantId} variantId - The variant
	 * @returns {Promise<Observable<ReturnType | undefined> | undefined>} - An observable for the value of the property
	 * @description Get an Observable for the value of this property.
	 */
	async propertyValueByAlias<PropertyValueType = unknown>(
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
	getPropertyValue<ReturnType = unknown>(alias: string, variantId?: UmbVariantId) {
		const currentData = this._data.getCurrent();
		if (currentData) {
			const newDataSet = currentData.values?.find(
				(x) => x.alias === alias && (variantId ? variantId.compare(x) : true),
			);
			return newDataSet?.value as ReturnType;
		}
		return undefined;
	}

	async setPropertyValue<ValueType = unknown>(alias: string, value: ValueType, variantId?: UmbVariantId) {
		this.initiatePropertyValueChange();
		variantId ??= UmbVariantId.CreateInvariant();
		const property = await this.structure.getPropertyStructureByAlias(alias);

		if (!property) {
			throw new Error(`Property alias "${alias}" not found.`);
		}

		const editorAlias = this.#dataTypeSchemaAliasMap.get(property.dataType.unique);
		if (!editorAlias) {
			throw new Error(`Editor Alias of "${property.dataType.unique}" not found.`);
		}

		const entry = { ...variantId.toObject(), alias, editorAlias, value } as UmbDocumentValueModel<ValueType>;

		const currentData = this.getData();
		if (currentData) {
			const values = appendToFrozenArray(
				currentData.values ?? [],
				entry,
				(x) => x.alias === alias && variantId!.compare(x),
			);
			this._data.updateCurrent({ values });

			// TODO: We should move this type of logic to the act of saving [NL]
			this._data.ensureVariantData(variantId);
		}
		this.finishPropertyValueChange();
	}

	initiatePropertyValueChange() {
		this._data.initiatePropertyValueChange();
	}

	finishPropertyValueChange = () => {
		this._data.finishPropertyValueChange();
	};

	protected async _determineVariantOptions() {
		const options = await firstValueFrom(this.variantOptions);

		const activeVariants = this.splitView.getActiveVariants();
		const activeVariantIds = activeVariants.map((activeVariant) => UmbVariantId.Create(activeVariant));
		const changedVariantIds = this._data.getChangedVariants();
		const selectedVariantIds = activeVariantIds.concat(changedVariantIds);

		// Selected can contain entries that are not part of the options, therefor the modal filters selection based on options.
		const readOnlyCultures = this.readOnlyState.getStates().map((s) => s.variantId.culture);
		let selected = selectedVariantIds.map((x) => x.toString()).filter((v, i, a) => a.indexOf(v) === i);
		selected = selected.filter((x) => readOnlyCultures.includes(x) === false);

		return {
			options,
			selected,
		};
	}

	public override destroy(): void {
		this.structure.destroy();
		this.#languageRepository.destroy();
		super.destroy();
	}
}
