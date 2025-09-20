import { UMB_DASHBOARD_CONTEXT } from '../default/index.js';
import type { UmbDashboardAliasConditionConfig } from './types.js';
import { UmbConditionBase } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbConditionControllerArguments, UmbExtensionCondition } from '@umbraco-cms/backoffice/extension-api';

export class UmbDashboardAliasCondition
	extends UmbConditionBase<UmbDashboardAliasConditionConfig>
	implements UmbExtensionCondition
{
	constructor(host: UmbControllerHost, args: UmbConditionControllerArguments<UmbDashboardAliasConditionConfig>) {
		super(host, args);

		let permissionCheck: ((dashboardAlias: string) => boolean) | undefined = undefined;

		if (this.config.match) {
			permissionCheck = (dashboardAlias: string) => dashboardAlias === this.config.match;
		} else if (this.config.oneOf) {
			permissionCheck = (dashboardAlias: string) => this.config.oneOf!.indexOf(dashboardAlias) !== -1;
		}

		if (permissionCheck !== undefined) {
			this.consumeContext(UMB_DASHBOARD_CONTEXT, (context) => {
				this.observe(
					context?.alias,
					(dashboardAlias) => {
						this.permitted = dashboardAlias ? permissionCheck!(dashboardAlias) : false;
					},
					'observeDashboardAlias',
				);
			});
		}
	}
}

export { UmbDashboardAliasCondition as api };
