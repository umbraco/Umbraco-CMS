import type { UmbSectionExtensionElement } from '../interfaces';
import type { ManifestElement } from 'src/libs/extension-api';

export interface ManifestSection extends ManifestElement<UmbSectionExtensionElement> {
	type: 'section';
	meta: MetaSection;
}

export interface MetaSection {
	label: string;
	pathname: string;
}
