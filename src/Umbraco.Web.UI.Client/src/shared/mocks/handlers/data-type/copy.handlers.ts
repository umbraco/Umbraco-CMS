const { rest } = window.MockServiceWorker;
import { umbDataTypeData } from '../../data/data-type.data';
import { slug } from './slug';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const copyHandlers = [
	rest.post(umbracoPath(`${slug}/:id/copy`), async (req, res, ctx) => {
		const id = req.params.id as string;
		if (!id) return;

		const data = await req.json();
		if (!data) return;

		const newIds = umbDataTypeData.copy([id], data.targetId);

		return res(ctx.status(200), ctx.set({ Location: newIds[0] }));
	}),
];
