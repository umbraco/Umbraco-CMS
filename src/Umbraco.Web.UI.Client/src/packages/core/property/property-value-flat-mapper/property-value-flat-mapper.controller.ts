import type { UmbPropertyValueDataPotentiallyWithEditorAlias } from '../index.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { createExtensionApi } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';

export class UmbPropertyValueFlatMapperController extends UmbControllerBase {
	/**
	 * Maps the property values of the given property data.
	 * @param {UmbPropertyValueDataPotentiallyWithEditorAlias} property - The property data.
	 * @returns {Promise<UmbPropertyValueDataPotentiallyWithEditorAlias>} - A promise that resolves to the mapped data.
	 */
	async flatMap<ReturnType, ValueType>(
		property: UmbPropertyValueDataPotentiallyWithEditorAlias<ValueType>,
		mapper: (property: UmbPropertyValueDataPotentiallyWithEditorAlias) => ReturnType | Promise<ReturnType>,
	): Promise<Array<ReturnType>> {
		const result = await this.#mapValues<ReturnType, ValueType>(property, mapper);

		this.destroy();

		return result ?? [];
	}

	async #mapValues<ReturnType, ValueType>(
		incomingProperty: UmbPropertyValueDataPotentiallyWithEditorAlias<ValueType>,
		mapper: (property: UmbPropertyValueDataPotentiallyWithEditorAlias) => ReturnType | Promise<ReturnType>,
	): Promise<Array<ReturnType> | undefined> {
		const editorAlias = (incomingProperty as any).editorAlias as string | undefined;
		if (!editorAlias) {
			return undefined;
		}

		// Find the resolver for this editor alias:
		const manifest = umbExtensionsRegistry.getByTypeAndFilter(
			'propertyValueResolver',
			(x) => x.forEditorAlias === editorAlias,
		)[0];

		if (!manifest) {
			return undefined;
		}

		const api = await createExtensionApi(this, manifest);
		if (!api) {
			return undefined;
		}

		(api as any).manifest = manifest;

		const mapperOfThisProperty: ReturnType = await mapper(incomingProperty);

		if (api.processValues) {
			let mappedValues: Array<ReturnType> = [];
			// TODO: can we extract this method, to avoid it being recreated again and again [NL]
			await api.processValues(incomingProperty, async (properties) => {
				// Transform the values:
				for (const property of properties) {
					const incomingMapValues = await this.#mapValues(property, mapper);
					if (incomingMapValues) {
						mappedValues = mappedValues.concat(incomingMapValues);
					}
				}

				return undefined;
			});

			mappedValues.push(mapperOfThisProperty);

			return mappedValues;
		}

		// the api did not provide a value processor, so we will return the incoming property:
		return [mapperOfThisProperty];
	}
}
