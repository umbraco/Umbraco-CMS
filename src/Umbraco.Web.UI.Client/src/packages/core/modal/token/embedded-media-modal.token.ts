import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export enum OEmbedStatus {
	NotSupported,
	Error,
	Success,
}

export interface UmbEmbeddedMediaDimensions {
	width: number;
	height: number;
	constrain?: boolean;
}

export interface UmbEmbeddedMediaModalData extends UmbEmbeddedMediaDimensions {
	url?: string;
}

export interface OEmbedResult extends UmbEmbeddedMediaDimensions {
	oEmbedStatus: OEmbedStatus;
	supportsDimensions: boolean;
	markup?: string;
}

export interface UmbEmbeddedMediaModalResult extends UmbEmbeddedMediaModalData {
	preview?: string;
	originalWidth: number;
	originalHeight: number;
}

export const UMB_EMBEDDED_MEDIA_MODAL = new UmbModalToken<UmbEmbeddedMediaModalData, UmbEmbeddedMediaModalResult>(
	'Umb.Modal.EmbeddedMedia',
	{
		type: 'sidebar',
		size: 'medium',
	}
);
