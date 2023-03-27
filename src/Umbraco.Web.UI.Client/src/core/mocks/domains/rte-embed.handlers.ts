import { rest } from "msw";
import { OEmbedResult, OEmbedStatus } from "../../../backoffice/shared/modals/embedded-media";
import { UmbEmbeddedMediaModalElement as ModalElement } from "../../../../src/backoffice/shared/modals/embedded-media/embedded-media-modal.element";
import { umbracoPath } from "@umbraco-cms/backoffice/utils";

export const handlers = [
	rest.get(umbracoPath('/rteembed'), (req, res, ctx) => {
		const width = req.url.searchParams.get('width') ?? ModalElement.defaultWidth;
		const height = req.url.searchParams.get('height') ?? ModalElement.defaultHeight;
		const url = req.url.searchParams.get('url');

		let embedUrl = 'https://www.youtube.com/embed/wJNbtYdr-Hg';

		if (url?.startsWith('https://youtu.be')) {
			embedUrl = embedUrl.replace('wJNbtYdr-Hg', url.substring(url.lastIndexOf('/') + 1))
		}

		const response: OEmbedResult = {
			width,
			height,
			supportsDimensions: true,			
			markup: `<iframe width="${width}" height="${height}" src="${embedUrl}?feature=oembed" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; web-share" allowfullscreen title="Sleep Token - The Summoning"></iframe>`,
			oEmbedStatus: OEmbedStatus.Success,			
		};

		return res(ctx.status(200), ctx.json(response));
	}),
];