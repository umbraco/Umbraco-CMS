const { rest } = window.MockServiceWorker;
import { umbDataTypeData } from '../../data/data-type.data';
import { slug } from './slug';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const detailHandlers = [
	rest.post(umbracoPath(`${slug}`), async (req, res, ctx) => {
		const data = await req.json();
		if (!data) return;

		umbDataTypeData.insert(data);

		return res(ctx.status(200));
	}),

	rest.get(umbracoPath(`${slug}/:id`), (req, res, ctx) => {
		const id = req.params.id as string;
		if (!id) return;

		const dataType = umbDataTypeData.getById(id);

		return res(ctx.status(200), ctx.json(dataType));
	}),

	rest.put(umbracoPath(`${slug}/:id`), async (req, res, ctx) => {
		const data = await req.json();
		if (!data) return;

		const saved = umbDataTypeData.save(data);

		return res(ctx.status(200), ctx.json(saved));
	}),

	rest.delete<string>(umbracoPath(`${slug}/:id`), async (req, res, ctx) => {
		const id = req.params.id as string;
		if (!id) return;

		umbDataTypeData.delete([id]);

		return res(ctx.status(200));
	}),
];
