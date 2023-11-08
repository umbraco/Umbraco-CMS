const { rest } = window.MockServiceWorker;
import { umbMediaTypeData } from '../../data/media-type.data.js';
import { slug } from './slug.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const detailHandlers = [
	rest.post(umbracoPath(`${slug}`), async (req, res, ctx) => {
		const data = await req.json();
		if (!data) return;

		umbMediaTypeData.insert(data);

		return res(ctx.status(200));
	}),

	rest.get(umbracoPath(`${slug}/:id`), (req, res, ctx) => {
		const id = req.params.id as string;
		if (!id) return;

		const data = umbMediaTypeData.getById(id);

		return res(ctx.status(200), ctx.json(data));
	}),

	rest.put(umbracoPath(`${slug}/:id`), async (req, res, ctx) => {
		const id = req.params.id as string;
		if (!id) return;
		const data = await req.json();
		if (!data) return;

		const saved = umbMediaTypeData.save(id, data);

		return res(ctx.status(200), ctx.json(saved));
	}),

	rest.delete<string>(umbracoPath(`${slug}/:id`), async (req, res, ctx) => {
		const id = req.params.id as string;
		if (!id) return;

		umbMediaTypeData.delete([id]);

		return res(ctx.status(200));
	}),
];
