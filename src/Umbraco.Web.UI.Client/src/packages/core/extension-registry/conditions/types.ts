import type { SectionAliasConditionConfig } from './section-alias.condition.js';
import type { SwitchConditionConfig } from './switch.condition.js';
import type { WorkspaceAliasConditionConfig, WorkspaceEntityTypeConditionConfig } from '@umbraco-cms/backoffice/workspace';
import type { UmbConditionConfigBase } from '@umbraco-cms/backoffice/extension-api';
import type { UserPermissionConditionConfig } from '@umbraco-cms/backoffice/current-user';
import type { CollectionEntityTypeConditionConfig } from '@umbraco-cms/backoffice/collection';

/* TODO: in theory should't the core package import from other packages.
Are there any other way we can do this?
Niels: Sadly I don't see any other solutions currently. But are very open for ideas :-) now that I think about it maybe there is some ability to extend a global type, similar to the 'declare global' trick we use on Elements.
*/
export type ConditionTypes =
  | CollectionEntityTypeConditionConfig
	| SectionAliasConditionConfig
	| WorkspaceAliasConditionConfig
	| WorkspaceEntityTypeConditionConfig
	| SwitchConditionConfig
	| UserPermissionConditionConfig
	| UmbConditionConfigBase;
