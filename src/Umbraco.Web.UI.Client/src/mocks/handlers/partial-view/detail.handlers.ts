const { rest } = window.MockServiceWorker;
import { umbPartialViewMockDB } from '../../data/partial-view/partial-view.db.js';
import { UMB_SLUG } from './slug.js';
import { CreateStylesheetRequestModel, UpdateStylesheetRequestModel } from '@umbraco-cms/backoffice/backend-api';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const detailHandlers = [
	rest.post(umbracoPath(UMB_SLUG), async (req, res, ctx) => {
		const requestBody = (await req.json()) as CreateStylesheetRequestModel;
		if (!requestBody) return res(ctx.status(400, 'no body found'));
		const path = umbPartialViewMockDB.file.create(requestBody);
		return res(
			ctx.status(201),
			ctx.set({
				Location: path,
			}),
		);
	}),

	rest.get(umbracoPath(`${UMB_SLUG}/:path`), (req, res, ctx) => {
		const path = req.params.path as string;
		if (!path) return res(ctx.status(400));
		const response = umbPartialViewMockDB.file.read(path);
		return res(ctx.status(200), ctx.json(response));
	}),

	rest.delete(umbracoPath(`${UMB_SLUG}/:path`), (req, res, ctx) => {
		const path = req.params.path as string;
		if (!path) return res(ctx.status(400));
		umbPartialViewMockDB.file.delete(path);
		return res(ctx.status(200));
	}),

	rest.put(umbracoPath(`${UMB_SLUG}/:path`), async (req, res, ctx) => {
		const path = req.params.path as string;
		if (!path) return res(ctx.status(400));
		const requestBody = (await req.json()) as UpdateStylesheetRequestModel;
		if (!requestBody) return res(ctx.status(400, 'no body found'));
		umbPartialViewMockDB.file.update(path, requestBody);
		return res(ctx.status(200));
	}),
];
