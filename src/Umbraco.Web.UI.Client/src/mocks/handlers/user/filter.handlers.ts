const { rest } = window.MockServiceWorker;
import { umbUsersData } from '../../data/user.data.js';
import { slug } from './slug.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const handlers = [
  rest.get(umbracoPath(`${slug}/filter`), (req, res, ctx) => {

    const filter = {
      skip: Number(req.url.searchParams.get('skip')),
      take: Number(req.url.searchParams.get('take')),
      orderBy: req.url.searchParams.get('orderBy'),
      orderDirection: req.url.searchParams.get('orderDirection'),
      userGroupIds: req.url.searchParams.getAll('userGroupIds'),
      userStates: req.url.searchParams.getAll('userStates'),
      filter: req.url.searchParams.get('filter'),
    };

    const response = umbUsersData.filter(filter);
    return res(ctx.status(200), ctx.json(response));
  }),
];
