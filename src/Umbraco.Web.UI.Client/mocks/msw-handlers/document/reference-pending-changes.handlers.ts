const { http, HttpResponse } = window.MockServiceWorker;
import { umbElementMockDb } from '../../db/element.db.js';
import { umbDocumentMockDb } from '../../db/document.db.js';
import { umbMockManager } from '../../mock-manager.js';
import { UMB_SLUG } from './slug.js';
import type {
	PagedReferencedElementWithPendingChangesResponseModel,
	ReferencedElementWithPendingChangesResponseModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const referencePendingChangesHandlers = [
	http.get(umbracoPath(`${UMB_SLUG}/:id/referenced-elements-with-pending-changes`), ({ request, params }) => {
		const id = params.id as string;
		if (!id) return new HttpResponse(null, { status: 400 });
		if (id === 'forbidden') return new HttpResponse(null, { status: 403 });

		try {
			umbDocumentMockDb.detail.read(id);
		} catch {
			return new HttpResponse(null, { status: 404 });
		}

		const url = new URL(request.url);
		const skip = url.searchParams.get('skip') ? parseInt(url.searchParams.get('skip') as string, 10) : 0;
		const take = url.searchParams.get('take') ? parseInt(url.searchParams.get('take') as string, 10) : 100;

		const entries = umbMockManager.getDataSet().referencedElementsWithPendingChanges?.[id] ?? [];

		// The state comes from the referenced element's own mock data, not a duplicate literal here — so a
		// referenced id that isn't (or is no longer) present in the active data set is dropped rather than
		// serialized as a broken `element: undefined`.
		const items = entries
			.map((entry): ReferencedElementWithPendingChangesResponseModel | undefined => {
				const element = umbElementMockDb.item.getItems([entry.id])[0];
				const state = element?.variants[0]?.state;
				if (!element || !state) return undefined;
				return { element, state, isScheduled: entry.isScheduled };
			})
			.filter((item): item is ReferencedElementWithPendingChangesResponseModel => item !== undefined);

		const response: PagedReferencedElementWithPendingChangesResponseModel = {
			total: items.length,
			items: items.slice(skip, skip + take),
		};

		return HttpResponse.json(response);
	}),
];
