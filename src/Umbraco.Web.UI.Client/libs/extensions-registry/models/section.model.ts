import type { UmbSectionElement } from '../interfaces';
import type { ManifestElement } from '.';

export interface ManifestSection extends ManifestElement<UmbSectionElement> {
	type: 'section';
	meta: MetaSection;
}

export interface MetaSection {
	label: string;
	pathname: string;
}
