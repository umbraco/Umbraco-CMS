const { http, HttpResponse } = window.MockServiceWorker;
import { umbElementMockDb } from '../../db/element.db.js';
import { UMB_SLUG } from './slug.js';
import type { PagedReferencedElementWithPendingChangesResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { PublishableVariantStateModel } from '@umbraco-cms/backoffice/external/backend-api';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

// Direct elements referenced by the element at the given id that are not fully published (the element→element
// case). Keyed by the referencing element's id — see mocks/data/sets/blocks-reusable-content/element.data.ts.
const REFERENCED_ELEMENTS_BY_ENTITY_ID: Record<
	string,
	Array<{ id: string; state: PublishableVariantStateModel; isScheduled: boolean }>
> = {
	'library-element-two-id': [
		{ id: 'library-element-three-id', state: PublishableVariantStateModel.DRAFT, isScheduled: true },
	],
};

export const referencePendingChangesHandlers = [
	http.get(umbracoPath(`${UMB_SLUG}/:id/referenced-elements-with-pending-changes`), ({ request, params }) => {
		const id = params.id as string;
		if (!id) return new HttpResponse(null, { status: 400 });
		if (id === 'forbidden') return new HttpResponse(null, { status: 403 });

		const url = new URL(request.url);
		const skip = url.searchParams.get('skip') ? parseInt(url.searchParams.get('skip') as string, 10) : 0;
		const take = url.searchParams.get('take') ? parseInt(url.searchParams.get('take') as string, 10) : 100;

		const entries = REFERENCED_ELEMENTS_BY_ENTITY_ID[id] ?? [];
		const page = entries.slice(skip, skip + take);
		const items = page.map((entry) => ({
			element: umbElementMockDb.item.getItems([entry.id])[0],
			state: entry.state,
			isScheduled: entry.isScheduled,
		}));

		const response: PagedReferencedElementWithPendingChangesResponseModel = {
			total: entries.length,
			items,
		};

		return HttpResponse.json(response);
	}),
];
