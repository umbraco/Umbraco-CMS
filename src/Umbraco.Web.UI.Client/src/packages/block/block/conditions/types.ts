import type { UmbConditionConfigBase } from '@umbraco-cms/backoffice/extension-api';

export type BlockWorkspaceHasSettingsConditionConfig =
	UmbConditionConfigBase<'Umb.Condition.BlockWorkspaceHasSettings'>;

export type BlockEntryShowContentEditConditionConfig =
	UmbConditionConfigBase<'Umb.Condition.BlockEntryShowContentEdit'>;

export interface BlockEntryIsExposedConditionConfig
	extends UmbConditionConfigBase<'Umb.Condition.BlockWorkspaceIsExposed'> {
	match?: boolean;
}

declare global {
	interface UmbExtensionConditionConfigMap {
		umbBlock:
			| BlockEntryShowContentEditConditionConfig
			| BlockWorkspaceHasSettingsConditionConfig
			| BlockEntryIsExposedConditionConfig;
	}
}
