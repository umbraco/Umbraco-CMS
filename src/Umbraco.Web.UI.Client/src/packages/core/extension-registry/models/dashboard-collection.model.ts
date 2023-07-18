import type { ManifestBase } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestDashboardCollection extends ManifestBase {
	type: 'dashboardCollection';
	meta: MetaDashboardCollection;
	conditions: ConditionsDashboardCollection;
}

export interface MetaDashboardCollection {
	/**
	 * The URL path for the dashboard which is used for navigating or deep linking directly to the dashboard
	 * @examples [
	 * "media-management-dashboard",
	 * "my-awesome-dashboard"
	 * ]
	 */
	pathname: string;

	/**
	 *  Optional string to display as the label for the dashboard collection
	 */
	label?: string;

	/**
	 * The alias of the repository that the dashboard collection is for
	 * @examples [
	 * "Umb.Repository.Media"
	 * ]
	 */
	repositoryAlias: string;
}

/**
 * The conditions for when the dashboard should be available
 */
export interface ConditionsDashboardCollection {
	/**
	 * An array of section aliases that the dashboard collection should be available in
	 *
	 * @uniqueItems true
	 * @examples [
	 *  "Umb.Section.Content",
	 * 	"Umb.Section.Settings"
	 * ]
	 */
	sections: string[];

	/**
	 * The entity type that the dashboard collection should be available for
	 * @examples [
	 * "media"
	 * ]
	 */
	entityType: string;
}
