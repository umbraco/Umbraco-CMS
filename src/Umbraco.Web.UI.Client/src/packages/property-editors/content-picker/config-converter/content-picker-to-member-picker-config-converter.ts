import { readContentPickerConfig } from './content-picker-config.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type {
	UmbPropertyEditorConfig,
	UmbPropertyEditorConfigConverter,
} from '@umbraco-cms/backoffice/property-editor';

/**
 * Converts the Multi Node Tree Picker's settings for the member pickers. Its filter carries over unchanged, as the
 * member pickers name it the same, and members have no start node.
 */
export class UmbContentPickerToMemberPickerConfigConverter
	extends UmbControllerBase
	implements UmbPropertyEditorConfigConverter
{
	convert(config: UmbPropertyEditorConfig): UmbPropertyEditorConfig {
		const { entityType } = readContentPickerConfig(config);

		// A filter for documents or media would otherwise carry over as a member type filter.
		return entityType === 'member' ? [] : [{ alias: 'filter', value: null }];
	}
}

export { UmbContentPickerToMemberPickerConfigConverter as api };
