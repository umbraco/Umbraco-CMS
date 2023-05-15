import type { UmbSectionViewExtensionElement } from '../interfaces/section-view-extension-element.interface';
import type { ManifestElement, ManifestWithConditions } from '@umbraco-cms/backoffice/extensions-api';

export interface ManifestSectionView
	extends ManifestElement<UmbSectionViewExtensionElement>,
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
