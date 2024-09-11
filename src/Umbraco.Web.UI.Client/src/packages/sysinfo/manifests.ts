import type { ManifestModal, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const modalManifests: Array<ManifestModal> = [
	{
		type: 'modal',
		alias: 'Umb.Sysinfo.Modal',
		name: 'Sysinfo Modal',
		js: () => import('./components/sysinfo.element.js'),
	},
	{
		type: 'modal',
		alias: 'Umb.NewVersion.Modal',
		name: 'New Version Modal',
		js: () => import('./components/new-version.element.js'),
	},
];

export const manifests: Array<ManifestTypes> = [...modalManifests];
