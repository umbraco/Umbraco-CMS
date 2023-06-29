import type { UmbDashboardExtensionElement } from '../interfaces/index.js';
import type { ManifestElement, ManifestWithConditions } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestDashboard
	extends ManifestElement<UmbDashboardExtensionElement>,
		ManifestWithConditions<ConditionsDashboard> {
	type: 'dashboard';
	meta: MetaDashboard;
}

export interface MetaDashboard {
	/**
	 * The displayed name (label) in the navigation.
	 */
	label?: string;

	/**
	 * This is the URL path part for this view. This is used for navigating or deep linking directly to the dashboard
	 * https://yoursite.com/section/settings/dashboard/my-dashboard-path
	 *
	 * @example my-dashboard-path
	 * @examples [
	 *  "my-dashboard-path"
	 * ]
	 */
	pathname?: string;
}

export interface ConditionsDashboard {
	/**
	 * An array of section aliases that the dashboard should be available in
	 *
	 * @uniqueItems true
	 * @minItems 1
	 * @items.examples [
	 *   "Umb.Section.Content",
	 *   "Umb.Section.Settings"
	 * ]
	 *
	 */
	sections: string[];
}
