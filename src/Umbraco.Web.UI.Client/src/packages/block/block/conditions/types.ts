import type { UmbConditionConfigBase } from '@umbraco-cms/backoffice/extension-api';

export type BlockWorkspaceHasSettingsConditionConfig =
	UmbConditionConfigBase<'Umb.Condition.BlockWorkspaceHasSettings'>;

export type BlockEntryShowContentEditConditionConfig =
	UmbConditionConfigBase<'Umb.Condition.BlockEntryShowContentEdit'>;

declare global {
	interface UmbExtensionConditionMap {
		UmbBlock: BlockEntryShowContentEditConditionConfig | BlockWorkspaceHasSettingsConditionConfig;
	}
}
