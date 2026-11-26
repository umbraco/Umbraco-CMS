import { readContentPickerConfig, withValues } from './utils.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type {
	UmbPropertyEditorConfig,
	UmbPropertyEditorSchemaConfigConverter,
} from '@umbraco-cms/backoffice/property-editor';

/**
 * Converts the Multi Node Tree Picker's settings for the document pickers.
 */
export class UmbContentPickerToDocumentPickerSchemaConfigConverter
	extends UmbControllerBase
	implements UmbPropertyEditorSchemaConfigConverter
{
	convert(config: UmbPropertyEditorConfig): UmbPropertyEditorConfig {
		const { entityType, startNodeId, dynamicRoot, filter } = readContentPickerConfig(config);

		// A start node and filter for media or members don't apply to documents.
		if (entityType !== 'content') return [];

		return withValues([
			{ alias: 'allowedContentTypes', value: filter },
			{ alias: 'startNodeId', value: startNodeId },
			{ alias: 'dynamicRoot', value: dynamicRoot },
		]);
	}
}

export { UmbContentPickerToDocumentPickerSchemaConfigConverter as api };
