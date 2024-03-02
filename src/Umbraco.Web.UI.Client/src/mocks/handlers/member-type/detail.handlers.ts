const { rest } = window.MockServiceWorker;
import { umbMemberTypeMockDb } from '../../data/member-type/member-type.db.js';
import { UMB_SLUG } from './slug.js';
import type {
	CreateMemberTypeRequestModel,
	UpdateMemberTypeRequestModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const detailHandlers = [
	rest.post(umbracoPath(`${UMB_SLUG}`), async (req, res, ctx) => {
		const requestBody = (await req.json()) as CreateMemberTypeRequestModel;
		if (!requestBody) return res(ctx.status(400, 'no body found'));

		const id = umbMemberTypeMockDb.detail.create(requestBody);

		return res(
			ctx.status(201),
			ctx.set({
				Location: req.url.href + '/' + id,
				'Umb-Generated-Resource': id,
			}),
		);
	}),

	rest.get(umbracoPath(`${UMB_SLUG}/:id`), (req, res, ctx) => {
		const id = req.params.id as string;
		if (!id) return res(ctx.status(400));
		const response = umbMemberTypeMockDb.detail.read(id);
		return res(ctx.status(200), ctx.json(response));
	}),

	rest.put(umbracoPath(`${UMB_SLUG}/:id`), async (req, res, ctx) => {
		const id = req.params.id as string;
		if (!id) return res(ctx.status(400));
		const requestBody = (await req.json()) as UpdateMemberTypeRequestModel;
		if (!requestBody) return res(ctx.status(400, 'no body found'));
		umbMemberTypeMockDb.detail.update(id, requestBody);
		return res(ctx.status(200));
	}),

	rest.delete(umbracoPath(`${UMB_SLUG}/:id`), (req, res, ctx) => {
		const id = req.params.id as string;
		if (!id) return res(ctx.status(400));
		umbMemberTypeMockDb.detail.delete(id);
		return res(ctx.status(200));
	}),
];
