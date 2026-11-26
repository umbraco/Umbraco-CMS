import { readContentPickerConfig, withValues } from './utils.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type {
	UmbPropertyEditorConfig,
	UmbPropertyEditorSchemaConfigConverter,
} from '@umbraco-cms/backoffice/property-editor';

/**
 * Converts the Multi Node Tree Picker's settings for the media pickers. Its filter carries over unchanged, as the media
 * pickers name it the same.
 */
export class UmbContentPickerToMediaPickerSchemaConfigConverter
	extends UmbControllerBase
	implements UmbPropertyEditorSchemaConfigConverter
{
	convert(config: UmbPropertyEditorConfig): UmbPropertyEditorConfig {
		const { entityType, startNodeId } = readContentPickerConfig(config);

		// A filter for documents or members would otherwise carry over as a media type filter.
		if (entityType !== 'media') return [{ alias: 'filter', value: null }];

		return withValues([{ alias: 'startNodeId', value: startNodeId }]);
	}
}

export { UmbContentPickerToMediaPickerSchemaConfigConverter as api };
