const { rest } = window.MockServiceWorker;
import type { OEmbedResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const handlers = [
	rest.get(umbracoPath('/oembed/query'), (req, res, ctx) => {
		const widthParam = req.url.searchParams.get('maxWidth');
		const width = widthParam ? parseInt(widthParam) : 360;

		const heightParam = req.url.searchParams.get('maxHeight');
		const height = heightParam ? parseInt(heightParam) : 240;

		const response: OEmbedResponseModel = {
			markup: `<iframe width="${width}" height="${height}" src="https://www.youtube.com/embed/wJNbtYdr-Hg?feature=oembed" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; web-share" allowfullscreen title="Sleep Token - The Summoning"></iframe>`,
		};

		return res(ctx.status(200), ctx.json(response));
	}),
];
