import { rest } from 'msw';
import { umbMemberData } from '../data/member.data';

// TODO: add schema
export const handlers = [
	rest.get('/umbraco/management/api/v1/tree/member/root', (req, res, ctx) => {
		const response = umbMemberData.getTreeRoot();
		return res(ctx.status(200), ctx.json(response));
	}),

	rest.get('/umbraco/management/api/v1/tree/member/item', (req, res, ctx) => {
		const ids = req.url.searchParams.getAll('id');
		if (!ids) return;

		const items = umbMemberData.getTreeItem(ids);

		return res(ctx.status(200), ctx.json(items));
	}),
];
