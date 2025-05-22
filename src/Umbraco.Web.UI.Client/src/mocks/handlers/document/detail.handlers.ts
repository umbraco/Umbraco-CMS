const { rest } = window.MockServiceWorker;
import { umbDocumentMockDb } from '../../data/document/document.db.js';
import { items as referenceData } from '../../data/tracked-reference.data.js';
import { UMB_SLUG } from './slug.js';
import type {
	CreateDocumentRequestModel,
	DefaultReferenceResponseModel,
	GetDocumentByIdReferencedDescendantsResponse,
	PagedIReferenceResponseModel,
	UpdateDocumentRequestModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const detailHandlers = [
	rest.post(umbracoPath(`${UMB_SLUG}`), async (req, res, ctx) => {
		const requestBody = (await req.json()) as CreateDocumentRequestModel;
		if (!requestBody) return res(ctx.status(400, 'no body found'));

		const id = umbDocumentMockDb.detail.create(requestBody);

		return res(
			ctx.status(201),
			ctx.set({
				Location: req.url.href + '/' + id,
				'Umb-Generated-Resource': id,
			}),
		);
	}),

	rest.get(umbracoPath(`${UMB_SLUG}/configuration`), (_req, res, ctx) => {
		return res(ctx.status(200), ctx.json(umbDocumentMockDb.getConfiguration()));
	}),

	rest.get(umbracoPath(`${UMB_SLUG}/:id/referenced-by`), (_req, res, ctx) => {
		const id = _req.params.id as string;
		if (!id) return;

		const query = _req.url.searchParams;
		const skip = query.get('skip') ? parseInt(query.get('skip') as string, 10) : 0;
		const take = query.get('take') ? parseInt(query.get('take') as string, 10) : 100;

		let data: Array<DefaultReferenceResponseModel> = [];

		if (id === 'all-property-editors-document-id') {
			data = referenceData;
		}

		const PagedTrackedReference: PagedIReferenceResponseModel = {
			total: data.length,
			items: data.slice(skip, skip + take),
		};

		return res(ctx.status(200), ctx.json(PagedTrackedReference));
	}),

	rest.get(umbracoPath(`${UMB_SLUG}/:id/referenced-descendants`), (_req, res, ctx) => {
		const id = _req.params.id as string;
		if (!id) return;

		const ReferencedDescendantsResponse: GetDocumentByIdReferencedDescendantsResponse = {
			total: 0,
			items: [],
		};

		return res(ctx.status(200), ctx.json(ReferencedDescendantsResponse));
	}),

	rest.put(umbracoPath(`${UMB_SLUG}/:id/validate`, 'v1.1'), (_req, res, ctx) => {
		const id = _req.params.id as string;
		if (!id) return res(ctx.status(400));

		return res(ctx.status(200));
	}),

	rest.get(umbracoPath(`${UMB_SLUG}/:id`), (req, res, ctx) => {
		const id = req.params.id as string;
		if (!id) return res(ctx.status(400));
		const response = umbDocumentMockDb.detail.read(id);
		return res(ctx.status(200), ctx.json(response));
	}),

	rest.put(umbracoPath(`${UMB_SLUG}/:id`), async (req, res, ctx) => {
		const id = req.params.id as string;
		if (!id) return res(ctx.status(400));
		const requestBody = (await req.json()) as UpdateDocumentRequestModel;
		if (!requestBody) return res(ctx.status(400, 'no body found'));
		umbDocumentMockDb.detail.update(id, requestBody);
		return res(ctx.status(200));
	}),

	rest.delete(umbracoPath(`${UMB_SLUG}/:id`), (req, res, ctx) => {
		const id = req.params.id as string;
		if (!id) return res(ctx.status(400));
		umbDocumentMockDb.detail.delete(id);
		return res(ctx.status(200));
	}),
];
