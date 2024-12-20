import type { UmbClipboardEntryValuesType } from './types.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { createExtensionApi } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';

export class UmbClipboardCopyTranslatorValueResolver extends UmbControllerBase {
	async resolve(propertyValue: any, propertyEditorUiAlias: string): Promise<UmbClipboardEntryValuesType> {
		if (!propertyValue) {
			throw new Error('Property value is required.');
		}

		if (!propertyEditorUiAlias) {
			throw new Error('Property editor UI alias is required.');
		}

		const defaultValue = {
			type: 'default',
			value: propertyValue,
		};

		const manifests = umbExtensionsRegistry.getByTypeAndFilter('clipboardCopyTranslator', (x) =>
			x.forPropertyEditorUis.includes(propertyEditorUiAlias),
		);

		if (!manifests || manifests.length === 0) {
			return [defaultValue];
		}

		const apiPromises = manifests.map((manifest) => createExtensionApi(this, manifest));
		const apis = await Promise.all(apiPromises);

		const valuePromises = apis.map(async (api) => api?.translate(propertyValue));
		const translatedValues = await Promise.all(valuePromises);

		return [defaultValue, ...translatedValues.filter((x) => x !== undefined).flat()];
	}
}
