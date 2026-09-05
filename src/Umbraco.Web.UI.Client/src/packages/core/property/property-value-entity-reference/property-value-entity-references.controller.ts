import type { UmbPropertyValueDataPotentiallyWithEditorAlias } from '../types.js';
import type { UmbPropertyValueEntityReferenceResolver } from './types.js';
import { UmbPropertyValueFlatMapperController } from '../property-value-flat-mapper/property-value-flat-mapper.controller.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { createExtensionApi } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { dedupeEntityModels } from '@umbraco-cms/backoffice/entity';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';

/**
 * Resolves the entities directly referenced by a property value, walking into any nested property values
 * (e.g. block content) via {@link UmbPropertyValueFlatMapperController}, and deduplicating the result by
 * entity type and unique.
 */
export class UmbPropertyValueEntityReferencesController extends UmbControllerBase {
	/**
	 * Resolves the entities directly referenced by the given property value.
	 * @param {UmbPropertyValueDataPotentiallyWithEditorAlias} value - The property value to resolve references from.
	 * @returns {Promise<Array<UmbEntityModel>>} The referenced entities, deduplicated by entity type and unique.
	 */
	async resolve(value: UmbPropertyValueDataPotentiallyWithEditorAlias): Promise<Array<UmbEntityModel>> {
		const flatMapper = new UmbPropertyValueFlatMapperController(this);
		const referencesPerValue = await flatMapper.flatMap(value, (property) => this.#resolveValue(property));
		return dedupeEntityModels(referencesPerValue.flat());
	}

	async #resolveValue(
		value: UmbPropertyValueDataPotentiallyWithEditorAlias,
	): Promise<Array<UmbEntityModel>> {
		const editorAlias = value.editorAlias;
		if (!editorAlias) return [];

		const manifest = umbExtensionsRegistry.getByTypeAndFilter(
			'propertyValueEntityReference',
			(x) => x.forEditorAlias === editorAlias,
		)[0];
		if (!manifest) return [];

		const api = await createExtensionApi<UmbPropertyValueEntityReferenceResolver>(this, manifest);
		if (!api) return [];

		api.manifest = manifest;

		return api.resolveEntityReferences(value);
	}
}
