import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { createExtensionApi } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import type {
	ManifestPropertyEditorSchemaConfigConverter,
	UmbPropertyEditorConfig,
	UmbPropertyEditorSchemaConfigConverter,
} from '@umbraco-cms/backoffice/property-editor';

/**
 * Converts a data type's settings from one property editor schema to another, using the registered
 * `propertyEditorSchemaConfigConverter` extensions.
 */
export class UmbDataTypeSchemaConfigConverterController extends UmbControllerBase {
	/**
	 * Whether any converter is registered for moving from one property editor schema to another.
	 * @param {string} fromSchemaAlias - The alias of the property editor schema the data type is moving from.
	 * @param {string} toSchemaAlias - The alias of the property editor schema the data type is moving to.
	 * @returns {boolean} True when settings can be converted.
	 */
	hasConverter(fromSchemaAlias: string, toSchemaAlias: string): boolean {
		return this.#getManifests(fromSchemaAlias, toSchemaAlias).length > 0;
	}

	/**
	 * Converts settings saved for one property editor schema into settings for another.
	 * @param {string} fromSchemaAlias - The alias of the property editor schema the settings were saved for.
	 * @param {string} toSchemaAlias - The alias of the property editor schema to convert the settings for.
	 * @param {UmbPropertyEditorConfig} config - The settings to convert.
	 * @returns {Promise<UmbPropertyEditorConfig>} The converted settings; a setting from a converter of higher weight wins.
	 * A converter that fails is skipped, so the others still apply.
	 */
	async convert(
		fromSchemaAlias: string,
		toSchemaAlias: string,
		config: UmbPropertyEditorConfig,
	): Promise<UmbPropertyEditorConfig> {
		const converted: UmbPropertyEditorConfig = [];

		for (const manifest of this.#getManifests(fromSchemaAlias, toSchemaAlias)) {
			let api: UmbPropertyEditorSchemaConfigConverter | undefined;
			try {
				api = await createExtensionApi(this, manifest);
				for (const setting of (await api?.convert(config)) ?? []) {
					if (converted.some((x) => x.alias === setting.alias) === false) {
						converted.push(setting);
					}
				}
			} catch (error) {
				console.error(
					`The property editor schema config converter "${manifest.alias}" failed, so it was skipped.`,
					error,
				);
			} finally {
				api?.destroy();
			}
		}

		return converted;
	}

	#getManifests(fromSchemaAlias: string, toSchemaAlias: string) {
		return umbExtensionsRegistry.getByTypeAndFilter<
			'propertyEditorSchemaConfigConverter',
			ManifestPropertyEditorSchemaConfigConverter
		>(
			'propertyEditorSchemaConfigConverter',
			(manifest) =>
				manifest.fromPropertyEditorSchemaAlias === fromSchemaAlias &&
				manifest.forPropertyEditorSchemaAlias === toSchemaAlias,
		);
	}
}
