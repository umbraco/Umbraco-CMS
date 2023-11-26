const { rest } = window.MockServiceWorker;
import { umbDataTypeData } from '../../data/data-type.data.js';
import { UMB_SLUG } from './slug.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const copyHandlers = [
	rest.post(umbracoPath(`${UMB_SLUG}/:id/copy`), async (req, res, ctx) => {
		const id = req.params.id as string;
		if (!id) return;

		const data = await req.json();
		if (!data) return;

		const newIds = umbDataTypeData.copy([id], data.targetId);

		return res(ctx.status(200), ctx.set({ Location: newIds[0] }));
	}),
];
