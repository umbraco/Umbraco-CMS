const { rest } = window.MockServiceWorker;
import { createProblemDetails } from '../../data/utils.js';
import { umbRelationTypeMockDb } from '../../data/relation-type/relationType.db.js';
import { UMB_SLUG } from './slug.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const detailHandlers = [
	rest.get(umbracoPath(`${UMB_SLUG}`), (req, res, ctx) => {
		const skipParam = req.url.searchParams.get('skip');
		const skip = skipParam ? Number.parseInt(skipParam) : undefined;
		const takeParam = req.url.searchParams.get('take');
		const take = takeParam ? Number.parseInt(takeParam) : undefined;

		const response = umbRelationTypeMockDb.get({ skip, take });

		return res(ctx.status(200), ctx.json(response));
	}),

	rest.get(umbracoPath(`${UMB_SLUG}/:id`), (req, res, ctx) => {
		const id = req.params.id as string;
		if (!id) return res(ctx.status(400));
		const response = umbRelationTypeMockDb.detail.read(id);
		return res(ctx.status(200), ctx.json(response));
	}),

	rest.delete(umbracoPath(`${UMB_SLUG}/:id`), (req, res, ctx) => {
		const id = req.params.id as string;
		if (!id) return res(ctx.status(400));
		const relationType = umbRelationTypeMockDb.detail.read(id);
		if (!relationType) return res(ctx.status(404));
		if (!relationType.isDeletable)
			return res(
				ctx.status(400),
				ctx.json(createProblemDetails({ title: 'Validation', detail: 'Cannot delete a non-deletable relation type' })),
			);
		umbRelationTypeMockDb.detail.delete(id);
		return res(ctx.status(200));
	}),
];
