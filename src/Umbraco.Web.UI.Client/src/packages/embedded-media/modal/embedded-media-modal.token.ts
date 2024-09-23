import { UmbModalToken } from './modal-token.js';

export interface UmbEmbeddedMediaModalData extends Partial<UmbEmbeddedMediaDimensionsModel> {
	url?: string;
}

export interface UmbEmbeddedMediaDimensionsModel {
	constrain: boolean;
	width: number;
	height: number;
}

export interface UmbEmbeddedMediaModalValue extends UmbEmbeddedMediaModalData {
	markup: string;
	url: string;
}

export const UMB_EMBEDDED_MEDIA_MODAL = new UmbModalToken<UmbEmbeddedMediaModalData, UmbEmbeddedMediaModalValue>(
	'Umb.Modal.EmbeddedMedia',
	{
		modal: {
			type: 'sidebar',
			size: 'medium',
		},
	},
);
