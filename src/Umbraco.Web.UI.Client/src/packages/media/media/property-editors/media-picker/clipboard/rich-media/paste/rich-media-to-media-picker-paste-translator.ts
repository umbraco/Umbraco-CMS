import type { UmbMediaPickerValueModel } from '../../../../types.js';
import type { UmbRichMediaClipboardEntryValueModel } from '../../../../../clipboard/types.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UmbId } from '@umbraco-cms/backoffice/id';
import type { UmbClipboardPastePropertyValueTranslator } from '@umbraco-cms/backoffice/clipboard';

export class UmbRichMediaToMediaPickerClipboardPastePropertyValueTranslator
	extends UmbControllerBase
	implements UmbClipboardPastePropertyValueTranslator<UmbRichMediaClipboardEntryValueModel, UmbMediaPickerValueModel>
{
	async translate(value: UmbRichMediaClipboardEntryValueModel): Promise<UmbMediaPickerValueModel> {
		if (!value) {
			throw new Error('Value is missing.');
		}

		return value.map((item) => ({
			key: UmbId.new(),
			mediaKey: item.unique,
			mediaTypeAlias: '',
			focalPoint: item.focalPoint ?? null,
			crops: structuredClone(item.crops ?? []),
		}));
	}

	async isCompatibleValue(): Promise<boolean> {
		// For now we accept all rich media clipboard values for the media picker.
		// Compatibility checks (e.g. allowed media types) can be added later if needed.
		return true;
	}
}

export { UmbRichMediaToMediaPickerClipboardPastePropertyValueTranslator as api };
