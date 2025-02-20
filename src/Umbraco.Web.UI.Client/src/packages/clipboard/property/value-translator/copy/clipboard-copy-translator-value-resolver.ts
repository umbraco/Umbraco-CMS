import type { UmbClipboardEntryValuesType } from '../../../clipboard-entry/types.js';
import type { UmbClipboardCopyPropertyValueTranslator } from './types.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { createExtensionApi } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';

export class UmbClipboardCopyPropertyValueTranslatorValueResolver extends UmbControllerBase {
	async resolve(propertyValue: any, propertyEditorUiAlias: string): Promise<UmbClipboardEntryValuesType> {
		if (!propertyValue) {
			throw new Error('Property value is required.');
		}

		if (!propertyEditorUiAlias) {
			throw new Error('Property editor UI alias is required.');
		}

		const manifests = umbExtensionsRegistry.getByTypeAndFilter(
			'clipboardCopyPropertyValueTranslator',
			(x) => x.fromPropertyEditorUi === propertyEditorUiAlias,
		);

		if (!manifests.length) {
			throw new Error('No clipboard copy translators found.');
		}

		// Create translators
		const apiPromises = manifests.map((manifest) => createExtensionApi(this, manifest));
		const apis = await Promise.all(apiPromises);

		// Translate values
		const valuePromises = apis.map(async (api: UmbClipboardCopyPropertyValueTranslator | undefined) =>
			api?.translate(propertyValue),
		);
		const translatedValues = await Promise.all(valuePromises);

		// Map to clipboard entry value models with entry type and value
		const entryValues = translatedValues.map((value: any, index: number) => {
			const valueType = manifests[index].toClipboardEntryValueType;
			return { type: valueType, value };
		});

		return entryValues;
	}
}
