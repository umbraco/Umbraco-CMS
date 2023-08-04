import { ConditionTypes } from '../conditions/types.js';
import type { UmbDashboardExtensionElement } from '../interfaces/index.js';
import type { ManifestElement, ManifestWithDynamicConditions } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestDashboard
	extends ManifestElement<UmbDashboardExtensionElement>,
		ManifestWithDynamicConditions<ConditionTypes> {
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

/*
export interface ConditionsDashboard {
	sections: string[];
}
*/
