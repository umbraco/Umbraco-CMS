import { rest } from 'msw';

import { umbPropertyEditorData } from '../data/property-editor.data';

// TODO: add schema
export const handlers = [
	rest.get('/umbraco/backoffice/property-editors/list', (req, res, ctx) => {
		const propertyEditors = umbPropertyEditorData.getAll();

		return res(ctx.status(200), ctx.json(propertyEditors));
	}),
];
