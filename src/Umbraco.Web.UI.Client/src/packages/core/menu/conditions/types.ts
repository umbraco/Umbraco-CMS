import type { UmbConditionConfigBase } from '@umbraco-cms/backoffice/extension-api';

export type MenuAliasConditionConfig = UmbConditionConfigBase & {
	match: string;
};
