import type { UmbPropertyValueDataPotentiallyWithEditorAlias } from '../index.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { createExtensionApi } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';

export class UmbPropertyValueCloneController extends UmbControllerBase {
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

		// Find the cloner for this editor alias:
		const manifest = umbExtensionsRegistry.getByTypeAndFilter(
			'propertyValueCloner',
			(x) => x.forEditorAlias === editorAlias,
		)[0];

		if (!manifest) {
			return incomingProperty;
		}

		const api = await createExtensionApi(this, manifest);
		if (!api) {
			return incomingProperty;
		}

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

		// Find the resolver for this editor alias:
		const manifest = umbExtensionsRegistry.getByTypeAndFilter(
			'propertyValueResolver',
			// TODO: Remove depcrated filter option in v.17 [NL]
			(x) => x.forEditorAlias === editorAlias || x.meta?.editorAlias === editorAlias,
		)[0];

		if (!manifest) {
			return incomingProperty;
		}

		const api = await createExtensionApi(this, manifest);
		if (!api) {
			return incomingProperty;
		}

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
