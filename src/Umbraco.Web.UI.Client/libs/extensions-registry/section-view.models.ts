import type { ManifestElement } from './models';

export interface ManifestSectionView extends ManifestElement {
	type: 'sectionView';
	meta: MetaSectionView;
	conditions: ConditionsSectionView;
}

export interface MetaSectionView {
	label: string;
	pathname: string;
	icon: string;
}

export interface ConditionsSectionView {
	sections: Array<string>;
}
