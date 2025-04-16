import type { UmbSectionElement } from './section-element.interface.js';
import type { ManifestElement, ManifestWithDynamicConditions } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestSection
	extends ManifestElement<UmbSectionElement>,
		ManifestWithDynamicConditions<UmbExtensionConditionConfig> {
	type: 'section';
	meta: MetaSection;
}

export interface MetaSection {
	label: string;
	pathname: string;
}
