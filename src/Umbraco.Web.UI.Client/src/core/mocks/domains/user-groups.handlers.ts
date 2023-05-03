import { rest } from 'msw';
import { umbUserGroupsData } from '../data/user-groups.data';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

const slug = '/user-group';

export const handlers = [
	rest.get(umbracoPath(`${slug}`), (req, res, ctx) => {
		const response = umbUserGroupsData.getAll();

		return res(ctx.status(200), ctx.json(response));
	}),
];
