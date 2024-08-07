import type { ManifestBase } from '@umbraco-cms/backoffice/extension-api';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbManifestViewerModalData extends ManifestBase {}

export type UmbManifestViewerModalValue = undefined;

export const UMB_MANIFEST_VIEWER_MODAL = new UmbModalToken<UmbManifestViewerModalData, UmbManifestViewerModalValue>(
	'Umb.Modal.ManifestViewer',
	{
		modal: {
			type: 'sidebar',
			size: 'medium',
		},
	},
);
