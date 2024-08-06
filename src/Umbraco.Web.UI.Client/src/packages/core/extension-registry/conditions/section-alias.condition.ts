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

export type SectionAliasConditionConfig = UmbConditionConfigBase<'Umb.Condition.SectionAlias'> & {
	/**
	 * Define the section that this extension should be available in
	 * @example "Umb.Section.Content"
	 */
	match: string;
	/**
	 * Define one or more workspaces that this extension should be available in
	 * @example
	 * ["Umb.Section.Content", "Umb.Section.Media"]
	 */
	oneOf?: Array<string>;
};

export const manifest: ManifestCondition = {
	type: 'condition',
	name: 'Section Alias Condition',
	alias: 'Umb.Condition.SectionAlias',
	api: UmbSectionAliasCondition,
};
