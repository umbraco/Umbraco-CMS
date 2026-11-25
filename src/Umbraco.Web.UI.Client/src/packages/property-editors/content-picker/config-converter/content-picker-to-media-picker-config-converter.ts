import { readContentPickerConfig, withValues } from './content-picker-config.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type {
	UmbPropertyEditorConfig,
	UmbPropertyEditorConfigConverter,
} from '@umbraco-cms/backoffice/property-editor';

/**
 * Converts the Multi Node Tree Picker's settings for the media pickers. Its filter carries over unchanged, as the media
 * pickers name it the same.
 */
export class UmbContentPickerToMediaPickerConfigConverter
	extends UmbControllerBase
	implements UmbPropertyEditorConfigConverter
{
	convert(config: UmbPropertyEditorConfig): UmbPropertyEditorConfig {
		const { entityType, startNodeId } = readContentPickerConfig(config);

		// A filter for documents or members would otherwise carry over as a media type filter.
		if (entityType !== 'media') return [{ alias: 'filter', value: null }];

		return withValues([{ alias: 'startNodeId', value: startNodeId }]);
	}
}

export { UmbContentPickerToMediaPickerConfigConverter as api };
