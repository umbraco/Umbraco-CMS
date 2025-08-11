import type { UMB_SECTION_ALIAS_CONDITION_ALIAS } from './constants.js';
import type { UmbConditionConfigBase } from '@umbraco-cms/backoffice/extension-api';

// TODO: Rename this to `UmbSectionAliasConditionConfig` in a future version.
// eslint-disable-next-line @typescript-eslint/naming-convention
export type SectionAliasConditionConfig = UmbConditionConfigBase<typeof UMB_SECTION_ALIAS_CONDITION_ALIAS> & {
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
