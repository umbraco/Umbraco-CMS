import type { ManifestElement } from './models';

export interface ManifestSectionView extends ManifestElement {
	type: 'sectionView';
	meta: MetaSectionView;
}

export interface MetaSectionView {
	sections: Array<string>;
	label: string;
	pathname: string;
	weight: number;
	icon: string;
}
