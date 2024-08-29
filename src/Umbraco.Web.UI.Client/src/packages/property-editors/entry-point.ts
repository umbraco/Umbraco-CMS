import type { ManifestWithDynamicConditions, UmbConditionConfigBase, UmbEntryPointOnInit } from '@umbraco-cms/backoffice/extension-api';
import type { WorkspaceAliasConditionConfig } from '@umbraco-cms/backoffice/workspace';

import './checkbox-list/components/index.js';
import './content-picker/components/index.js';

export const onInit: UmbEntryPointOnInit = (_host, _extensionRegistry) => {

	console.log('HELLO AGAIN');

	const condition: UmbConditionConfigBase = {
		alias: 'Umb.Condition.WorkspaceAlias',
		match: 'Umb.Workspace.WARRENYO',
	} as WorkspaceAliasConditionConfig;

	_extensionRegistry.appendCondition('Umb.Dashboard.UmbracoNewsLATE', condition);

	const ext:ManifestWithDynamicConditions = {
		alias: 'Umb.Dashboard.UmbracoNewsLATE',
		type: 'dashboard',
		name: 'WARREN Package',
		weight: 100,
		conditions: [
			{
				alias: 'Umb.Condition.WorkspaceAlias',
				match: 'Umb.Workspace.LATE-COMER-EXISTING',
			} as WorkspaceAliasConditionConfig,
		],
	};

	_extensionRegistry.register(ext);

};
