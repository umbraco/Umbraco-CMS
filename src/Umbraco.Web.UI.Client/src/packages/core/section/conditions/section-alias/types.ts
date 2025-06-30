import type { UmbConditionConfigBase } from '@umbraco-cms/backoffice/extension-api';

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

declare global {
	interface UmbExtensionConditionConfigMap {
		UmbSectionAliasConditionConfig: SectionAliasConditionConfig;
	}
}
