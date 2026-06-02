const { http, HttpResponse } = window.MockServiceWorker;
import type { OEmbedResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const handlers = [
	http.get(umbracoPath('/oembed/query'), ({ request }) => {
		const url = new URL(request.url);
		const widthParam = url.searchParams.get('maxWidth');
		const width = widthParam ? parseInt(widthParam) : 360;

		const heightParam = url.searchParams.get('maxHeight');
		const height = heightParam ? parseInt(heightParam) : 240;

		const response: OEmbedResponseModel = {
			markup: `<iframe width="${width}" height="${height}" src="https://www.youtube.com/embed/QRIWz9SotY4?feature=oembed" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; web-share" allowfullscreen title="Deep dive into Rich Text Editor"></iframe>`,
		};

		return HttpResponse.json(response);
	}),
];
