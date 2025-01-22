const { rest } = window.MockServiceWorker;
import { createProblemDetails } from '../../data/utils.js';
import { umbTemplateMockDb } from '../../data/template/template.db.js';
import { UMB_SLUG } from './slug.js';
import type {
	CreateTemplateRequestModel,
	UpdateTemplateRequestModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const detailHandlers = [
	rest.post(umbracoPath(`${UMB_SLUG}`), async (req, res, ctx) => {
		const requestBody = (await req.json()) as CreateTemplateRequestModel;
		if (!requestBody) return res(ctx.status(400, 'no body found'));

		// Validate name and alias
		if (!requestBody.name || !requestBody.alias) {
			return res(
				ctx.status(400, 'name and alias are required'),
				ctx.json(createProblemDetails({ title: 'Validation', detail: 'name and alias are required' })),
			);
		}

		const id = umbTemplateMockDb.detail.create(requestBody);

		return res(
			ctx.status(201),
			ctx.set({
				Location: req.url.href + '/' + id,
				'Umb-Generated-Resource': id,
			}),
		);
	}),

	rest.get(umbracoPath(`${UMB_SLUG}/scaffold`), (req, res, ctx) => {
		const response = umbTemplateMockDb.detail.createScaffold();
		return res(ctx.status(200), ctx.json(response));
	}),

	rest.get(umbracoPath(`${UMB_SLUG}/:id`), (req, res, ctx) => {
		const id = req.params.id as string;
		if (!id) return res(ctx.status(400));
		const response = umbTemplateMockDb.detail.read(id);
		return res(ctx.status(200), ctx.json(response));
	}),

	rest.put(umbracoPath(`${UMB_SLUG}/:id`), async (req, res, ctx) => {
		const id = req.params.id as string;
		if (!id) return res(ctx.status(400));
		const requestBody = (await req.json()) as UpdateTemplateRequestModel;
		if (!requestBody) return res(ctx.status(400, 'no body found'));

		// Validate name and alias
		if (!requestBody.name || !requestBody.alias) {
			return res(
				ctx.status(400, 'name and alias are required'),
				ctx.json(createProblemDetails({ title: 'Validation', detail: 'name and alias are required' })),
			);
		}

		umbTemplateMockDb.detail.update(id, requestBody);
		return res(ctx.status(200));
	}),

	rest.delete(umbracoPath(`${UMB_SLUG}/:id`), (req, res, ctx) => {
		const id = req.params.id as string;
		if (!id) return res(ctx.status(400));
		umbTemplateMockDb.detail.delete(id);
		return res(ctx.status(200));
	}),
];
