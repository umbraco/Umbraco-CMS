import type { UmbClipboardEntryValuesType } from '../../clipboard-entry/types.js';
import type { UmbClipboardPasteTranslator } from './types.js';
import type { ManifestClipboardPasteTranslator } from './clipboard-paste-translator.extension.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { createExtensionApi, type ManifestBase } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';

export class UmbClipboardPasteTranslatorValueResolver<PropertyValueType = any> extends UmbControllerBase {
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

		const supportedManifests = umbExtensionsRegistry.getByTypeAndFilter('clipboardPasteTranslator', (manifest) => {
			const entryValueTypes = clipboardEntryValues.map((x) => x.type);
			const canTranslateValue = entryValueTypes.includes(manifest.fromClipboardEntryValueType);
			const supportsPropertyEditorUi = manifest.toPropertyEditorUi === propertyEditorUiAlias;
			return canTranslateValue && supportsPropertyEditorUi;
		});

		if (!supportedManifests.length) {
			throw new Error('No paste translator found for the given property editor ui and entry value type.');
		}

		// Pick the manifest with the highest priority
		const manifest: ManifestClipboardPasteTranslator = supportedManifests.sort(
			(a: ManifestBase, b: ManifestBase): number => (b.weight || 0) - (a.weight || 0),
		)[0];

		const pasteTranslator = await createExtensionApi<UmbClipboardPasteTranslator>(this, manifest);

		if (!pasteTranslator) {
			throw new Error('Failed to create paste translator.');
		}

		if (!pasteTranslator.translate) {
			throw new Error('Paste translator does not have a translate method.');
		}

		const valueToTranslate = clipboardEntryValues.find((x) => x.type === manifest.fromClipboardEntryValueType);

		if (!valueToTranslate) {
			throw new Error(`Value to translate is missing`);
		}

		return pasteTranslator.translate(valueToTranslate.value);
	}
}
