const { rest } = window.MockServiceWorker;
import { umbMemberGroupData } from '../data/member-group.data';

// TODO: add schema
export const handlers = [
	rest.get('/umbraco/management/api/v1/tree/member-group/root', (req, res, ctx) => {
		const response = umbMemberGroupData.getTreeRoot();
		return res(ctx.status(200), ctx.json(response));
	}),

	rest.get('/umbraco/management/api/v1/tree/member-group/item', (req, res, ctx) => {
		const ids = req.url.searchParams.getAll('id');
		if (!ids) return;

		const items = umbMemberGroupData.getTreeItem(ids);

		return res(ctx.status(200), ctx.json(items));
	}),
];
