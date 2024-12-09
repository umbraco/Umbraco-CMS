import type { UmbPropertyValueData } from '../index.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { createExtensionApi } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';

export class UmbPropertyValueTransformController extends UmbControllerBase {
	/**
	 * Transforms the property data.
	 * @param {UmbPropertyValueData} property - The property data.
	 * @returns {Promise<UmbPropertyValueData>} - A promise that resolves to the transformed property data.
	 */
	async transform(property: UmbPropertyValueData): Promise<ModelType> {
		const result = this.#transformValue(property);

		this.destroy();

		return result;
	}

	async #transformValue(propertyData: UmbPropertyValueData): Promise<UmbPropertyValueData | undefined> {
		const editorAlias = propertyData?.editorAlias;
		if (!editorAlias) {
			console.error(`Editor alias not found for ${propertyData.alias}`);
			return propertyData;
		}

		// Transform the property value it self:

		let result = propertyData;

		// Transform sub values of this property:

		// Find the resolver for this editor alias:
		const manifest = umbExtensionsRegistry.getByTypeAndFilter(
			'propertyValueResolver',
			// TODO: Remove depcrated filter in v.17 [NL]
			(x) => x.forEditorAlias === editorAlias || x.meta?.editorAlias === editorAlias,
		)[0];

		if (!manifest) {
			// No resolver found, then we can continue using the draftValue as is.
			return result;
		}

		const api = await createExtensionApi(this, manifest);
		if (!api) {
			// If api is not to be found, then we can continue using the draftValue as is.
			return result;
		}

		if (api.processValues) {
			let valuesIndex = 0;
			result =
				(await api.processValues(result, async (values) => {
					// got some values (content and/or settings):
					// but how to get the persisted and the draft of this.....
					const persistedValues = persistedValuesHolder[valuesIndex++];

					return await this.#processValues(persistedValues);
				})) ?? result;
		}

		// the api did not provide a value processor, so we will return the draftValue:
		return result;
	}
}
