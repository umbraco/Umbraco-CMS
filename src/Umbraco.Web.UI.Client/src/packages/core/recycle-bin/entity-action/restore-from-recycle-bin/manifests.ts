import { manifest as kindManifest } from './restore-from-recycle-bin.action.kind.js';

export const manifests: Array<ManifestTypes> = [
	kindManifest,
	{
		type: 'modal',
		alias: 'Umb.Modal.RecycleBin.Restore',
		name: 'Restore From Recycle Bin Modal',
		element: () => import('./modal/restore-from-recycle-bin-modal.element.js'),
	},
];
