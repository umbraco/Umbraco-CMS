import type { UmbSectionSidebarAppElement } from './section-sidebar-app-element.interface.js';
import type { ManifestElement, ManifestWithDynamicConditions } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestSectionSidebarApp
	extends ManifestElement<UmbSectionSidebarAppElement>,
		ManifestWithDynamicConditions<UmbExtensionConditionConfig> {
	type: 'sectionSidebarApp';
}
