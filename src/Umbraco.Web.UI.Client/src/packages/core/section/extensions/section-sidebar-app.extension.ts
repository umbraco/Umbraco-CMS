import type { ManifestElement, ManifestWithDynamicConditions } from '@umbraco-cms/backoffice/extension-api';
import type { UmbSectionSidebarAppElement } from '@umbraco-cms/backoffice/section';

export interface ManifestSectionSidebarApp
	extends ManifestElement<UmbSectionSidebarAppElement>,
		ManifestWithDynamicConditions<UmbExtensionConditionConfig> {
	type: 'sectionSidebarApp';
}
