import { manifest as kindManifest } from './restore-from-recycle-bin.action.kind.js';
import UmbRestoreFromRecycleBinModalElement from './modal/restore-from-recycle-bin-modal.element.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	kindManifest,
	{
		type: 'modal',
		alias: 'Umb.Modal.RecycleBin.Restore',
		name: 'Restore From Recycle Bin Modal',
		element: UmbRestoreFromRecycleBinModalElement,
	},
];
