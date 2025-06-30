const { rest } = window.MockServiceWorker;
import { umbMemberGroupMockDb } from '../../data/member-group/member-group.db.js';
import { UMB_SLUG } from './slug.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const detailHandlers = [
	rest.post(umbracoPath(`${UMB_SLUG}`), async (req, res, ctx) => {
		const requestBody = (await req.json()) as any;
		if (!requestBody) return res(ctx.status(400, 'no body found'));

		const id = umbMemberGroupMockDb.detail.create(requestBody);

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

		const response = umbMemberGroupMockDb.get({ skip, take });

		return res(ctx.status(200), ctx.json(response));
	}),

	rest.get(umbracoPath(`${UMB_SLUG}/:id`), (req, res, ctx) => {
		const id = req.params.id as string;
		if (!id) return res(ctx.status(400));
		if (id === 'forbidden') {
			// Simulate a forbidden response
			return res(ctx.status(403));
		}
		const response = umbMemberGroupMockDb.detail.read(id);
		return res(ctx.status(200), ctx.json(response));
	}),

	rest.put(umbracoPath(`${UMB_SLUG}/:id`), async (req, res, ctx) => {
		const id = req.params.id as string;
		if (!id) return res(ctx.status(400));
		if (id === 'forbidden') {
			// Simulate a forbidden response
			return res(ctx.status(403));
		}
		const requestBody = (await req.json()) as any;
		if (!requestBody) return res(ctx.status(400, 'no body found'));
		umbMemberGroupMockDb.detail.update(id, requestBody);
		return res(ctx.status(200));
	}),

	rest.delete(umbracoPath(`${UMB_SLUG}/:id`), (req, res, ctx) => {
		const id = req.params.id as string;
		if (!id) return res(ctx.status(400));
		if (id === 'forbidden') {
			// Simulate a forbidden response
			return res(ctx.status(403));
		}
		umbMemberGroupMockDb.detail.delete(id);
		return res(ctx.status(200));
	}),
];
