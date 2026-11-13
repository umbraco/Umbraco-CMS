import type { ManifestFilterBase } from './filter.extension.js';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

/**
 * The API of a filter extension, shared by the filter extension types of every filter host.
 */
export interface UmbFilterExtensionApi<ManifestType extends ManifestFilterBase = ManifestFilterBase> extends UmbApi {
	manifest?: ManifestType;
}
