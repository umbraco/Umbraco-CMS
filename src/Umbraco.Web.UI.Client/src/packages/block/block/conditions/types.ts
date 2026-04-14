import type { UmbConditionConfigBase } from '@umbraco-cms/backoffice/extension-api';

// eslint-disable-next-line @typescript-eslint/naming-convention
export type BlockWorkspaceHasSettingsConditionConfig =
	UmbConditionConfigBase<'Umb.Condition.BlockWorkspaceHasSettings'>;

// eslint-disable-next-line @typescript-eslint/naming-convention
export interface BlockEntryShowContentEditConditionConfig
	extends UmbConditionConfigBase<'Umb.Condition.BlockEntryShowContentEdit'> {
	match?: boolean;
}

// eslint-disable-next-line @typescript-eslint/naming-convention
export interface BlockEntryIsExposedConditionConfig
	extends UmbConditionConfigBase<'Umb.Condition.BlockWorkspaceIsExposed'> {
	match?: boolean;
}

// eslint-disable-next-line @typescript-eslint/naming-convention
export interface BlockWorkspaceIsReadOnlyConditionConfig
	extends UmbConditionConfigBase<'Umb.Condition.BlockWorkspaceIsReadOnly'> {
	match?: boolean;
}

// eslint-disable-next-line @typescript-eslint/naming-convention
export type BlockEntryHasSettingsConditionConfig = UmbConditionConfigBase<'Umb.Condition.BlockEntryHasSettings'>;

// eslint-disable-next-line @typescript-eslint/naming-convention
export interface BlockEntryIsReadOnlyConditionConfig
	extends UmbConditionConfigBase<'Umb.Condition.BlockEntryIsReadOnly'> {
	match?: boolean;
}

// NOTE: Named with a `Umb` prefix, as clashed with `BlockEntryIsExposedConditionConfig`,
// but that one is a misnomer as the condition targets the block workspace. [LK]
export interface UmbBlockEntryIsExposedConditionConfig
	extends UmbConditionConfigBase<'Umb.Condition.BlockEntryIsExposed'> {
	match?: boolean;
}

declare global {
	interface UmbExtensionConditionConfigMap {
		umbBlock:
			| BlockEntryShowContentEditConditionConfig
			| BlockWorkspaceHasSettingsConditionConfig
			| BlockEntryIsExposedConditionConfig
			| BlockWorkspaceIsReadOnlyConditionConfig
			| BlockEntryIsReadOnlyConditionConfig
			| BlockEntryHasSettingsConditionConfig
			| UmbBlockEntryIsExposedConditionConfig;
	}
}
