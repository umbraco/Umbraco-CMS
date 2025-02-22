import type { UmbPropertyValueData, UmbPropertyValueDataPotentiallyWithEditorAlias } from '../index.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { createExtensionApi } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import type {
	ManifestPropertyValuePreset,
	UmbPropertyTypePresetModel,
	UmbPropertyTypePresetWithSchemaAliasModel,
	UmbPropertyValuePresetApi,
	UmbPropertyValuePresetApiCallArgs,
} from './types.js';

const EMPTY_CALL_ARGS = Object.freeze({});

export class UmbPropertyValuePresetBuilderController<
	ReturnType = UmbPropertyValueData | UmbPropertyValueDataPotentiallyWithEditorAlias,
> extends UmbControllerBase {
	/**
	 * Clones the property data.
	 * @param {UmbPropertyValueDataPotentiallyWithEditorAlias} propertyTypes - Data about the properties to make a preset for.
	 * @returns {Promise<UmbPropertyValueDataPotentiallyWithEditorAlias>} - A promise that resolves to the cloned property data.
	 */
	async create<GivenPropertyTypesType extends UmbPropertyTypePresetModel>(
		propertyTypes: Array<GivenPropertyTypesType>,
	): Promise<Array<ReturnType>> {
		const result = await Promise.all(propertyTypes.map(this.#createPropertyPreset));

		//Merge all the values into a single array:
		const values = result.flatMap((x) => x);

		this.destroy();

		return values;
	}

	#createPropertyPreset = async (
		propertyType: UmbPropertyTypePresetModel | UmbPropertyTypePresetWithSchemaAliasModel,
	): Promise<Array<ReturnType>> => {
		const editorAlias: string | undefined = (propertyType as UmbPropertyTypePresetWithSchemaAliasModel)
			.propertyEditorSchemaAlias;

		const editorUiAlias = propertyType.propertyEditorUiAlias;
		if (!editorUiAlias) {
			throw new Error(`propertyEditorUiAlias was not defined in ${propertyType}`);
		}

		let filter: (x: ManifestPropertyValuePreset) => boolean;
		if (editorAlias && editorUiAlias) {
			filter = (x) => x.forPropertyEditorSchemaAlias === editorAlias || x.forPropertyEditorUiAlias === editorUiAlias;
		} else {
			filter = (x) => x.forPropertyEditorUiAlias === editorUiAlias;
		}

		// Find a preset for this editor alias:
		const manifests = umbExtensionsRegistry.getByTypeAndFilter('propertyValuePreset', filter);

		const apis = (await Promise.all(manifests.map((x) => createExtensionApi(this, x)))).filter(
			(x) => x !== undefined,
		) as Array<UmbPropertyValuePresetApi>;

		const result = await this._generatePropertyValues(apis, propertyType);

		for (const api of apis) {
			api.destroy();
		}

		return result;
	};

	protected async _generatePropertyValues(
		apis: Array<UmbPropertyValuePresetApi>,
		propertyType: UmbPropertyTypePresetModel | UmbPropertyTypePresetWithSchemaAliasModel,
	): Promise<Array<ReturnType>> {
		const property = await this._generatePropertyValue(apis, propertyType, EMPTY_CALL_ARGS);
		return property ? [property] : [];
	}

	protected async _generatePropertyValue(
		apis: Array<UmbPropertyValuePresetApi>,
		propertyType: UmbPropertyTypePresetModel | UmbPropertyTypePresetWithSchemaAliasModel,
		callArgs: UmbPropertyValuePresetApiCallArgs,
	): Promise<ReturnType | undefined> {
		let value: unknown = undefined;
		// Important to use a inline for loop, to secure that each entry is processed(asynchronously) in order
		for (const api of apis) {
			if (!api.processValue) {
				throw new Error(`'processValue()' method is not defined in the api: ${api.constructor.name}`);
			}

			value = await api.processValue(value, propertyType.config, propertyType.typeArgs, callArgs);
		}

		if (!value) {
			return;
		}

		if ((propertyType as UmbPropertyTypePresetWithSchemaAliasModel).propertyEditorSchemaAlias) {
			return {
				editorAlias: (propertyType as UmbPropertyTypePresetWithSchemaAliasModel).propertyEditorSchemaAlias,
				alias: propertyType.alias,
				value,
			} satisfies UmbPropertyValueDataPotentiallyWithEditorAlias as ReturnType;
		} else {
			return {
				alias: propertyType.alias,
				value,
			} satisfies UmbPropertyValueData as ReturnType;
		}
	}
}
