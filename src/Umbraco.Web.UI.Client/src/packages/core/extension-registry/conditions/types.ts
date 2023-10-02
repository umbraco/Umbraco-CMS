import type { UserPermissionConditionConfig } from '@umbraco-cms/backoffice/user-permission';
import type { SectionAliasConditionConfig } from './section-alias.condition.js';
import type { SwitchConditionConfig } from './switch.condition.js';
import type { WorkspaceAliasConditionConfig } from '@umbraco-cms/backoffice/workspace';
import type { UmbConditionConfigBase } from '@umbraco-cms/backoffice/extension-api';

/* TODO: in theory should't the core package import from other packages.
Are there any other way we can do this? */
export type ConditionTypes =
	| SectionAliasConditionConfig
	| WorkspaceAliasConditionConfig
	| SwitchConditionConfig
	| UserPermissionConditionConfig
	| UmbConditionConfigBase;
