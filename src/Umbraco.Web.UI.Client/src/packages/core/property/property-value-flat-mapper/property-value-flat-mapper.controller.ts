import type { UmbPropertyValueDataPotentiallyWithEditorAlias } from '../index.js';
import type { ManifestPropertyValueResolver } from '../property-value-resolver/property-value-resolver.extension.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { createExtensionApi } from '@umbraco-cms/backoffice/extension-api';
import { UmbExtensionsManifestCacheIndex } from '@umbraco-cms/backoffice/extension-registry';

export class UmbPropertyValueFlatMapperController extends UmbControllerBase {
	#resolvers = new UmbExtensionsManifestCacheIndex<ManifestPropertyValueResolver>(
		this,
		'propertyValueResolver',
		(manifest, editorAlias) => manifest.forEditorAlias === editorAlias,
	);

	/**
	 * Maps the property values of the given property data.
	 * @template ValueType
	 * @template {UmbPropertyValueDataPotentiallyWithEditorAlias<ValueType>} PropertyType
	 * @param {UmbPropertyValueDataPotentiallyWithEditorAlias} property - The property data.
	 * @param {(property: PropertyType) => ReturnType | Promise<ReturnType>} mapper - The function used to map each property.
	 * @returns {Promise<UmbPropertyValueDataPotentiallyWithEditorAlias>} - A promise that resolves to the mapped data.
	 */
	async flatMap<ReturnType, ValueType, PropertyType extends UmbPropertyValueDataPotentiallyWithEditorAlias<ValueType>>(
		property: PropertyType,
		mapper: (property: PropertyType) => ReturnType | Promise<ReturnType>,
	): Promise<Array<ReturnType>> {
		const result = await this.#mapValues<ReturnType, ValueType, PropertyType>(property, mapper);

		this.destroy();

		return result ?? [];
	}

	async #mapValues<
		ReturnType,
		ValueType,
		PropertyType extends UmbPropertyValueDataPotentiallyWithEditorAlias<ValueType>,
	>(
		incomingProperty: PropertyType,
		mapper: (property: PropertyType) => ReturnType | Promise<ReturnType>,
	): Promise<Array<ReturnType> | undefined> {
		const mapOfThisProperty: ReturnType = await mapper(incomingProperty);

		const editorAlias = (incomingProperty as any).editorAlias as string | undefined;
		if (!editorAlias) {
			return [mapOfThisProperty];
		}
		const manifest = this.#resolvers.getManifests(editorAlias)[0];
		if (!manifest) {
			return [mapOfThisProperty];
		}

		const api = await createExtensionApi(this, manifest);
		if (!api) {
			return [mapOfThisProperty];
		}

		// TODO: Setting the manifest on the api is legacy, the implementation should not have to do this. [NL]
		api.manifest = manifest;

		if (api.processValues) {
			let mappedValues: Array<ReturnType> = [];
			await api.processValues(incomingProperty, async (properties) => {
				// Transform the values:
				for (const property of properties) {
					const incomingMapValues = await this.#mapValues(property, mapper);
					if (incomingMapValues) {
						mappedValues = mappedValues.concat(incomingMapValues);
					}
				}

				return properties;
			});

			// push in the front of the mapped values, so that the outer property is first in the array:
			mappedValues.splice(0, 0, mapOfThisProperty);

			return mappedValues;
		}

		// the api did not provide a value processor, so we will return the incoming property:
		return [mapOfThisProperty];
	}
}
