import type { ManifestElement } from './models';

export interface ManifestDashboard extends ManifestElement {
	type: 'dashboard';
	meta: MetaDashboard;
}

export interface MetaDashboard {

	/**
	 * A string array of section aliases to which this dashboard will appear such as 
	 * 'Umb.Section.Content', 'Umb.Section.Settings', 'Umb.Section.Translation'
	 * 
	 * @minItems 1
	 * @uniqueItems true
	 */
	sections: string[];

	/**
	 * This is the URL path for the dashboard which is used for navigating or deep linking directly to the dashboard
	 * https://yoursite.com/section/settings/dashboard/my-dashboard-path
	 * 
	 * @example 'my-dashboard-path'
	 */
	pathname: string;

	/**
	 * The displayed name (label) for the tab of the dashboard
	 */
	label?: string;
}
