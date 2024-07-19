import type { CollectionAliasConditionConfig } from '../../collection/collection-alias.manifest.js';
import type { CollectionBulkActionPermissionConditionConfig } from '../../collection/collection-bulk-action-permission.manifest.js';
import type { UmbSectionUserPermissionConditionConfig } from '../../section/conditions/index.js';
import type { SectionAliasConditionConfig } from './section-alias.condition.js';
import type { SwitchConditionConfig } from './switch.condition.js';
import type {
	WorkspaceAliasConditionConfig,
	WorkspaceEntityTypeConditionConfig,
} from '@umbraco-cms/backoffice/workspace';
import type { UmbConditionConfigBase } from '@umbraco-cms/backoffice/extension-api';
import type { UmbDocumentUserPermissionConditionConfig } from '@umbraco-cms/backoffice/document';

/* TODO: in theory should't the core package import from other packages.
Are there any other way we can do this?
Niels: Sadly I don't see any other solutions currently. But are very open for ideas :-) now that I think about it maybe there is some ability to extend a global type, similar to the 'declare global' trick we use on Elements.
*/

// temp location to avoid circular dependencies
export type BlockWorkspaceHasSettingsConditionConfig =
	UmbConditionConfigBase<'Umb.Condition.BlockWorkspaceHasSettings'>;

export type BlockEntryShowContentEditConditionConfig =
	UmbConditionConfigBase<'Umb.Condition.BlockEntryShowContentEdit'>;

export type ConditionTypes =
	| BlockEntryShowContentEditConditionConfig
	| BlockWorkspaceHasSettingsConditionConfig
	| CollectionAliasConditionConfig
	| CollectionBulkActionPermissionConditionConfig
	| SectionAliasConditionConfig
	| SwitchConditionConfig
	| UmbDocumentUserPermissionConditionConfig
	| UmbSectionUserPermissionConditionConfig
	| WorkspaceAliasConditionConfig
	| WorkspaceEntityTypeConditionConfig
	| UmbConditionConfigBase;
