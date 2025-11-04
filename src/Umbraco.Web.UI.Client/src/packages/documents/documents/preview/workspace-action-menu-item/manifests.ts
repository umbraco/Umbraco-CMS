import { manifest as workspaceActionItemKind } from './preview-option.workspace-action-item.kind.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

const saveAndPreview: UmbExtensionManifest = {
	type: 'workspaceActionMenuItem',
	kind: 'previewOption',
	alias: 'Umb.Document.WorkspaceActionMenuItem.SaveAndPreview',
	name: 'Save And Preview Document Preview Option',
	forWorkspaceActions: 'Umb.WorkspaceAction.Document.SaveAndPreview',
	weight: 100,
	meta: {
		label: '#buttons_saveAndPreview',
		urlProviderAlias: 'umbDocumentUrlProvider',
	},
};

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	saveAndPreview,
	workspaceActionItemKind,
];
