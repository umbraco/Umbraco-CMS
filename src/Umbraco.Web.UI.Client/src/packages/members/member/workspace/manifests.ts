import { UMB_MEMBER_ENTITY_TYPE } from '../entity.js';
import type {
	ManifestWorkspace,
	ManifestWorkspaceAction,
	ManifestWorkspaceView,
} from '@umbraco-cms/backoffice/extension-registry';

export const UMB_MEMBER_WORKSPACE_ALIAS = 'Umb.Workspace.Member';

const workspace: ManifestWorkspace = {
	type: 'workspace',
	alias: UMB_MEMBER_WORKSPACE_ALIAS,
	name: 'Member Workspace',
	js: () => import('./member-workspace.element.js'),
	meta: {
		entityType: UMB_MEMBER_ENTITY_TYPE,
	},
};

const workspaceViews: Array<ManifestWorkspaceView> = [];
const workspaceActions: Array<ManifestWorkspaceAction> = [];

export const manifests = [workspace, ...workspaceViews, ...workspaceActions];
