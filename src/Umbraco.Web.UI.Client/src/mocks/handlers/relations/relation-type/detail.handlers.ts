const { rest } = window.MockServiceWorker;
import { umbRelationTypeData } from '../../../data/relations/relation-type.data.js';
import { UMB_SLUG } from './slug.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

// TODO: add schema
export const handlers = [
	rest.delete<string[]>(umbracoPath(`${UMB_SLUG}/:id`), async (req, res, ctx) => {
		const id = req.params.id as string;
		if (!id) return;

		umbRelationTypeData.delete([id]);

		return res(ctx.status(200));
	}),

	rest.get(umbracoPath(`${UMB_SLUG}/:id`), (req, res, ctx) => {
		const id = req.params.id as string;
		if (!id) return;

		const RelationType = umbRelationTypeData.getById(id);

		return res(ctx.status(200), ctx.json(RelationType));
	}),

	rest.post(umbracoPath(`${UMB_SLUG}/:id`), async (req, res, ctx) => {
		const id = req.params.id as string;
		if (!id) return;
		const data = await req.json();
		if (!data) return;

		umbRelationTypeData.insert(data);

		return res(ctx.status(200));
	}),

	rest.put(umbracoPath(`${UMB_SLUG}/:id`), async (req, res, ctx) => {
		const id = req.params.id as string;
		if (!id) return;
		const data = await req.json();
		if (!data) return;

		umbRelationTypeData.save(id, data);

		return res(ctx.status(200));
	}),
];
