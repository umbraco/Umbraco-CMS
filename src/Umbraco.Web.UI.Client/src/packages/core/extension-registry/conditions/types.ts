import type { SectionAliasConditionConfig } from './section-alias.condition.js';
import type { SwitchConditionConfig } from './switch.condition.js';
import type { WorkspaceAliasConditionConfig } from '@umbraco-cms/backoffice/workspace';
import type { UmbConditionConfigBase } from '@umbraco-cms/backoffice/extension-api';

export type ConditionTypes =
	| SectionAliasConditionConfig
	| WorkspaceAliasConditionConfig
	| SwitchConditionConfig
	| UmbConditionConfigBase;
