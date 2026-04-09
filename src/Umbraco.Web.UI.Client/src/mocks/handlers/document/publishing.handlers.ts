const { http, HttpResponse } = window.MockServiceWorker;
import { createProblemDetails } from '../../data/utils.js';
import { umbDocumentMockDb } from '../../data/document/document.db.js';
import { UMB_SLUG } from './slug.js';
import type {
	PublishWithDescendantsResultModel,
	GetDocumentByIdPublishedResponse,
	PublishDocumentRequestModel,
	PublishDocumentWithDescendantsRequestModel,
	UnpublishDocumentRequestModel,
	GetDocumentByIdPublishWithDescendantsResultByTaskIdResponse,
} from '@umbraco-cms/backoffice/external/backend-api';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const publishingHandlers = [
	http.put(umbracoPath(`${UMB_SLUG}/:id/publish`), async ({ request, params }) => {
		const id = params.id as string;
		if (!id) return new HttpResponse(null, { status: 400 });
		const requestBody = (await request.json()) as PublishDocumentRequestModel;
		if (!requestBody) return new HttpResponse(null, { status: 400, statusText: 'no body found' });

		try {
			umbDocumentMockDb.publishing.publish(id, requestBody);
			return new HttpResponse(null, { status: 200 });
		} catch (error) {
			if (error instanceof Error) {
				return HttpResponse.json(createProblemDetails({ title: 'Schedule', detail: error.message }), { status: 400 });
			}
			throw new Error('An error occurred while publishing the document');
		}
	}),

	http.put(umbracoPath(`${UMB_SLUG}/:id/publish-with-descendants`), async ({ request, params }) => {
		const id = params.id as string;
		if (!id) return new HttpResponse(null, { status: 400 });
		const requestBody = (await request.json()) as PublishDocumentWithDescendantsRequestModel;
		if (!requestBody) return new HttpResponse(null, { status: 400, statusText: 'no body found' });

		try {
			const taskId = umbDocumentMockDb.publishing.publishWithDescendants(id, requestBody);
			return HttpResponse.json<PublishWithDescendantsResultModel>({ taskId, isComplete: false });
		} catch (error) {
			if (error instanceof Error) {
				return HttpResponse.json(createProblemDetails({ title: 'Schedule', detail: error.message }), { status: 400 });
			}
			throw new Error('An error occurred while publishing the document');
		}
	}),

	http.get(umbracoPath(`${UMB_SLUG}/:id/publish-with-descendants/result/:taskId`), async ({ params }) => {
		const id = params.id as string;
		if (!id) return new HttpResponse(null, { status: 400 });
		const taskId = params.taskId as string;
		if (!taskId) return new HttpResponse(null, { status: 400 });

		try {
			const isComplete = umbDocumentMockDb.publishing.taskResult(taskId);
			return HttpResponse.json<GetDocumentByIdPublishWithDescendantsResultByTaskIdResponse>({ taskId, isComplete });
		} catch (error) {
			return HttpResponse.json(
				createProblemDetails({ title: 'Task', detail: error instanceof Error ? error.message : 'An error occurred' }),
				{ status: 400 },
			);
		}
	}),

	http.put(umbracoPath(`${UMB_SLUG}/:id/unpublish`), async ({ request, params }) => {
		const id = params.id as string;
		if (!id) return new HttpResponse(null, { status: 400 });
		const requestBody = (await request.json()) as UnpublishDocumentRequestModel;
		if (!requestBody) return new HttpResponse(null, { status: 400, statusText: 'no body found' });
		umbDocumentMockDb.publishing.unpublish(id, requestBody);
		return new HttpResponse(null, { status: 200 });
	}),

	http.get(umbracoPath(`${UMB_SLUG}/:id/published`), async ({ params }) => {
		const id = params.id as string;
		if (!id) return new HttpResponse(null, { status: 400 });

		const document = umbDocumentMockDb.detail.read(id);

		if (!document) return new HttpResponse(null, { status: 404 });

		const responseModel: GetDocumentByIdPublishedResponse = {
			documentType: document.documentType,
			id: document.id,
			isTrashed: document.isTrashed,
			values: document.values,
			variants: document.variants,
			template: document.template,
			flags: document.flags,
		};

		return HttpResponse.json<GetDocumentByIdPublishedResponse>(responseModel);
	}),
];
