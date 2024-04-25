import type { ManifestModal, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const workspaceModal: ManifestModal = {
	type: 'modal',
	alias: 'Umb.Modal.Workspace',
	name: 'Workspace Modal',
	js: () => import('./workspace-modal.element.js'),
};

export const manifests: Array<ManifestTypes> = [workspaceModal];
