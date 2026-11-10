import type { UmbPropertyValueDataPotentiallyWithEditorAlias } from '../index.js';
import type { ManifestPropertyValueResolver } from '../property-value-resolver/property-value-resolver.extension.js';
import type { ManifestPropertyValueCloner } from './property-value-cloner.extension.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { createExtensionApi } from '@umbraco-cms/backoffice/extension-api';
import { UmbExtensionsManifestCacheIndex } from '@umbraco-cms/backoffice/extension-registry';

export class UmbPropertyValueCloneController extends UmbControllerBase {
	#cloners = new UmbExtensionsManifestCacheIndex<ManifestPropertyValueCloner>(
		this,
		'propertyValueCloner',
		(manifest, editorAlias) => manifest.forEditorAlias === editorAlias,
	);
	#resolvers = new UmbExtensionsManifestCacheIndex<ManifestPropertyValueResolver>(
		this,
		'propertyValueResolver',
		(manifest, editorAlias) => manifest.forEditorAlias === editorAlias,
	);

	/**
	 * Clones the property data.
	 * @param {UmbPropertyValueDataPotentiallyWithEditorAlias} property - The property data.
	 * @returns {Promise<UmbPropertyValueDataPotentiallyWithEditorAlias>} - A promise that resolves to the cloned property data.
	 */
	async clone<ValueType = unknown>(
		property: UmbPropertyValueDataPotentiallyWithEditorAlias<ValueType>,
	): Promise<UmbPropertyValueDataPotentiallyWithEditorAlias<ValueType>> {
		const result = await this.#cloneProperty<ValueType>(property);

		this.destroy();

		return result ?? property;
	}

	async #cloneProperty<ValueType>(
		property: UmbPropertyValueDataPotentiallyWithEditorAlias<ValueType>,
	): Promise<UmbPropertyValueDataPotentiallyWithEditorAlias<ValueType>> {
		const clonedProperty = await this.#cloneValue(property);
		return await this.#cloneInnerValues<ValueType>(clonedProperty);
	}

	async #cloneValue<ValueType>(
		incomingProperty: UmbPropertyValueDataPotentiallyWithEditorAlias<ValueType>,
	): Promise<UmbPropertyValueDataPotentiallyWithEditorAlias<ValueType>> {
		const editorAlias = (incomingProperty as any).editorAlias as string | undefined;
		if (!editorAlias) {
			console.error(`Editor alias not found for ${incomingProperty.alias}`);
			return incomingProperty;
		}

		const manifest = this.#cloners.getManifests(editorAlias)[0];
		if (!manifest) {
			return incomingProperty;
		}

		const api = await createExtensionApi(this, manifest);
		if (!api) {
			return incomingProperty;
		}

		// TODO: Setting the manifest on the api is legacy, the implementation should not have to do this. [NL]
		(api as { manifest?: ManifestPropertyValueCloner }).manifest = manifest;

		let clonedProperty = incomingProperty;

		if (api.cloneValue) {
			const clonedValue = await api.cloneValue(incomingProperty.value);
			if (clonedValue) {
				clonedProperty = { ...incomingProperty, value: clonedValue };
			}
		}

		return clonedProperty;
	}

	async #cloneInnerValues<ValueType>(
		incomingProperty: UmbPropertyValueDataPotentiallyWithEditorAlias<ValueType>,
	): Promise<UmbPropertyValueDataPotentiallyWithEditorAlias<ValueType>> {
		const editorAlias = (incomingProperty as any).editorAlias as string | undefined;
		if (!editorAlias) {
			return incomingProperty;
		}

		const manifest = this.#resolvers.getManifests(editorAlias)[0];
		if (!manifest) {
			return incomingProperty;
		}

		const api = await createExtensionApi(this, manifest);
		if (!api) {
			return incomingProperty;
		}

		// TODO: Setting the manifest on the api is legacy, the implementation should not have to do this. [NL]
		api.manifest = manifest;

		if (api.processValues) {
			return (
				(await api.processValues(incomingProperty, async (properties) => {
					// Transform the values:
					const clonedValues = await Promise.all(
						properties.map(async (value) => {
							return (await this.#cloneProperty(value)) ?? value;
						}),
					);

					return clonedValues;
				})) ?? incomingProperty
			);
		}

		// the api did not provide a value processor, so we will return the incoming property:
		return incomingProperty;
	}
}
