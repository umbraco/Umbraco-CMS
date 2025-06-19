import type { ManifestBase } from '@umbraco-cms/backoffice/extension-api';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbManifestViewerModalData extends ManifestBase {}

export type UmbManifestViewerModalValue = undefined;

/* TODO: This is a temporary location for the manifest viewer files.They are located here because of build issues.
Please don't export this token from the block package. [MR] */
export const UMB_MANIFEST_VIEWER_MODAL = new UmbModalToken<UmbManifestViewerModalData, UmbManifestViewerModalValue>(
	'Umb.Modal.ManifestViewer',
	{
		modal: {
			type: 'sidebar',
			size: 'medium',
		},
	},
);
