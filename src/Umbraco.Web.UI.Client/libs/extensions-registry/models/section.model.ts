import type { UmbSectionExtensionElement } from '../interfaces';
import type { ManifestElement } from '@umbraco-cms/backoffice/extensions-api';

export interface ManifestSection extends ManifestElement<UmbSectionExtensionElement> {
	type: 'section';
	meta: MetaSection;
}

export interface MetaSection {
	label: string;
	pathname: string;
}
