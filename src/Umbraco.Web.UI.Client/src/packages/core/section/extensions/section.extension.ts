import type { UmbSectionContext } from '../section.context.js';
import type { UmbSectionElement } from './section-element.interface.js';
import type { ManifestElementAndApi, ManifestWithDynamicConditions } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestSection
	extends ManifestElementAndApi<UmbSectionElement, UmbSectionContext>,
		ManifestWithDynamicConditions<UmbExtensionConditionConfig> {
	type: 'section';
	meta: MetaSection;
}

export interface MetaSection {
	label: string;
	pathname: string;
}
