const { rest } = window.MockServiceWorker;
import { umbDocumentMockDb } from '../../data/document/document.db.js';
import { UMB_SLUG } from './slug.js';
import type { PublishDocumentRequestModel, UnpublishDocumentRequestModel } from '@umbraco-cms/backoffice/backend-api';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const publishingHandlers = [
	rest.put(umbracoPath(`${UMB_SLUG}/:id/publish`), async (req, res, ctx) => {
		const id = req.params.id as string;
		if (!id) return res(ctx.status(400));
		const requestBody = (await req.json()) as PublishDocumentRequestModel;
		if (!requestBody) return res(ctx.status(400, 'no body found'));
		umbDocumentMockDb.publishing.publish(id, requestBody);
		return res(ctx.status(200));
	}),

	rest.put(umbracoPath(`${UMB_SLUG}/:id/unpublish`), async (req, res, ctx) => {
		const id = req.params.id as string;
		if (!id) return res(ctx.status(400));
		const requestBody = (await req.json()) as UnpublishDocumentRequestModel;
		if (!requestBody) return res(ctx.status(400, 'no body found'));
		umbDocumentMockDb.publishing.unpublish(id, requestBody);
		return res(ctx.status(200));
	}),
];
