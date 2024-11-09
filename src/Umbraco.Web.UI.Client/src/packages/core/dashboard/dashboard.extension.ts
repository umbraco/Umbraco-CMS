import type { UmbDashboardElement } from './dashboard-element.interface.js';
import type { ManifestElement, ManifestWithDynamicConditions } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestDashboard
	extends ManifestElement<UmbDashboardElement>,
		ManifestWithDynamicConditions<UmbExtensionConditionConfig> {
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
	 * @example my-dashboard-path
	 * @examples [
	 *  "my-dashboard-path"
	 * ]
	 */
	pathname?: string;
}

declare global {
	interface UmbExtensionManifestMap {
		umbDashboard: ManifestDashboard;
	}
}
