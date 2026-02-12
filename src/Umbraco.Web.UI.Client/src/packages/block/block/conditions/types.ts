import type { UmbConditionConfigBase } from '@umbraco-cms/backoffice/extension-api';

// eslint-disable-next-line @typescript-eslint/naming-convention
export type BlockWorkspaceHasSettingsConditionConfig =
	UmbConditionConfigBase<'Umb.Condition.BlockWorkspaceHasSettings'>;

// eslint-disable-next-line @typescript-eslint/naming-convention
export type BlockEntryShowContentEditConditionConfig =
	UmbConditionConfigBase<'Umb.Condition.BlockEntryShowContentEdit'>;

// eslint-disable-next-line @typescript-eslint/naming-convention
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
