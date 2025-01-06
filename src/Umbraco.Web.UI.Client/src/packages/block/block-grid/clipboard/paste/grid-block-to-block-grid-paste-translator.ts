import type { UmbBlockGridValueModel } from '../../types.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbClipboardEntryValueModel, UmbClipboardPasteTranslator } from '@umbraco-cms/backoffice/clipboard';

export class UmbGridBlockToBlockGridClipboardPasteTranslator
	extends UmbControllerBase
	implements UmbClipboardPasteTranslator<UmbBlockGridValueModel>
{
	async translate(model: UmbClipboardEntryValueModel) {
		if (!model) {
			throw new Error('Value model is missing.');
		}

		const valueClone = structuredClone(model.value);

		debugger;

		//return structuredClone(model.value);
	}
}

export { UmbGridBlockToBlockGridClipboardPasteTranslator as api };
