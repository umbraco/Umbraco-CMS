const { rest } = window.MockServiceWorker;
import { umbRelationTypeMockDb } from '../../data/relation-type/relationType.db.js';
import { UMB_SLUG } from './slug.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';
import type {
	GetRelationTypeByIdResponse,
	GetRelationTypeResponse,
} from '@umbraco-cms/backoffice/external/backend-api';

export const detailHandlers = [
	rest.get(umbracoPath(`${UMB_SLUG}`), (req, res, ctx) => {
		const skipParam = req.url.searchParams.get('skip');
		const skip = skipParam ? Number.parseInt(skipParam) : undefined;
		const takeParam = req.url.searchParams.get('take');
		const take = takeParam ? Number.parseInt(takeParam) : undefined;

		const response = umbRelationTypeMockDb.get({ skip, take });

		return res(ctx.status(200), ctx.json<GetRelationTypeResponse>(response));
	}),

	rest.get(umbracoPath(`${UMB_SLUG}/:id`), (req, res, ctx) => {
		const id = req.params.id as string;
		if (!id) return res(ctx.status(400));
		const response = umbRelationTypeMockDb.detail.read(id);
		return res(ctx.status(200), ctx.json<GetRelationTypeByIdResponse>(response));
	}),
];
