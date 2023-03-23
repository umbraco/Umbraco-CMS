import { rest } from "msw";
import { umbracoPath } from "@umbraco-cms/backoffice/utils";
import { OEmbedResult, OEmbedStatus } from "../../../backoffice/shared/modals/embedded-media";

export const handlers = [
	rest.get(umbracoPath('/rteembed'), (req, res, ctx) => {
		const width = req.url.searchParams.get('width') ?? 360;
		const height = req.url.searchParams.get('height') ?? 240;
		const response: OEmbedResult = {
			supportsDimensions: true,			
			markup: `<iframe width="${width}" height="${height}" src="https://www.youtube.com/embed/wJNbtYdr-Hg?feature=oembed" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; web-share" allowfullscreen title="Sleep Token - The Summoning"></iframe>`,
			oEmbedStatus: OEmbedStatus.Success,			
		};

		return res(ctx.status(200), ctx.json(response));
	}),
];