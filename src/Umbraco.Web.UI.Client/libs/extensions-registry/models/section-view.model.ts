import type { UmbSectionViewElement } from '../interfaces/section-view-element.interface';
import type { ManifestElement, ManifestWithConditions } from '.';

export interface ManifestSectionView
	extends ManifestElement<UmbSectionViewElement>,
		ManifestWithConditions<ConditionsSectionView> {
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
