import { rest } from 'msw';
import { tempData } from '../../backoffice/editors/users/views/users/tempData';

// TODO: add schema
export const handlers = [
	rest.get('/umbraco/backoffice/users', (req, res, ctx) => {
		const response = {
			items: tempData,
		};

		return res(ctx.status(200), ctx.json(response));
	}),

	rest.get('/umbraco/backoffice/users/:key', (req, res, ctx) => {
		const key = req.params.key as string;
		if (!key) return;

		const user = tempData.find((x) => x.key === key);

		return res(ctx.status(200), ctx.json([user]));
	}),
];
