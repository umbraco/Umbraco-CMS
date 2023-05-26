import type {
	ManifestWorkspace,
	ManifestWorkspaceAction,
	ManifestWorkspaceEditorView,
} from '@umbraco-cms/backoffice/extension-registry';

const workspace: ManifestWorkspace = {
	type: 'workspace',
	alias: 'Umb.Workspace.DocumentBlueprint.Root',
	name: 'Document Blueprint Root Workspace',
	loader: () => import('./document-blueprint-root-workspace.element.js'),
	meta: {
		entityType: 'document-blueprint-root',
	},
};

const workspaceEditorViews: Array<ManifestWorkspaceEditorView> = [];
const workspaceActions: Array<ManifestWorkspaceAction> = [];

export const manifests = [workspace, ...workspaceEditorViews, ...workspaceActions];
