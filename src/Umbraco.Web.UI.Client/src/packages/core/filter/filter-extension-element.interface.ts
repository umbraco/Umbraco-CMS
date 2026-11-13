import type { UmbFilterExtensionApi } from './filter-extension-api.interface.js';
import type { ManifestFilterBase } from './filter.extension.js';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';

/**
 * The element of a filter extension, shared by the filter extension types of every filter host. It is rendered inside
 * `<umb-filter>`, which provides the filter context it reads and writes the filter's values through.
 */
export interface UmbFilterExtensionElement<
	ManifestType extends ManifestFilterBase = ManifestFilterBase,
> extends UmbControllerHostElement {
	api?: UmbFilterExtensionApi<ManifestType>;
	manifest?: ManifestType;
}
