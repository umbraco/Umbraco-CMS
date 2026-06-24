const { http, HttpResponse } = window.MockServiceWorker;
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const blockReferenceHandlers = [
	// GET /document/:id/referenced-elements-with-pending-changes
	// Returns library elements referenced by a document that have unpublished draft changes.
	http.get(umbracoPath('/document/:id/referenced-elements-with-pending-changes'), () => {
		return HttpResponse.json({
			total: 2,
			items: [
				{
					id: 'simple-element-id',
					name: 'Simple Element',
					documentType: { id: '4f68ba66-6fb2-4778-83b8-6ab4ca3a7c5c', icon: 'icon-lab' },
					state: 'PublishedPendingChanges',
					publishDate: '2024-02-01T10:00:00.000Z',
					scheduledPublishDate: null,
				},
				{
					id: 'element-in-folder-id',
					name: 'Element In Folder',
					documentType: { id: '4f68ba66-6fb2-4778-83b8-6ab4ca3a7c5c', icon: 'icon-lab' },
					state: 'PublishedPendingChanges',
					publishDate: '2024-01-17T08:00:00.000Z',
					scheduledPublishDate: '2026-05-01T00:00:00.000Z',
				},
			],
		});
	}),
];
