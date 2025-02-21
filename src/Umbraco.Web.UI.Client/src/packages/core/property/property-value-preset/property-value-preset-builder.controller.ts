import type { UmbPropertyValueData, UmbPropertyValueDataPotentiallyWithEditorAlias } from '../index.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { createExtensionApi } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import type {
	ManifestPropertyValuePreset,
	UmbPropertyTypePresetModel,
	UmbPropertyTypePresetWithSchemaAliasModel,
	UmbPropertyValuePresetApi,
} from './types.js';

//type PropertyTypesType = UmbPropertyTypePresetModel | UmbPropertyTypePresetWithSchemaAliasModel;

export class UmbPropertyValuePresetBuilderController extends UmbControllerBase {
	/**
	 * Clones the property data.
	 * @param {UmbPropertyValueDataPotentiallyWithEditorAlias} property - The property data.
	 * @returns {Promise<UmbPropertyValueDataPotentiallyWithEditorAlias>} - A promise that resolves to the cloned property data.
	 */
	async create<
		GivenPropertyTypesType extends UmbPropertyTypePresetModel | UmbPropertyTypePresetWithSchemaAliasModel,
		ReturnType = GivenPropertyTypesType extends UmbPropertyTypePresetWithSchemaAliasModel
			? UmbPropertyValueDataPotentiallyWithEditorAlias
			: UmbPropertyValueData,
	>(propertyTypes: Array<GivenPropertyTypesType>): Promise<Array<ReturnType>> {
		const result = await Promise.all(propertyTypes.map(this.#createPropertyPreset<ReturnType>));

		this.destroy();

		return result;
	}

	#createPropertyPreset = async <ReturnType>(
		propertyType: UmbPropertyTypePresetModel | UmbPropertyTypePresetWithSchemaAliasModel,
	): Promise<ReturnType> => {
		const editorAlias: string | undefined = (propertyType as UmbPropertyTypePresetWithSchemaAliasModel)
			.propertyEditorSchemaAlias;

		const editorUiAlias = propertyType.propertyEditorUiAlias;
		if (!editorUiAlias) {
			throw new Error(`propertyEditorUiAlias was not defined in ${propertyType}`);
		}

		const alias = propertyType.alias;
		if (!alias) {
			throw new Error(`alias not defined in ${propertyType}`);
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

		let value: unknown = undefined;
		// Important to use a inline for loop, to secure that each entry is processed(asynchronously) in order
		for (const api of apis) {
			if (!api.processValue) {
				throw new Error(
					`processValue method is not defined in one of the apis of these extensions: ${manifests.map((x) => x.alias).join(', ')}`,
				);
			}
			value = await api.processValue(value, propertyType.config);
		}

		if (editorAlias) {
			return {
				editorAlias,
				alias,
				value,
			} satisfies UmbPropertyValueDataPotentiallyWithEditorAlias as ReturnType;
		} else {
			return {
				alias,
				value,
			} satisfies UmbPropertyValueData as ReturnType;
		}
	};
}
