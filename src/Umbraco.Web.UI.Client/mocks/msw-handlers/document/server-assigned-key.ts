const { http, HttpResponse } = window.MockServiceWorker;
import { useMockHandlers } from '../../index.js';
import { umbDocumentMockDb } from '../../db/document.db.js';
import { UMB_SLUG } from './slug.js';
import type {
	CreateAndPublishDocumentRequestModel,
	CreateDocumentRequestModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

/**
 * Makes the server store a newly created document under `key` instead of the key the client sent, as a
 * Saving notification handler assigning its own key would. Reset with `resetMockHandlers()`.
 * @param {string} key - The key the server assigns.
 */
export function useServerAssignedDocumentKey(key: string) {
	const createdResponse = (request: Request, id: string) =>
		HttpResponse.json(null, {
			status: 201,
			headers: { Location: request.url + '/' + id, 'Umb-Generated-Resource': id },
		});

	useMockHandlers(
		http.post(umbracoPath(`${UMB_SLUG}`), async ({ request }) => {
			const body = (await request.json()) as CreateDocumentRequestModel;
			return createdResponse(request, umbDocumentMockDb.detail.create({ ...body, id: key }));
		}),
		http.post(umbracoPath(`${UMB_SLUG}/create-and-publish`), async ({ request }) => {
			const body = (await request.json()) as CreateAndPublishDocumentRequestModel;
			return createdResponse(request, umbDocumentMockDb.publishing.createAndPublish({ ...body, id: key }));
		}),
	);
}
