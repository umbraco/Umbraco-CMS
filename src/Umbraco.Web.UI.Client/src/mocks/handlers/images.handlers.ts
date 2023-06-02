import { rest } from 'msw';

// TODO: add schema
export const handlers = [
	rest.get('/umbraco/management/api/v1/images/getprocessedimageurl', (req, res, ctx) => {
		return res(ctx.status(200), ctx.json('/url/to/processed/image'));
	}),
];
