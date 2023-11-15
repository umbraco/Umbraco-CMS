const { rest } = window.MockServiceWorker;
import { umbDocumentTypeData } from '../../data/document-type.data.js';
import { slug } from './slug.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const handlers = [
	rest.get(umbracoPath(`${slug}/:id`), (req, res, ctx) => {
		const id = req.params.id as string;
		if (!id) return;
		const document = umbDocumentTypeData.getById(id);
		return res(ctx.status(200), ctx.json(document));
	}),

	rest.delete(umbracoPath(`${slug}/:id`), (req, res, ctx) => {
		const id = req.params.id as string;
		if (!id) return;
		const document = umbDocumentTypeData.getById(id);
		return res(ctx.status(200), ctx.json(document));
	}),

	rest.put(umbracoPath(`${slug}/:id`), async (req, res, ctx) => {
		const id = req.params.id as string;
		if (!id) return;
		const data = await req.json();
		if (!data) return;
		const saved = umbDocumentTypeData.save(id, data);
		return res(ctx.status(200), ctx.json(saved));
	}),

	rest.post(umbracoPath(`${slug}`), async (req, res, ctx) => {
		const data = await req.json();
		if (!data) return;
		umbDocumentTypeData.insert(data);
		return res(ctx.status(200));
	}),
];
