import { rest } from 'msw';
import { umbMemberGroupData } from '../data/member-group.data';

// TODO: add schema
export const handlers = [
	rest.get('/umbraco/management/api/v1/tree/member-group/root', (req, res, ctx) => {
		const response = umbMemberGroupData.getTreeRoot();
		return res(ctx.status(200), ctx.json(response));
	}),

	rest.get('/umbraco/management/api/v1/tree/member-group/item', (req, res, ctx) => {
		const keys = req.url.searchParams.getAll('key');
		if (!keys) return;

		const items = umbMemberGroupData.getTreeItem(keys);

		return res(ctx.status(200), ctx.json(items));
	}),
];
