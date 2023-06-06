import { ManifestModal } from '@umbraco-cms/backoffice/extension-registry';

const workspaceModal: ManifestModal = {
	type: 'modal',
	alias: 'Umb.Modal.Workspace',
	name: 'Workspace Modal',
	loader: () => import('./workspace-modal.element.js'),
};

export const manifests = [workspaceModal];
