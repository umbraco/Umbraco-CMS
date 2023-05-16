const { rest } = window.MockServiceWorker;
import { umbDataTypeData } from '../../data/data-type.data';
import { slug } from './slug';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const moveHandlers = [
	rest.post(umbracoPath(`${slug}/:id/move`), async (req, res, ctx) => {
		const id = req.params.id as string;
		if (!id) return;

		const data = await req.json();
		if (!data) return;

		umbDataTypeData.move([id], data.targetId);

		return res(ctx.status(200));
	}),
];
