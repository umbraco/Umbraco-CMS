import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { createExtensionApi } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbPropertyValueDataPotentiallyWithEditorAlias } from '@umbraco-cms/backoffice/property';
import { umbQueryMapperForJsonPaths } from '../../utils/query-mapper-json-paths.function.js';
import { UmbDataPathPropertyValueQuery } from '../../utils/data-path-property-value-query.function.js';

// Write interface that can be handed to the API for the Host, so each path Translator can communicate back to the host here. For translating inner values.
export class UmbValidationPropertyPathTranslatorController extends UmbControllerBase {
	/**
	 * translates the property data.
	 * @param {UmbPropertyValueDataPotentiallyWithEditorAlias} property - The property data.
	 * @returns {Promise<UmbPropertyValueDataPotentiallyWithEditorAlias>} - A promise that resolves to the cloned property data.
	 */
	async translateProperties(
		paths: Array<string>,
		data: Array<UmbPropertyValueDataPotentiallyWithEditorAlias>,
		queryConstructor: (entry: UmbPropertyValueDataPotentiallyWithEditorAlias) => string,
	): Promise<Array<string>> {
		return await umbQueryMapperForJsonPaths(
			paths,
			data,
			UmbDataPathPropertyValueQuery,
			async (scopedPaths: Array<string>, data) => {
				return await this.translateProperty(scopedPaths, data);
			},
		);
	}

	/**
	 * translates the property data.
	 * @param {UmbPropertyValueDataPotentiallyWithEditorAlias} property - The property data.
	 * @returns {Promise<UmbPropertyValueDataPotentiallyWithEditorAlias>} - A promise that resolves to the cloned property data.
	 */
	async translateProperty(
		propertyPaths: Array<string>,
		propertyData: UmbPropertyValueDataPotentiallyWithEditorAlias,
	): Promise<Array<string>> {
		const editorAlias = propertyData.editorAlias;
		if (!editorAlias) {
			console.error(`Editor alias not found for ${propertyData.alias}`);
			return propertyPaths;
		}

		// Find the cloner for this editor alias:
		const manifest = umbExtensionsRegistry.getByTypeAndFilter(
			'propertyValidationPathTranslator',
			(x) => x.forEditorAlias === editorAlias,
		)[0];

		if (!manifest) {
			return propertyPaths;
		}

		const api = await createExtensionApi(this, manifest);
		if (!api) {
			return propertyPaths;
		}

		propertyPaths = (await api.translate(propertyPaths, propertyData)) ?? propertyPaths;

		return propertyPaths;
	}
}
