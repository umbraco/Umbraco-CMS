const { rest } = window.MockServiceWorker;
import { umbObjectTypeData } from '../../data/object-type/object-type.data.js';
import { UMB_SLUG } from './slug.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const handlers = [
	rest.get(umbracoPath(`${UMB_SLUG}`), (req, res, ctx) => {
		const response = umbObjectTypeData.getAll();
		return res(ctx.status(200), ctx.json(response));
	}),
];
