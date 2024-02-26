const { rest } = window.MockServiceWorker;
import { umbPartialViewMockDB } from '../../data/partial-view/partial-view.db.js';
import { UMB_SLUG } from './slug.js';
import type { CreatePartialViewFolderRequestModel } from '@umbraco-cms/backoffice/external/backend-api';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const folderHandlers = [
	rest.post(umbracoPath(`${UMB_SLUG}/folder`), async (req, res, ctx) => {
		const requestBody = (await req.json()) as CreatePartialViewFolderRequestModel;
		if (!requestBody) return res(ctx.status(400, 'no body found'));
		const path = umbPartialViewMockDB.folder.create(requestBody);
		const encodedPath = encodeURIComponent(path);

		return res(
			ctx.status(201),
			ctx.set({
				Location: req.url.href + '/' + encodedPath,
				'Umb-Generated-Resource': encodedPath,
			}),
		);
	}),

	rest.get(umbracoPath(`${UMB_SLUG}/folder/:path`), (req, res, ctx) => {
		const path = req.params.path as string;
		if (!path) return res(ctx.status(400));
		const response = umbPartialViewMockDB.folder.read(decodeURIComponent(path));
		return res(ctx.status(200), ctx.json(response));
	}),

	rest.delete(umbracoPath(`${UMB_SLUG}/folder/:path`), (req, res, ctx) => {
		const path = req.params.path as string;
		if (!path) return res(ctx.status(400));
		umbPartialViewMockDB.folder.delete(decodeURIComponent(path));
		return res(ctx.status(200));
	}),
];
