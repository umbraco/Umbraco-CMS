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

		// Find the cloner for this editor alias:
		const manifests = umbExtensionsRegistry.getByTypeAndFilter('clipboardPasteTranslator', (manifest) => {
			const entryValueTypes = clipboardEntryValues.map((x) => x.type);
			const canTranslateValue = manifest.fromClipboardEntryValueTypes.some((type) => entryValueTypes.includes(type));
			const supportsPropertyEditorUi = manifest.toPropertyEditorUis.includes(propertyEditorUiAlias);
			return canTranslateValue && supportsPropertyEditorUi;
		});

		const defaultEntryValue = clipboardEntryValues.find((x) => x.type === 'default');

		if (!defaultEntryValue) {
			throw new Error(`Default value is missing`);
		}

		// return the default value if we have no paste translators
		if (!manifests.length) {
			return defaultEntryValue;
		}

		/* We are in a situation where we have multiple paste translators that will handle the paste for the 
		same property editor ui and entry value type. We will throw an error to inform that this situation has happened.
		We might be able to handle this in the future */
		if (manifests.length > 1) {
			throw new Error('Multiple paste translators found for the same property editor ui and entry value type.');
		}

		const manifest: ManifestClipboardPasteTranslator = manifests[0];

		const pasteTranslator = await createExtensionApi<UmbClipboardPasteTranslator>(this, manifest);

		if (!pasteTranslator || !pasteTranslator.translate) {
			return defaultEntryValue;
		}

		const translatableValues = clipboardEntryValues.filter((x) =>
			manifest.fromClipboardEntryValueTypes.includes(x.type),
		);

		const translatedValues = await pasteTranslator.translate(translatableValues);
		debugger;

		// We can have multiple values returned from the paste translator, but we only want to return one value
		return translatedValues[0];
	}
}
