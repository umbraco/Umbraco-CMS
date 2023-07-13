import type { UmbSectionExtensionElement } from '../interfaces/index.js';
import type { ManifestElement, ManifestWithDynamicConditions } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestSection extends ManifestElement<UmbSectionExtensionElement>, ManifestWithDynamicConditions {
	type: 'section';
	meta: MetaSection;
}

export interface MetaSection {
	label: string;
	pathname: string;
}
