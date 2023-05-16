const { rest } = window.MockServiceWorker;
import { OEmbedResult, OEmbedStatus } from '@umbraco-cms/backoffice/modal';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const handlers = [
	rest.get(umbracoPath('/rteembed'), (req, res, ctx) => {
		const widthParam = req.url.searchParams.get('width');
		const width = widthParam ? parseInt(widthParam) : 360;

		const heightParam = req.url.searchParams.get('height');
		const height = heightParam ? parseInt(heightParam) : 240;

		const response: OEmbedResult = {
			supportsDimensions: true,
			markup: `<iframe width="${width}" height="${height}" src="https://www.youtube.com/embed/wJNbtYdr-Hg?feature=oembed" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; web-share" allowfullscreen title="Sleep Token - The Summoning"></iframe>`,
			oEmbedStatus: OEmbedStatus.Success,
			width,
			height,
		};

		return res(ctx.status(200), ctx.json(response));
	}),
];
