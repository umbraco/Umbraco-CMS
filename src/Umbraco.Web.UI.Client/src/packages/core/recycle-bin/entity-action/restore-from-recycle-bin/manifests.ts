import { manifest as kindManifest } from './restore-from-recycle-bin.action.kind.js';

export const manifests = [
	kindManifest,
	{
		type: 'modal',
		alias: 'Umb.Modal.RecycleBin.Restore',
		name: 'Restore From Recycle Bin Modal',
		js: () => import('./modal/restore-from-recycle-bin-modal.element.js'),
	},
];
