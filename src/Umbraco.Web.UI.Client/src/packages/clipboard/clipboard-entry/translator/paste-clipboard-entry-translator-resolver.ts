import type { UmbPasteClipboardEntryTranslator } from './types.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { createExtensionApi } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';

export class UmbPasteClipboardEntryTranslateController
	extends UmbControllerBase
	implements UmbPasteClipboardEntryTranslator
{
	async translate(entry: any) {
		if (!entry) {
			throw new Error('Clipboard entry is required.');
		}

		if (!entry.type) {
			throw new Error('Clipboard entry type is required.');
		}

		// Find the cloner for this editor alias:
		const manifest = umbExtensionsRegistry.getByTypeAndFilter('pasteClipboardEntryTranslator', (x) =>
			x.forClipboardEntryTypes.includes(entry.type),
		)[0];

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
