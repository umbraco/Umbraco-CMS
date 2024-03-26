import { UmbConditionBase } from './condition-base.controller.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type {
	ManifestCondition,
	UmbConditionConfigBase,
	UmbConditionControllerArguments,
	UmbExtensionCondition,
} from '@umbraco-cms/backoffice/extension-api';
import { UMB_SECTION_CONTEXT } from '@umbraco-cms/backoffice/section';

export class UmbSectionAliasCondition
	extends UmbConditionBase<SectionAliasConditionConfig>
	implements UmbExtensionCondition
{
	constructor(host: UmbControllerHost, args: UmbConditionControllerArguments<SectionAliasConditionConfig>) {
		super(host, args);
		this.consumeContext(UMB_SECTION_CONTEXT, (context) => {
			this.observe(context.alias, (sectionAlias) => {
				this.permitted = sectionAlias === this.config.match;
			});
		});
	}
}

export type SectionAliasConditionConfig = UmbConditionConfigBase<'Umb.Condition.SectionAlias'> & {
	/**
	 * Define the section that this extension should be available in
	 *
	 * @example "Umb.Section.Content"
	 */
	match: string;
};

export const manifest: ManifestCondition = {
	type: 'condition',
	name: 'Section Alias Condition',
	alias: 'Umb.Condition.SectionAlias',
	api: UmbSectionAliasCondition,
};
