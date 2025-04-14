import type { ManifestElement, ManifestWithDynamicConditions } from '@umbraco-cms/backoffice/extension-api';
import type { UmbSectionSidebarAppElement } from './section-sidebar-app-element.interface.js';

export interface ManifestSectionSidebarApp
	extends ManifestElement<UmbSectionSidebarAppElement>,
		ManifestWithDynamicConditions<UmbExtensionConditionConfig> {
	type: 'sectionSidebarApp';
}
