import { manifest as workspaceActionItemKind } from './url-provider.workspace-action-item.kind.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

const saveAndPreview: UmbExtensionManifest = {
	type: 'workspaceActionMenuItem',
	kind: 'urlProvider',
	alias: 'Umb.Document.WorkspaceActionMenuItem.SaveAndPreview',
	name: 'Save And Preview Document URL Provider Workspace Action Menu Item',
	forWorkspaceActions: 'Umb.WorkspaceAction.Document.SaveAndPreview',
	urlProviderAlias: 'umbDocumentUrlProvider',
	weight: 100,
	meta: {
		label: '#buttons_saveAndPreview',
	},
};

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	saveAndPreview,
	workspaceActionItemKind,
];
