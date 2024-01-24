const { rest } = window.MockServiceWorker;
import { umbDataTypeMockDb } from '../../data/data-type/data-type.db.js';
import { UMB_SLUG } from './slug.js';
import { MoveDataTypeRequestModel } from '@umbraco-cms/backoffice/backend-api';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const moveHandlers = [
	rest.post(umbracoPath(`${UMB_SLUG}/:id/move`), async (req, res, ctx) => {
		const id = req.params.id as string;
		if (!id) return res(ctx.status(400));

		const requestBody = (await req.json()) as MoveDataTypeRequestModel;
		if (!requestBody) return res(ctx.status(400, 'no body found'));
		if (!requestBody.targetId) return res(ctx.status(400, 'no targetId found'));

		umbDataTypeMockDb.tree.move([id], requestBody.targetId);
		return res(ctx.status(200));
	}),
];
