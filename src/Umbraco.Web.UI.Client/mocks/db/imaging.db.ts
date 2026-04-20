import { umbMediaMockDb, getMediaFileUrl } from './media.db.js';
import type { GetImagingResizeUrlsResponse, MediaUrlInfoModel } from '@umbraco-cms/backoffice/external/backend-api';

export interface UmbImagingResizeParams {
	width: string;
	height: string;
	mode: string | null;
	format: string | null;
}

class UmbImagingMockDb {
	getResizeUrls(ids: string[], params: UmbImagingResizeParams): GetImagingResizeUrlsResponse {
		const media = umbMediaMockDb.getAll().filter((item) => ids.includes(item.id));

		return media.map((item) => {
			const src = getMediaFileUrl(item);
			const urlInfos: Array<MediaUrlInfoModel> = [];

			if (src) {
				urlInfos.push({
					culture: null,
					url: this.#buildResizeUrl(src, params),
				});
			}

			return { id: item.id, urlInfos };
		});
	}

	#buildResizeUrl(src: string, { width, height, mode, format }: UmbImagingResizeParams): string {
		const resizeParams = new URLSearchParams();
		resizeParams.set('width', width);
		resizeParams.set('height', height);
		if (mode) resizeParams.set('mode', mode);
		if (format) resizeParams.set('format', format);

		return `${src}?${resizeParams.toString()}`;
	}
}

export const umbImagingMockDb = new UmbImagingMockDb();
