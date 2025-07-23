import type { UmbBlockDataModel, UmbBlockDataValueModel, UmbBlockLayoutBaseModel } from '../types.js';
import { UmbBlockElementPropertyDatasetContext } from './block-element-property-dataset.context.js';
import type { UmbBlockWorkspaceContext } from './block-workspace.context.js';
import type { UmbContentTypeModel, UmbPropertyTypeModel } from '@umbraco-cms/backoffice/content-type';
import { UmbContentTypeStructureManager } from '@umbraco-cms/backoffice/content-type';
import {
	type Observable,
	UmbClassState,
	appendToFrozenArray,
	mergeObservables,
} from '@umbraco-cms/backoffice/observable-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { type UmbClassInterface, UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UmbDocumentTypeDetailRepository } from '@umbraco-cms/backoffice/document-type';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import { UmbValidationController } from '@umbraco-cms/backoffice/validation';
import { UmbElementWorkspaceDataManager, type UmbElementPropertyDataOwner } from '@umbraco-cms/backoffice/content';
import { UmbReadOnlyVariantGuardManager } from '@umbraco-cms/backoffice/utils';

import { UmbDataTypeItemRepositoryManager } from '@umbraco-cms/backoffice/data-type';
import { UmbVariantPropertyGuardManager } from '@umbraco-cms/backoffice/property';

export class UmbBlockElementManager<LayoutDataType extends UmbBlockLayoutBaseModel = UmbBlockLayoutBaseModel>
	extends UmbControllerBase
	implements UmbElementPropertyDataOwner<UmbBlockDataModel, UmbContentTypeModel>
{
	//

	readonly #data = new UmbElementWorkspaceDataManager<UmbBlockDataModel>(this);
	//#data = new UmbObjectState<UmbBlockDataModel | undefined>(undefined);
	readonly data = this.#data.current;
	#getDataPromise = new Promise<void>((resolve) => {
		this.#getDataResolver = resolve;
	});
	#getDataResolver!: () => void;

	// TODO: who is controlling this? We need to be aware about seperation of concerns. [NL]
	public readonly readOnlyGuard = new UmbReadOnlyVariantGuardManager(this);

	#variantId = new UmbClassState<UmbVariantId | undefined>(undefined);
	readonly variantId = this.#variantId.asObservable();

	readonly name;
	readonly getName;
	readonly unique = this.#data.createObservablePartOfCurrent((data) => data?.key);
	readonly contentTypeId = this.#data.createObservablePartOfCurrent((data) => data?.contentTypeKey);

	readonly values = this.#data.createObservablePartOfCurrent((data) => data?.values);
	getValues() {
		return this.#data.getCurrent()?.values;
	}

	readonly #dataTypeItemManager = new UmbDataTypeItemRepositoryManager(this);
	#dataTypeSchemaAliasMap = new Map<string, string>();

	readonly structure = new UmbContentTypeStructureManager<UmbContentTypeModel>(
		this,
		new UmbDocumentTypeDetailRepository(this),
	);

	public readonly propertyViewGuard = new UmbVariantPropertyGuardManager(this);
	public readonly propertyWriteGuard = new UmbVariantPropertyGuardManager(this);

	readonly validation = new UmbValidationController(this);

	constructor(host: UmbBlockWorkspaceContext<LayoutDataType>, dataPathPropertyName: string) {
		super(host);

		// Ugly, but we just inherit these from the workspace context: [NL]
		this.name = host.name;
		this.getName = host.getName;

		this.propertyViewGuard.fallbackToPermitted();
		this.propertyWriteGuard.fallbackToPermitted();

		this.observe(this.contentTypeId, (id) => {
			if (id) {
				this.structure.loadType(id);
			}
		});
		this.observe(this.unique, (key) => {
			if (key) {
				this.validation.setDataPath('$.' + dataPathPropertyName + `[?(@.key == '${key}')]`);
			}
		});

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
	}

	public isLoaded() {
		return this.#getDataPromise;
	}

	resetState() {
		this.#data.clear();
		this.propertyViewGuard.clearRules();
		this.propertyWriteGuard.clearRules();
		// default:
		this.propertyViewGuard.fallbackToPermitted();
		this.propertyWriteGuard.fallbackToPermitted();
	}

	setVariantId(variantId: UmbVariantId | undefined) {
		this.#variantId.setValue(variantId);
	}
	getVariantId(): UmbVariantId {
		return this.#variantId.getValue() ?? UmbVariantId.CreateInvariant();
	}

	// TODO: rename to currentData:
	setData(data: UmbBlockDataModel | undefined) {
		this.#data.setPersisted(data);
		this.#data.setCurrent(data);
		this.#getDataResolver();
	}

	getData() {
		return this.#data.getCurrent();
	}

	setPersistedData(data: UmbBlockDataModel | undefined) {
		this.#data.setPersisted(data);
	}

	/**
	 * Check if there are unpersisted changes.
	 * @returns { boolean } true if there are unpersisted changes.
	 */
	public getHasUnpersistedChanges(): boolean {
		return this.#data.getHasUnpersistedChanges();
	}

	getUnique() {
		return this.getData()?.key;
	}

	getEntityType() {
		return 'element';
	}

	getContentTypeId() {
		return this.getData()?.contentTypeKey;
	}

	#createPropertyVariantId(property: UmbPropertyTypeModel, variantId: UmbVariantId) {
		return variantId.toVariant(property.variesByCulture, property.variesBySegment);
	}

	// We will implement propertyAlias in the future, when implementing Varying Blocks. [NL]

	async propertyVariantId(propertyAlias: string) {
		return mergeObservables(
			[await this.structure.propertyStructureByAlias(propertyAlias), this.variantId],
			([property, variantId]) =>
				property && variantId ? this.#createPropertyVariantId(property, variantId) : undefined,
		);
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
		return this.#data.createObservablePartOfCurrent(
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
		const currentData = this.#data.getCurrent();
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

		// TODO: I think we should await this in the same way as we do for Content Detail Workspace Context. [NL]
		const editorAlias = this.#dataTypeSchemaAliasMap.get(property.dataType.unique);
		if (!editorAlias) {
			throw new Error(`Editor Alias of "${property.dataType.unique}" not found.`);
		}

		const entry = { editorAlias, ...variantId.toObject(), alias, value } as UmbBlockDataValueModel<ValueType>;

		const currentData = this.getData();
		if (currentData) {
			const values = appendToFrozenArray(
				currentData.values ?? [],
				entry,
				(x) => x.alias === alias && variantId!.compare(x),
			);
			this.#data.updateCurrent({ values });
		}
		this.finishPropertyValueChange();
	}

	initiatePropertyValueChange() {
		this.#data.initiatePropertyValueChange();
	}
	finishPropertyValueChange = () => {
		this.#data.finishPropertyValueChange();
	};

	public createPropertyDatasetContext(host: UmbControllerHost, variantId: UmbVariantId) {
		return new UmbBlockElementPropertyDatasetContext(host, this, variantId);
	}

	public setup(host: UmbClassInterface, variantId: UmbVariantId) {
		this.createPropertyDatasetContext(host, variantId);

		// Provide Validation Context for this view:
		this.validation.provideAt(host);
	}

	public override destroy(): void {
		this.structure.destroy();
		super.destroy();
	}
}

export default UmbBlockElementManager;
