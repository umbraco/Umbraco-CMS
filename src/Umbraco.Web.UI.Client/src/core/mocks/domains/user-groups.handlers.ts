import { rest } from 'msw';
import { umbUserGroupsData } from '../data/user-groups.data';
import type { UserGroupDetails } from '../../../backoffice/users/user-groups/types';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

const slug = '/user-group';

export const handlers = [
	rest.get(umbracoPath(`${slug}`), (req, res, ctx) => {
		const response = umbUserGroupsData.getAll();
		console.log(response);

		return res(ctx.status(200), ctx.json(response));
	}),
];
