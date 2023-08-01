import type { ConditionTypes } from '../conditions/types.js';
import type { InterfaceColor, InterfaceLook } from '@umbraco-cms/backoffice/external/uui';
import type {
	ManifestElement,
	ClassConstructor,
	ManifestWithDynamicConditions,
} from '@umbraco-cms/backoffice/extension-api';
import type { UmbWorkspaceAction } from '@umbraco-cms/backoffice/workspace';

export interface ManifestWorkspaceAction extends ManifestElement, ManifestWithDynamicConditions<ConditionTypes> {
	type: 'workspaceAction';
	meta: MetaWorkspaceAction;
}

export interface MetaWorkspaceAction {
	label?: string; //TODO: Use or implement additional label-key
	look?: InterfaceLook;
	color?: InterfaceColor;
	api: ClassConstructor<UmbWorkspaceAction>;
}
