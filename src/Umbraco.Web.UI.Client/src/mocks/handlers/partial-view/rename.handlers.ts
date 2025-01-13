const { rest } = window.MockServiceWorker;
import { umbPartialViewMockDB } from '../../data/partial-view/partial-view.db.js';
import { UMB_SLUG } from './slug.js';
import type { RenamePartialViewRequestModel } from '@umbraco-cms/backoffice/external/backend-api';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const renameHandlers = [
	rest.put(umbracoPath(`${UMB_SLUG}/:path/rename`), async (req, res, ctx) => {
		const path = req.params.path as string;
		if (!path) return res(ctx.status(400));

		const requestBody = (await req.json()) as RenamePartialViewRequestModel;
		if (!requestBody) return res(ctx.status(400, 'no body found'));

		const newPath = umbPartialViewMockDB.file.rename(decodeURIComponent(path), requestBody.name);
		const encodedPath = encodeURIComponent(newPath);

		return res(
			ctx.status(201),
			ctx.set({
				Location: req.url.href + '/' + encodedPath,
				'Umb-Generated-Resource': encodedPath,
			}),
		);
	}),
];
