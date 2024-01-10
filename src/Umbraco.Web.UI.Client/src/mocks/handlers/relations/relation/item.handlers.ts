const { rest } = window.MockServiceWorker;
import { umbRelationData } from '../../../data/relations/relation.data.js';
import { UMB_SLUG } from './slug.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const handlers = [
	rest.get(umbracoPath(`${UMB_SLUG}/type/:id`), (req, res, ctx) => {
		const id = req.params.id as string;
		if (!id) return;

		const response = umbRelationData.getRelationsByParentId(id);

		return res(ctx.status(200), ctx.json(response));
	}),
];
