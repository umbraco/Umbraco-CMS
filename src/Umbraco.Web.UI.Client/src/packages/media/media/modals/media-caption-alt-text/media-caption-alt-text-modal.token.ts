import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbMediaCaptionAltTextModalData {
	mediaUnique: string;
}

export type UmbMediaCaptionAltTextModalValue = {
	altText?: string;
	caption?: string;
	url: string;
};

export const UMB_MEDIA_CAPTION_ALT_TEXT_MODAL = new UmbModalToken<
	UmbMediaCaptionAltTextModalData,
	UmbMediaCaptionAltTextModalValue
>('Umb.Modal.MediaCaptionAltText', {
	modal: {
		type: 'sidebar',
		size: 'small',
	},
});
