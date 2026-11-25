import type { UmbPropertyEditorConfig } from '../config/types.js';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

/**
 * Converts the settings of a data type that moves from one property editor schema to another.
 */
export interface UmbPropertyEditorConfigConverter extends UmbApi {
	/**
	 * Converts settings stored for the property editor schema the data type is moving from into settings for the one it
	 * is moving to.
	 *
	 * Only return the settings that need converting. Settings it doesn't return are carried over when both property
	 * editors declare them, and otherwise take their default.
	 * @param {UmbPropertyEditorConfig} config - The settings as saved for the previous property editor schema.
	 * @returns {UmbPropertyEditorConfig | Promise<UmbPropertyEditorConfig>} The converted settings.
	 */
	convert(config: UmbPropertyEditorConfig): UmbPropertyEditorConfig | Promise<UmbPropertyEditorConfig>;
}
