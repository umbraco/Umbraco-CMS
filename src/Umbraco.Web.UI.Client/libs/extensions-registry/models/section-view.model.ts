import type { ManifestElement, ManifestWithConditions } from './models';

export interface ManifestSectionView extends ManifestElement, ManifestWithConditions<ConditionsSectionView> {
	type: 'sectionView';
	meta: MetaSectionView;
}

export interface MetaSectionView {
	label: string;
	pathname: string;
	icon: string;
}

export interface ConditionsSectionView {
	sections: Array<string>;
}
