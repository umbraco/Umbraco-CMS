import type { UmbClipboardEntryPasteTranslator } from './types.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { createExtensionApi } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';

export class UmbClipboardEntryPasteTranslatorResolver
	extends UmbControllerBase
	implements UmbClipboardEntryPasteTranslator
{
	async translate(entry: any, propertyEditorUiAlias: string): Promise<any> {
		if (!entry) {
			throw new Error('Clipboard entry is required.');
		}

		if (!entry.type) {
			throw new Error('Clipboard entry type is required.');
		}

		// Find the cloner for this editor alias:
		const manifest = umbExtensionsRegistry.getByTypeAndFilter(
			'pasteClipboardEntryTranslator',
			(x) =>
				x.forClipboardEntryTypes.includes(entry.type) && x.forPropertyEditorUiAliases.includes(propertyEditorUiAlias),
		)[0];

		debugger;

		if (!manifest) {
			return entry.value;
		}

		const api = await createExtensionApi(this, manifest);
		if (!api) {
			return entry.value;
		}

		return api.translate ? await api.translate(entry) : entry.value;
	}
}
