const { rest } = window.MockServiceWorker;
import { umbStylesheetData } from '../../data/stylesheet/stylesheet.db.js';
import { UMB_SLUG } from './slug.js';
import { CreateStylesheetFolderRequestModel } from '@umbraco-cms/backoffice/backend-api';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const folderHandlers = [
	rest.post(umbracoPath(`${UMB_SLUG}/folder`), async (req, res, ctx) => {
		const requestBody = (await req.json()) as CreateStylesheetFolderRequestModel;
		if (!requestBody) return res(ctx.status(400, 'no body found'));

		const path = umbStylesheetData.folder.create(requestBody);

		return res(
			ctx.status(201),
			ctx.set({
				Location: path,
			}),
		);
	}),

	rest.get(umbracoPath(`${UMB_SLUG}/folder/:path`), (req, res, ctx) => {
		const path = req.params.path as string;
		if (!path) return res(ctx.status(400));
		const response = umbStylesheetData.folder.read(path);
		return res(ctx.status(200), ctx.json(response));
	}),

	rest.delete(umbracoPath(`${UMB_SLUG}/folder/:path`), (req, res, ctx) => {
		const path = req.params.path as string;
		if (!path) return res(ctx.status(400));
		umbStylesheetData.folder.delete(path);
		return res(ctx.status(200));
	}),
];
