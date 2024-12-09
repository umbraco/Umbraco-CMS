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
	async transform(property: UmbPropertyValueData): Promise<UmbPropertyValueData> {
		const result = await this.#transformProperty(property);

		this.destroy();

		return result ?? property;
	}

	async #transformProperty(property: UmbPropertyValueData): Promise<UmbPropertyValueData> {
		const result = await this.#transformValue(property);
		return await this.#transformInnerValues(result);
	}

	async #transformValue(property: UmbPropertyValueData): Promise<UmbPropertyValueData> {
		// TODO: make a public type for UmbPropertyValueData with editorAlias property — one that is not the UmbPotentialContentValueModel, cause that is bound to content... [NL]

		const editorAlias = (property as any).editorAlias as string | undefined;
		if (!editorAlias) {
			console.error(`Editor alias not found for ${property.alias}`);
			return property;
		}

		// Find the transformer for this editor alias:
		const manifest = umbExtensionsRegistry.getByTypeAndFilter(
			'propertyValueTransformer',
			(x) => x.forEditorAlias === editorAlias,
		)[0];

		if (!manifest) {
			return property;
		}

		const api = await createExtensionApi(this, manifest);
		if (!api) {
			return property;
		}

		let result = property;

		if (api.transformValue) {
			const transformedValue = await api.transformValue(property.value);
			if (transformedValue) {
				result = { ...property, value: transformedValue };
			}
		}

		return result;
	}

	async #transformInnerValues(property: UmbPropertyValueData): Promise<UmbPropertyValueData> {
		const editorAlias = (property as any).editorAlias as string | undefined;
		if (!editorAlias) {
			return property;
		}

		// Find the resolver for this editor alias:
		const manifest = umbExtensionsRegistry.getByTypeAndFilter(
			'propertyValueResolver',
			// TODO: Remove depcrated filter option in v.17 [NL]
			(x) => x.forEditorAlias === editorAlias || x.meta?.editorAlias === editorAlias,
		)[0];

		if (!manifest) {
			return property;
		}

		const api = await createExtensionApi(this, manifest);
		if (!api) {
			return property;
		}

		if (api.processValues) {
			return (
				(await api.processValues(property, async (properties) => {
					// Transform the values:
					const persistedValues = await Promise.all(
						properties.map(async (value) => {
							return (await this.#transformProperty(value)) ?? value;
						}),
					);

					return persistedValues;
				})) ?? property
			);
		}

		// the api did not provide a value processor, so we will return the draftValue:
		return property;
	}
}
