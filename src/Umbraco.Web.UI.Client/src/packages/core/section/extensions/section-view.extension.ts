import type { UmbSectionViewElement } from './section-view-element.interface.js';
import type { ManifestElement, ManifestWithDynamicConditions } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestSectionView
	extends ManifestElement<UmbSectionViewElement>,
		ManifestWithDynamicConditions<UmbExtensionConditionConfig> {
	type: 'sectionView';
	meta: MetaSectionView;
}

export interface MetaSectionView {
	/**
	 * The displayed name (label) in the navigation.
	 */
	label?: string;

	/**
	 * This is the URL path part for this view. This is used for navigating or deep linking directly to the view
	 * https://yoursite.com/section/settings/view/my-view-path
	 * @example my-view-path
	 * @examples [
	 *  "my-view-path"
	 * ]
	 */
	pathname?: string;

	/**
	 * The icon displayed for this view in the navigation.
	 * @example "box"
	 * @examples [
	 *  "box"
	 * ]
	 */
	icon: string;
}
