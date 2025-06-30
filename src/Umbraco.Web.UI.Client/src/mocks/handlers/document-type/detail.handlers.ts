const { rest } = window.MockServiceWorker;
import { umbDocumentBlueprintMockDb } from '../../data/document-blueprint/document-blueprint.db.js';
import { umbDocumentTypeMockDb } from '../../data/document-type/document-type.db.js';
import { UMB_SLUG } from './slug.js';
import type {
	CreateMediaTypeRequestModel,
	PagedDocumentTypeBlueprintItemResponseModel,
	UpdateMediaTypeRequestModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const detailHandlers = [
	rest.post(umbracoPath(`${UMB_SLUG}`), async (req, res, ctx) => {
		const requestBody = (await req.json()) as CreateMediaTypeRequestModel;
		if (!requestBody) return res(ctx.status(400, 'no body found'));

		const id = umbDocumentTypeMockDb.detail.create(requestBody);

		return res(
			ctx.status(201),
			ctx.set({
				Location: req.url.href + '/' + id,
				'Umb-Generated-Resource': id,
			}),
		);
	}),

	rest.get(umbracoPath(`${UMB_SLUG}/:id/blueprint`), (req, res, ctx) => {
		const id = req.params.id as string;
		if (!id) return res(ctx.status(400));
		if (id === 'forbidden') {
			// Simulate a forbidden response
			return res(ctx.status(403));
		}

		const relevantBlueprints = umbDocumentBlueprintMockDb
			.getAll()
			.filter((blueprint) => blueprint.documentType.id === id);
		const response: PagedDocumentTypeBlueprintItemResponseModel = {
			total: relevantBlueprints.length,
			items: relevantBlueprints,
		};
		return res(ctx.status(200), ctx.json(response));
	}),

	rest.get(umbracoPath(`${UMB_SLUG}/:id`), (req, res, ctx) => {
		const id = req.params.id as string;
		if (!id) return res(ctx.status(400));
		if (id === 'forbidden') {
			// Simulate a forbidden response
			return res(ctx.status(403));
		}
		const response = umbDocumentTypeMockDb.detail.read(id);
		return res(ctx.status(200), ctx.json(response));
	}),

	rest.put(umbracoPath(`${UMB_SLUG}/:id`), async (req, res, ctx) => {
		const id = req.params.id as string;
		if (!id) return res(ctx.status(400));
		if (id === 'forbidden') {
			// Simulate a forbidden response
			return res(ctx.status(403));
		}
		const requestBody = (await req.json()) as UpdateMediaTypeRequestModel;
		if (!requestBody) return res(ctx.status(400, 'no body found'));
		umbDocumentTypeMockDb.detail.update(id, requestBody);
		return res(ctx.status(200));
	}),

	rest.delete(umbracoPath(`${UMB_SLUG}/:id`), (req, res, ctx) => {
		const id = req.params.id as string;
		if (!id) return res(ctx.status(400));
		if (id === 'forbidden') {
			// Simulate a forbidden response
			return res(ctx.status(403));
		}
		umbDocumentTypeMockDb.detail.delete(id);
		return res(ctx.status(200));
	}),
];
