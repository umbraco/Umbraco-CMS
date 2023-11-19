const { rest } = window.MockServiceWorker;
import { umbDataTypeData } from '../../data/data-type.data.js';
import { UMB_slug } from './slug.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const moveHandlers = [
	rest.post(umbracoPath(`${UMB_slug}/:id/move`), async (req, res, ctx) => {
		const id = req.params.id as string;
		if (!id) return;

		const data = await req.json();
		if (!data) return;

		umbDataTypeData.move([id], data.targetId);

		return res(ctx.status(200));
	}),
];
