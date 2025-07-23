import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbNewVersionModalData {
	/**
	 * The release notes of the new version.
	 */
	comment: string;

	/**
	 * The download URL of the new version.
	 */
	downloadUrl: string;
}

export const UMB_NEWVERSION_MODAL = new UmbModalToken<UmbNewVersionModalData>('Umb.Modal.NewVersion', {
	modal: {
		type: 'dialog',
		size: 'medium',
	},
});
