import type { InterfaceColor, InterfaceLook } from '@umbraco-ui/uui-base/lib/types/index'
import type { ManifestElement } from './models';

export interface ManifestWorkspaceAction extends ManifestElement {
	type: 'workspaceAction';
	meta: MetaEditorAction;
}

export interface MetaEditorAction {
	workspaces: Array<string>;
	label?: string, //TODO: Use or implement additional label-key
	look?: InterfaceLook,
	color?: InterfaceColor,
}
