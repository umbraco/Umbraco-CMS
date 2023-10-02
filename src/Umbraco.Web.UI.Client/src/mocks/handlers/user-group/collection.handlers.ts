const { rest } = window.MockServiceWorker;
import { umbUserGroupData } from '../../data/user-group.data.js';
import { slug } from './slug.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const collectionHandlers = [
	rest.get(umbracoPath(`${slug}`), (req, res, ctx) => {
		const response = umbUserGroupData.getAll();
		return res(ctx.status(200), ctx.json(response));
	}),
];
