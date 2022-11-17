import { rest } from 'msw';
import { umbMemberTypeData } from '../data/member-type.data';

// TODO: add schema
export const handlers = [
	rest.get('/umbraco/management/api/v1/tree/member-type/root', (req, res, ctx) => {
		const response = umbMemberTypeData.getTreeRoot();
		return res(ctx.status(200), ctx.json(response));
	}),

	rest.get('/umbraco/management/api/v1/tree/member-type/item', (req, res, ctx) => {
		const keys = req.params.keys as string;
		if (!keys) return;

		const items = umbMemberTypeData.getTreeItem(keys.split(','));

		return res(ctx.status(200), ctx.json(items));
	}),
];
