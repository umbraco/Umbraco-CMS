import { rest } from 'msw';
import umbracoPath from '../../helpers/umbraco-path';
import { umbPropertyEditorData } from '../data/property-editor.data';
import type {
	PropertyEditorsListResponse,
	PropertyEditorResponse,
	PropertyEditorConfigResponse,
} from '@umbraco-cms/models';

// TODO: add schema
export const handlers = [
	rest.get(umbracoPath('/property-editors/list'), (req, res, ctx) => {
		const propertyEditors = umbPropertyEditorData.getAll();

		const response = {
			propertyEditors,
		};

		return res(ctx.status(200), ctx.json<PropertyEditorsListResponse>(response));
	}),

	rest.get('/umbraco/backoffice/property-editors/property-editor/:alias', (req, res, ctx) => {
		const alias = req.params.alias as string;
		if (!alias) return;

		const propertyEditor = umbPropertyEditorData.getByAlias(alias);
		if (propertyEditor) {
			return res(ctx.status(200), ctx.json<PropertyEditorResponse>(propertyEditor));
		} else {
			return res(ctx.status(404));
		}
	}),

	rest.get('/umbraco/backoffice/property-editors/property-editor/config/:alias', (req, res, ctx) => {
		const alias = req.params.alias as string;
		if (!alias) return;

		const config = umbPropertyEditorData.getConfig(alias);
		if (!config) return;

		return res(ctx.status(200), ctx.json<PropertyEditorConfigResponse>(config));
	}),
];
