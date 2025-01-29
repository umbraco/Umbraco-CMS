const { rest } = window.MockServiceWorker;
import { umbDocumentBlueprintMockDb } from '../../data/document-blueprint/document-blueprint.db.js';
import { UMB_SLUG } from './slug.js';
import type {
	CreateDocumentRequestModel,
	UpdateDocumentRequestModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const detailHandlers = [
	rest.post(umbracoPath(`${UMB_SLUG}`), async (req, res, ctx) => {
		const requestBody = (await req.json()) as CreateDocumentRequestModel;
		if (!requestBody) return res(ctx.status(400, 'no body found'));

		const id = umbDocumentBlueprintMockDb.detail.create(requestBody);

		return res(
			ctx.status(201),
			ctx.set({
				Location: req.url.href + '/' + id,
				'Umb-Generated-Resource': id,
			}),
		);
	}),

	rest.get(umbracoPath(`${UMB_SLUG}/:id`), (req, res, ctx) => {
		const id = req.params.id as string;
		if (!id) return res(ctx.status(400));
		const response = umbDocumentBlueprintMockDb.detail.read(id);
		return res(ctx.status(200), ctx.json(response));
	}),

	rest.put(umbracoPath(`${UMB_SLUG}/:id`), async (req, res, ctx) => {
		const id = req.params.id as string;
		if (!id) return res(ctx.status(400));
		const requestBody = (await req.json()) as UpdateDocumentRequestModel;
		if (!requestBody) return res(ctx.status(400, 'no body found'));
		umbDocumentBlueprintMockDb.detail.update(id, requestBody);
		return res(ctx.status(200));
	}),

	rest.delete(umbracoPath(`${UMB_SLUG}/:id`), (req, res, ctx) => {
		const id = req.params.id as string;
		if (!id) return res(ctx.status(400));
		umbDocumentBlueprintMockDb.detail.delete(id);
		return res(ctx.status(200));
	}),
];
