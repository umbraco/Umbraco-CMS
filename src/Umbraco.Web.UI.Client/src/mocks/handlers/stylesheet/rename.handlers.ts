const { rest } = window.MockServiceWorker;
import { umbStylesheetData } from '../../data/stylesheet/stylesheet.db.js';
import { UMB_SLUG } from './slug.js';
import { RenameStylesheetRequestModel } from '@umbraco-cms/backoffice/backend-api';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const renameHandlers = [
	rest.put(umbracoPath(`${UMB_SLUG}/:path/rename`), async (req, res, ctx) => {
		const path = req.params.path as string;
		if (!path) return res(ctx.status(400));

		const requestBody = (await req.json()) as RenameStylesheetRequestModel;
		if (!requestBody) return res(ctx.status(400, 'no body found'));

		const newPath = umbStylesheetData.file.rename(decodeURIComponent(path), requestBody.name);

		return res(
			ctx.status(201),
			ctx.set({
				Location: encodeURIComponent(newPath),
			}),
		);
	}),
];
