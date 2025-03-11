const { rest } = window.MockServiceWorker;
import { createProblemDetails } from '../../data/utils.js';
import { umbDocumentMockDb } from '../../data/document/document.db.js';
import { UMB_SLUG } from './slug.js';
import type {
	GetDocumentByIdPublishedResponse,
	PublishDocumentRequestModel,
	UnpublishDocumentRequestModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const publishingHandlers = [
	rest.put(umbracoPath(`${UMB_SLUG}/:id/publish`), async (req, res, ctx) => {
		const id = req.params.id as string;
		if (!id) return res(ctx.status(400));
		const requestBody = (await req.json()) as PublishDocumentRequestModel;
		if (!requestBody) return res(ctx.status(400, 'no body found'));

		try {
			umbDocumentMockDb.publishing.publish(id, requestBody);
			return res(ctx.status(200));
		} catch (error) {
			if (error instanceof Error) {
				return res(ctx.status(400), ctx.json(createProblemDetails({ title: 'Schedule', detail: error.message })));
			}
			throw new Error('An error occurred while publishing the document');
		}
	}),

	rest.put(umbracoPath(`${UMB_SLUG}/:id/unpublish`), async (req, res, ctx) => {
		const id = req.params.id as string;
		if (!id) return res(ctx.status(400));
		const requestBody = (await req.json()) as UnpublishDocumentRequestModel;
		if (!requestBody) return res(ctx.status(400, 'no body found'));
		umbDocumentMockDb.publishing.unpublish(id, requestBody);
		return res(ctx.status(200));
	}),

	rest.get(umbracoPath(`${UMB_SLUG}/:id/published`), async (req, res, ctx) => {
		const id = req.params.id as string;
		if (!id) return res(ctx.status(400));

		const document = umbDocumentMockDb.detail.read(id);

		if (!document) return res(ctx.status(404));

		const responseModel: GetDocumentByIdPublishedResponse = {
			documentType: document.documentType,
			id: document.id,
			isTrashed: document.isTrashed,
			urls: document.urls,
			values: document.values,
			variants: document.variants,
			template: document.template,
		};

		return res(ctx.status(200), ctx.json<GetDocumentByIdPublishedResponse>(responseModel));
	}),
];
