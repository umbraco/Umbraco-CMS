import type { ManifestElement, ManifestWithConditions } from './models';

export interface ManifestDashboard extends ManifestElement, ManifestWithConditions<ConditionsDashboard> {
	type: 'dashboard';
	meta: MetaDashboard;
}

export interface MetaDashboard {
	/**
	 * This is the URL path for the dashboard which is used for navigating or deep linking directly to the dashboard
	 * https://yoursite.com/section/settings/dashboard/my-dashboard-path
	 *
	 * @example my-dashboard-path
	 * @examples [
	 *  "my-dashboard-path"
	 * ]
	 */
	pathname: string;

	/**
	 * The displayed name (label) for the tab of the dashboard
	 */
	label?: string;
}

export interface ConditionsDashboard {
	/**
	 * An array of section aliases that the dashboard should be available in
	 *
	 * @examples [
	 *   ["Umb.Section.Content"],
	 *   ["Umb.Section.Settings"]
	 * ]
	 * @uniqueItems true
	 */
	sections: string[];
}
