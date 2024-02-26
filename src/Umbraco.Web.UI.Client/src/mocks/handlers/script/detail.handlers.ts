const { rest } = window.MockServiceWorker;
import { umbScriptMockDb } from '../../data/script/script.db.js';
import { UMB_SLUG } from './slug.js';
import type {
	CreateStylesheetRequestModel,
	UpdateStylesheetRequestModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const detailHandlers = [
	rest.post(umbracoPath(UMB_SLUG), async (req, res, ctx) => {
		const requestBody = (await req.json()) as CreateStylesheetRequestModel;
		if (!requestBody) return res(ctx.status(400, 'no body found'));
		const path = umbScriptMockDb.file.create(requestBody);
		const encodedPath = encodeURIComponent(path);
		return res(
			ctx.status(201),
			ctx.set({
				Location: req.url.href + '/' + encodedPath,
				'Umb-Generated-Resource': encodedPath,
			}),
		);
	}),

	rest.get(umbracoPath(`${UMB_SLUG}/:path`), (req, res, ctx) => {
		const path = req.params.path as string;
		if (!path) return res(ctx.status(400));
		const response = umbScriptMockDb.file.read(decodeURIComponent(path));
		return res(ctx.status(200), ctx.json(response));
	}),

	rest.delete(umbracoPath(`${UMB_SLUG}/:path`), (req, res, ctx) => {
		const path = req.params.path as string;
		if (!path) return res(ctx.status(400));
		umbScriptMockDb.file.delete(decodeURIComponent(path));
		return res(ctx.status(200));
	}),

	rest.put(umbracoPath(`${UMB_SLUG}/:path`), async (req, res, ctx) => {
		const path = req.params.path as string;
		if (!path) return res(ctx.status(400));
		const requestBody = (await req.json()) as UpdateStylesheetRequestModel;
		if (!requestBody) return res(ctx.status(400, 'no body found'));
		umbScriptMockDb.file.update(decodeURIComponent(path), requestBody);
		return res(ctx.status(200));
	}),
];
