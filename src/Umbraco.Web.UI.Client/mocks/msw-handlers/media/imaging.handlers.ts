const { http, HttpResponse } = window.MockServiceWorker;
import { umbMediaMockDb } from '../../db/media.db.js';
import { getMediaFileUrl } from './utils.js';
import type { GetImagingResizeUrlsResponse, MediaUrlInfoModel } from '@umbraco-cms/backoffice/external/backend-api';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

/**
 * Builds a resize URL by appending width, height, mode, and format query parameters to the source path.
 * @param {string} src The original media file URL.
 * @param {URLSearchParams} params The request query parameters containing resize options.
 * @returns {string} The source URL with resize query parameters appended.
 */
function buildResizeUrl(src: string, params: URLSearchParams): string {
	const height = params.get('height') || '200';
	const width = params.get('width') || '200';
	const mode = params.get('mode');
	const format = params.get('format');

	const resizeParams = new URLSearchParams();
	resizeParams.set('width', width);
	resizeParams.set('height', height);
	if (mode) resizeParams.set('mode', mode);
	if (format) resizeParams.set('format', format);

	return `${src}?${resizeParams.toString()}`;
}

export const imagingHandlers = [
	http.get(umbracoPath('/imaging/resize/urls'), ({ request }) => {
		const url = new URL(request.url);
		const ids = url.searchParams.getAll('id');
		if (!ids.length) return new HttpResponse(null, { status: 400 });

		const media = umbMediaMockDb.getAll().filter((item) => ids.includes(item.id));

		const response: GetImagingResizeUrlsResponse = media.map((item) => {
			const src = getMediaFileUrl(item);
			const urlInfos: Array<MediaUrlInfoModel> = [];

			if (src) {
				urlInfos.push({
					culture: null,
					url: buildResizeUrl(src, url.searchParams),
				});
			}

			return { id: item.id, urlInfos };
		});

		return HttpResponse.json(response);
	}),
];
