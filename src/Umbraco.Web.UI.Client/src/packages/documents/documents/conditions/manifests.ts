import { UmbDocumentWorkspaceHasCollectionCondition } from './document-workspace-has-collection.condition.js';
import type { ManifestCondition } from '@umbraco-cms/backoffice/extension-api';

const documentWorkspaceHasCollectionManifest: ManifestCondition = {
	type: 'condition',
	name: 'Document Workspace Has Collection Condition',
	alias: 'Umb.Condition.DocumentWorkspaceHasCollection',
	api: UmbDocumentWorkspaceHasCollectionCondition,
};

export const manifests = [documentWorkspaceHasCollectionManifest];
