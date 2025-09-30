import type { UmbConditionConfigBase } from '@umbraco-cms/backoffice/extension-api';

// eslint-disable-next-line @typescript-eslint/naming-convention
export type MenuAliasConditionConfig = UmbConditionConfigBase & {
	match: string;
};
