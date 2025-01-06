import type { UmbClipboardEntryValuesType, UmbClipboardPasteTranslator } from '../types.js';
import type { ManifestClipboardPasteTranslator } from './clipboard-paste-translator.extension.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { createExtensionApi } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';

export class UmbClipboardPasteTranslatorValueResolver extends UmbControllerBase {
	async resolve(clipboardEntryValues: UmbClipboardEntryValuesType, propertyEditorUiAlias: string): Promise<unknown> {
		if (!clipboardEntryValues.length) {
			throw new Error('Clipboard entry values are required.');
		}

		if (!propertyEditorUiAlias) {
			throw new Error('Property editor UI alias is required.');
		}

		// Find the translator for this editor alias:
		const manifests = umbExtensionsRegistry.getByTypeAndFilter('clipboardPasteTranslator', (manifest) => {
			const entryValueTypes = clipboardEntryValues.map((x) => x.type);
			const canTranslateValue = entryValueTypes.includes(manifest.fromClipboardEntryValueType);
			const supportsPropertyEditorUi = manifest.toPropertyEditorUi.includes(propertyEditorUiAlias);
			return canTranslateValue && supportsPropertyEditorUi;
		});

		if (!manifests.length) {
			throw new Error('No paste translator found for the given property editor ui and entry value type.');
		}

		/*
		const defaultEntryValue = clipboardEntryValues.find((x) => x.type === 'default');

		if (!defaultEntryValue) {
			throw new Error(`Default value is missing`);
		}

		// return the default value if we have no paste translators
		if (!manifests.length) {
			return defaultEntryValue;
		}
		*/

		/* We are in a situation where we have multiple paste translators that will handle the paste for the 
		same property editor ui and entry value type. We will throw an error to inform that this situation has happened.
		We might be able to handle this in the future */
		if (manifests.length > 1) {
			throw new Error('Multiple paste translators found for the same property editor ui and entry value type.');
		}

		// Pick the manifest with the highest priority
		const manifest: ManifestClipboardPasteTranslator = manifests[0];

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

		return pasteTranslator.translate(valueToTranslate);
	}
}
