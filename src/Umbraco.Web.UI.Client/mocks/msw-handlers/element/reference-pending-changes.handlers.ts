const { http, HttpResponse } = window.MockServiceWorker;
import { umbElementMockDb } from '../../db/element.db.js';
import { umbMockManager } from '../../mock-manager.js';
import { UMB_SLUG } from './slug.js';
import type { ElementItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbPagedModel } from '@umbraco-cms/backoffice/repository';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const referencePendingChangesHandlers = [
	http.get(umbracoPath(`${UMB_SLUG}/:id/referenced-elements-with-pending-changes`), ({ request, params }) => {
		const id = params.id as string;
		if (!id) return new HttpResponse(null, { status: 400 });
		if (id === 'forbidden') return new HttpResponse(null, { status: 403 });

		try {
			umbElementMockDb.detail.read(id);
		} catch {
			return new HttpResponse(null, { status: 404 });
		}

		const url = new URL(request.url);
		const skip = url.searchParams.get('skip') ? parseInt(url.searchParams.get('skip') as string, 10) : 0;
		const take = url.searchParams.get('take') ? parseInt(url.searchParams.get('take') as string, 10) : 100;

		const referencedIds = umbMockManager.getDataSet().referencedElementsWithPendingChanges?.[id] ?? [];

		// getItems() only returns ids it actually finds, so a referenced id that isn't (or is no longer) present
		// in the active data set is silently dropped rather than serialized as a broken entry.
		const items = umbElementMockDb.item.getItems(referencedIds);

		const response: UmbPagedModel<ElementItemResponseModel> = {
			total: items.length,
			items: items.slice(skip, skip + take),
		};

		return HttpResponse.json(response);
	}),
];
