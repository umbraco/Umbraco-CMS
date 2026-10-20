const { http, HttpResponse } = window.MockServiceWorker;
import { umbDocumentBlueprintMockDb } from '../../db/document-blueprint.db.js';
import { UMB_SLUG } from './slug.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';
import type { FolderItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';

export const itemHandlers = [
	// Folders come first, so that the more specific path is matched before the blueprint one.
	http.get(umbracoPath(`/item${UMB_SLUG}/folder`), ({ request }) => {
		const ids = new URL(request.url).searchParams.getAll('id');
		if (!ids) return;

		const items: Array<FolderItemResponseModel> = umbDocumentBlueprintMockDb
			.getAll()
			.filter((blueprint) => blueprint.isFolder && ids.includes(blueprint.id))
			.map((folder) => ({
				id: folder.id,
				name: folder.name,
				flags: folder.flags,
			}));

		return HttpResponse.json(items);
	}),

	http.get(umbracoPath(`/item${UMB_SLUG}`), ({ request }) => {
		const ids = new URL(request.url).searchParams.getAll('id');
		if (!ids) return;
		const items = umbDocumentBlueprintMockDb.item.getItems(ids);
		return HttpResponse.json(items);
	}),
];
