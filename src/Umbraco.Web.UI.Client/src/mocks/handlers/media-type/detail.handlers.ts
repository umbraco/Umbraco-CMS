const { rest } = window.MockServiceWorker;
import { umbMediaTypeData } from '../../data/media-type.data.js';
import { UMB_SLUG } from './slug.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const detailHandlers = [
	rest.post(umbracoPath(`${UMB_SLUG}`), async (req, res, ctx) => {
		const data = await req.json();
		if (!data) return;

		umbMediaTypeData.insert(data);

		return res(ctx.status(200));
	}),

	rest.get(umbracoPath(`${UMB_SLUG}/:id`), (req, res, ctx) => {
		const id = req.params.id as string;
		if (!id) return;

		const data = umbMediaTypeData.getById(id);

		return res(ctx.status(200), ctx.json(data));
	}),

	rest.put(umbracoPath(`${UMB_SLUG}/:id`), async (req, res, ctx) => {
		const id = req.params.id as string;
		if (!id) return;
		const data = await req.json();
		if (!data) return;

		umbMediaTypeData.save(id, data);

		return res(ctx.status(200));
	}),

	rest.delete<string>(umbracoPath(`${UMB_SLUG}/:id`), async (req, res, ctx) => {
		const id = req.params.id as string;
		if (!id) return;

		umbMediaTypeData.delete([id]);

		return res(ctx.status(200));
	}),
];
