import type { UmbClipboardEntryValuesType } from '../../../clipboard-entry/types.js';
import type { UmbClipboardPastePropertyValueTranslator } from './types.js';
import type { ManifestClipboardPastePropertyValueTranslator } from './clipboard-paste-translator.extension.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { createExtensionApi, type ManifestBase } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';

export class UmbClipboardPastePropertyValueTranslatorValueResolver<
	PropertyValueType = unknown,
> extends UmbControllerBase {
	#apiCache = new Map<string, UmbClipboardPastePropertyValueTranslator>();

	async resolve(
		clipboardEntryValues: UmbClipboardEntryValuesType,
		propertyEditorUiAlias: string,
	): Promise<PropertyValueType> {
		if (!clipboardEntryValues.length) {
			throw new Error('Clipboard entry values are required.');
		}

		if (!propertyEditorUiAlias) {
			throw new Error('Property editor UI alias is required.');
		}

		const manifest = this.#getManifestWithBestFit(clipboardEntryValues, propertyEditorUiAlias);
		const pasteTranslator = await this.getPasteTranslator(clipboardEntryValues, propertyEditorUiAlias);

		const valueToTranslate = clipboardEntryValues.find((x) => x.type === manifest.fromClipboardEntryValueType);

		if (!valueToTranslate) {
			throw new Error(`Value to translate is missing`);
		}

		return pasteTranslator.translate(valueToTranslate.value);
	}

	/**
	 * Get the paste translator for the given clipboard entry values and property editor ui alias
	 * @param {UmbClipboardEntryValuesType} clipboardEntryValues
	 * @param {string} propertyEditorUiAlias
	 * @returns {Promise<UmbClipboardPastePropertyValueTranslator>} - The paste translator
	 * @memberof UmbClipboardPastePropertyValueTranslatorValueResolver
	 */
	async getPasteTranslator(
		clipboardEntryValues: UmbClipboardEntryValuesType,
		propertyEditorUiAlias: string,
	): Promise<UmbClipboardPastePropertyValueTranslator> {
		const manifest = this.#getManifestWithBestFit(clipboardEntryValues, propertyEditorUiAlias);

		// Check the cache before creating a new instance
		if (this.#apiCache.has(manifest.alias)) {
			return this.#apiCache.get(manifest.alias)!;
		}

		const pasteTranslator = await createExtensionApi<UmbClipboardPastePropertyValueTranslator>(this, manifest);

		if (!pasteTranslator) {
			throw new Error('Failed to create paste translator.');
		}

		if (!pasteTranslator.translate) {
			throw new Error('Paste translator does not have a translate method.');
		}

		// Cache the api instance for future use
		this.#apiCache.set(manifest.alias, pasteTranslator);

		return pasteTranslator;
	}

	#getManifestWithBestFit(
		clipboardEntryValues: UmbClipboardEntryValuesType,
		propertyEditorUiAlias: string,
	): ManifestClipboardPastePropertyValueTranslator {
		const supportedManifests = this.#getSupportedManifests(clipboardEntryValues, propertyEditorUiAlias);

		if (!supportedManifests.length) {
			throw new Error('No paste translator found for the given property editor ui and entry value type.');
		}

		// Pick the manifest with the highest priority
		// TODO: This should have been handled in the extension registry, but until then we do it here: [NL]
		return supportedManifests.sort((a: ManifestBase, b: ManifestBase): number => (b.weight || 0) - (a.weight || 0))[0];
	}

	#getSupportedManifests(clipboardEntryValues: UmbClipboardEntryValuesType, propertyEditorUiAlias: string) {
		const entryValueTypes = clipboardEntryValues.map((x) => x.type);

		const supportedManifests = umbExtensionsRegistry.getByTypeAndFilter(
			'clipboardPastePropertyValueTranslator',
			(manifest) => {
				const canTranslateValue = entryValueTypes.includes(manifest.fromClipboardEntryValueType);
				const supportsPropertyEditorUi = manifest.toPropertyEditorUi === propertyEditorUiAlias;
				return canTranslateValue && supportsPropertyEditorUi;
			},
		);

		return supportedManifests;
	}
}
