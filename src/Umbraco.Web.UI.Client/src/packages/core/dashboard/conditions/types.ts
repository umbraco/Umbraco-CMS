import type { UmbConditionConfigBase } from '@umbraco-cms/backoffice/extension-api';

export type UmbDashboardAliasConditionConfig = UmbConditionConfigBase<'Umb.Condition.DashboardAlias'> & {
	/**
	 * Define the dashboard that this extension should be available in
	 * @example "Umb.Dashboard.Content"
	 */
	match: string;
	/**
	 * Define one or more dashboards that this extension should be available in
	 * @example
	 * ["Umb.Dashboard.Content", "Umb.Dashboard.Media"]
	 */
	oneOf?: Array<string>;
};

declare global {
	interface UmbExtensionConditionConfigMap {
		umbDashboardAliasConditionConfig: UmbDashboardAliasConditionConfig;
	}
}
