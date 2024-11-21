import { UmbConditionBase } from '../../extension-registry/conditions/condition-base.controller.js';
import type { SectionAliasConditionConfig } from './types.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbConditionControllerArguments, UmbExtensionCondition } from '@umbraco-cms/backoffice/extension-api';
import { UMB_SECTION_CONTEXT } from '@umbraco-cms/backoffice/section';

export class UmbSectionAliasCondition
	extends UmbConditionBase<SectionAliasConditionConfig>
	implements UmbExtensionCondition
{
	constructor(host: UmbControllerHost, args: UmbConditionControllerArguments<SectionAliasConditionConfig>) {
		super(host, args);

		let permissionCheck: ((sectionAlias: string) => boolean) | undefined = undefined;
		if (this.config.match) {
			permissionCheck = (sectionAlias: string) => sectionAlias === this.config.match;
		} else if (this.config.oneOf) {
			permissionCheck = (sectionAlias: string) => this.config.oneOf!.indexOf(sectionAlias) !== -1;
		}

		if (permissionCheck !== undefined) {
			this.consumeContext(UMB_SECTION_CONTEXT, (context) => {
				this.observe(
					context.alias,
					(sectionAlias) => {
						this.permitted = sectionAlias ? permissionCheck!(sectionAlias) : false;
					},
					'observeAlias',
				);
			});
		}
	}
}
