const { http, HttpResponse } = window.MockServiceWorker;
import { umbElementMockDb } from '../../db/element.db.js';
import { UMB_SLUG } from './slug.js';
import type { PagedReferencedElementWithPendingChangesResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { PublishableVariantStateModel } from '@umbraco-cms/backoffice/external/backend-api';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

// Direct elements referenced by the document at the given id that are not fully published.
// Keyed by the referencing document's id — see mocks/data/sets/blocks-reusable-content.
const REFERENCED_ELEMENTS_BY_DOCUMENT_ID: Record<
	string,
	Array<{ id: string; state: PublishableVariantStateModel; isScheduled: boolean }>
> = {
	// Block Grid
	'17cd53f2-93b3-4e34-ade2-916e7a6639ed': [
		{ id: 'library-element-three-id', state: PublishableVariantStateModel.DRAFT, isScheduled: true },
		{ id: 'library-element-four-id', state: PublishableVariantStateModel.PUBLISHED_PENDING_CHANGES, isScheduled: false },
	],
	// Block List
	'39842212-489e-46ec-a63b-6eeff36c7156': [
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

		const entries = REFERENCED_ELEMENTS_BY_DOCUMENT_ID[id] ?? [];
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
