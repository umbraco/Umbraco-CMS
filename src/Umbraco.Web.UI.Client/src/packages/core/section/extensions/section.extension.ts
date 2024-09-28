import type { ManifestElement, ManifestWithDynamicConditions } from '@umbraco-cms/backoffice/extension-api';
import type { UmbSectionElement } from '@umbraco-cms/backoffice/extension-registry';

export interface ManifestSection
	extends ManifestElement<UmbSectionElement>,
		ManifestWithDynamicConditions<UmbExtensionCondition> {
	type: 'section';
	meta: MetaSection;
}

export interface MetaSection {
	label: string;
	pathname: string;
}
