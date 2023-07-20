import { rest } from 'msw';
import texts from '../data/texts.json';

export const handlers = [
	rest.get('/umbraco/localizedtext', async (_req, res, ctx) => {
		return res(ctx.status(200), ctx.json(texts));
	})
];
