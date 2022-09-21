import { rest } from 'msw';

import { umbPropertyEditorData } from '../data/property-editor.data';
import { umbPropertyEditorConfigData } from '../data/property-editor-config.data';

// TODO: add schema
export const handlers = [
	rest.get('/umbraco/backoffice/property-editors/list', (req, res, ctx) => {
		const propertyEditors = umbPropertyEditorData.getAll();

		return res(ctx.status(200), ctx.json(propertyEditors));
	}),

	rest.get('/umbraco/backoffice/property-editors/property-editor/:alias', (req, res, ctx) => {
		console.warn('Please move to schema');
		const alias = req.params.alias as string;
		if (!alias) return;

		const propertyEditor = umbPropertyEditorData.getByAlias(alias);

		return res(ctx.status(200), ctx.json([propertyEditor]));
	}),

	rest.get('/umbraco/backoffice/property-editors/config/:alias', (req, res, ctx) => {
		console.warn('Please move to schema');
		const alias = req.params.alias as string;
		if (!alias) return;

		const propertyEditorConfig = umbPropertyEditorConfigData.getByAlias(alias);

		return res(ctx.status(200), ctx.json([propertyEditorConfig]));
	}),
];
