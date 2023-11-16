import { UMB_MEMBER_GROUP_ENTITY_TYPE } from '../entity.js';
import type {
	ManifestWorkspace,
	ManifestWorkspaceAction,
	ManifestWorkspaceEditorView,
} from '@umbraco-cms/backoffice/extension-registry';

export const UMB_MEMBER_GROUP_WORKSPACE_ALIAS = 'Umb.Workspace.MemberGroup';

const workspace: ManifestWorkspace = {
	type: 'workspace',
	alias: UMB_MEMBER_GROUP_WORKSPACE_ALIAS,
	name: 'MemberGroup Workspace',
	loader: () => import('./member-group-workspace.element.js'),
	meta: {
		entityType: UMB_MEMBER_GROUP_ENTITY_TYPE,
	},
};

const workspaceViews: Array<ManifestWorkspaceEditorView> = [];
const workspaceActions: Array<ManifestWorkspaceAction> = [];

export const manifests = [workspace, ...workspaceViews, ...workspaceActions];
