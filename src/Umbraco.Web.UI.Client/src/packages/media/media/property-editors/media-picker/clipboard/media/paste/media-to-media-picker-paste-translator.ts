import type { UmbMediaPickerValueModel } from '../../../../types.js';
import type { UmbMediaClipboardEntryValueModel } from '../../../../../clipboard/types.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UmbId } from '@umbraco-cms/backoffice/id';
import type { UmbClipboardPastePropertyValueTranslator } from '@umbraco-cms/backoffice/clipboard';

export class UmbMediaToMediaPickerClipboardPastePropertyValueTranslator
	extends UmbControllerBase
	implements UmbClipboardPastePropertyValueTranslator<UmbMediaClipboardEntryValueModel, UmbMediaPickerValueModel>
{
	async translate(value: UmbMediaClipboardEntryValueModel): Promise<UmbMediaPickerValueModel> {
		if (!value) {
			throw new Error('Value is missing.');
		}

		return value.map((item) => ({
			key: UmbId.new(),
			mediaKey: item.unique,
			mediaTypeAlias: '',
			focalPoint: null,
			crops: [],
		}));
	}

	async isCompatibleValue(): Promise<boolean> {
		// A bare media reference is always acceptable for the media picker; crops default to empty.
		return true;
	}
}

export { UmbMediaToMediaPickerClipboardPastePropertyValueTranslator as api };
