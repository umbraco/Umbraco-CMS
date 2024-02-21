const { rest } = window.MockServiceWorker;
import { umbUserGroupMockDb } from '../../data/user-group/user-group.db.js';
import { UMB_SLUG } from './slug.js';
import type {
	CreateUserGroupRequestModel,
	UpdateUserGroupRequestModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const detailHandlers = [
	rest.post(umbracoPath(`${UMB_SLUG}`), async (req, res, ctx) => {
		const requestBody = (await req.json()) as CreateUserGroupRequestModel;
		if (!requestBody) return res(ctx.status(400, 'no body found'));

		const id = umbUserGroupMockDb.detail.create(requestBody);

		return res(
			ctx.status(201),
			ctx.set({
				Location: req.url.href + '/' + id,
				'Umb-Generated-Resource': id,
			}),
		);
	}),

	rest.get(umbracoPath(`${UMB_SLUG}`), (req, res, ctx) => {
		const skipParam = req.url.searchParams.get('skip');
		const skip = skipParam ? Number.parseInt(skipParam) : undefined;
		const takeParam = req.url.searchParams.get('take');
		const take = takeParam ? Number.parseInt(takeParam) : undefined;

		const response = umbUserGroupMockDb.get({ skip, take });
		return res(ctx.status(200), ctx.json(response));
	}),

	rest.get(umbracoPath(`${UMB_SLUG}/:id`), (req, res, ctx) => {
		const id = req.params.id as string;
		if (!id) return res(ctx.status(400));
		const response = umbUserGroupMockDb.detail.read(id);
		return res(ctx.status(200), ctx.json(response));
	}),

	rest.put(umbracoPath(`${UMB_SLUG}/:id`), async (req, res, ctx) => {
		const id = req.params.id as string;
		if (!id) return res(ctx.status(400));
		const requestBody = (await req.json()) as UpdateUserGroupRequestModel;
		if (!requestBody) return res(ctx.status(400, 'no body found'));
		umbUserGroupMockDb.detail.update(id, requestBody);
		return res(ctx.status(200));
	}),

	rest.delete(umbracoPath(`${UMB_SLUG}/:id`), (req, res, ctx) => {
		const id = req.params.id as string;
		if (!id) return res(ctx.status(400));
		umbUserGroupMockDb.detail.delete(id);
		return res(ctx.status(200));
	}),
];
