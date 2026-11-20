import type { UmbCropModel, UmbMediaPickerValueModel } from '../../../../types.js';
import type { UmbRichMediaClipboardEntryValueModel } from '../../../../../clipboard/types.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UmbId } from '@umbraco-cms/backoffice/id';
import type { UmbClipboardPastePropertyValueTranslator } from '@umbraco-cms/backoffice/clipboard';
import type { UmbPropertyEditorConfig } from '@umbraco-cms/backoffice/property-editor';

export class UmbRichMediaToMediaPickerClipboardPastePropertyValueTranslator
	extends UmbControllerBase
	implements
		UmbClipboardPastePropertyValueTranslator<
			UmbRichMediaClipboardEntryValueModel,
			UmbMediaPickerValueModel,
			UmbPropertyEditorConfig | undefined
		>
{
	async translate(
		value: UmbRichMediaClipboardEntryValueModel,
		config: UmbPropertyEditorConfig | undefined,
	): Promise<UmbMediaPickerValueModel> {
		if (!value) {
			throw new Error('Value is missing.');
		}

		const configuredCrops = this.#configValue<Array<UmbCropModel>>(config, 'crops') ?? [];
		const focalPointEnabled = Boolean(this.#configValue(config, 'enableLocalFocalPoint'));

		return value.map((item) => ({
			key: UmbId.new(),
			mediaKey: item.unique,
			mediaTypeAlias: '',
			focalPoint: focalPointEnabled ? (item.focalPoint ?? null) : null,
			crops: this.#supportedCrops(item.crops ?? [], configuredCrops),
		}));
	}

	// A clipboard entry carries crops from wherever it was copied, which the property being pasted into may not be
	// configured for. Only the framing the user set is worth bringing along — a crop without coordinates is added
	// by the server anyway, exactly as for a freshly picked media.
	#supportedCrops(crops: Array<UmbCropModel>, configuredCrops: Array<UmbCropModel>): Array<UmbCropModel> {
		return configuredCrops.flatMap((configured) => {
			const crop = crops.find((candidate) => candidate.alias === configured.alias);
			if (!crop?.coordinates) return [];

			// Coordinates are insets of the source image, so they survive a change of size but not of shape.
			// Cross-multiplied to compare the aspects without floating point.
			const sameAspect = configured.width * crop.height === crop.width * configured.height;
			if (!sameAspect) return [];

			// The configuration defines the crop; the pasted value only contributes the framing.
			return [{ ...configured, coordinates: crop.coordinates }];
		});
	}

	#configValue<ValueType>(config: UmbPropertyEditorConfig | undefined, alias: string): ValueType | undefined {
		return config?.find((property) => property.alias === alias)?.value as ValueType | undefined;
	}

	async isCompatibleValue(): Promise<boolean> {
		// Allowed media types are not enforced on paste.
		return true;
	}
}

export { UmbRichMediaToMediaPickerClipboardPastePropertyValueTranslator as api };
