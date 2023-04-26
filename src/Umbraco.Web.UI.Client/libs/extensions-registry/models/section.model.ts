import type { ManifestElement } from '.';

export interface ManifestSection extends ManifestElement {
	type: 'section';
	meta: MetaSection;
}

export interface MetaSection {
	label: string;
	pathname: string;
}
