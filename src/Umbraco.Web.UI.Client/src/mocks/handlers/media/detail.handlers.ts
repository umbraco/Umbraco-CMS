const { rest } = window.MockServiceWorker;
import { umbMediaMockDb } from '../../data/media/media.db.js';
import { items as referenceData } from '../../data/tracked-reference.data.js';
import { UMB_SLUG } from './slug.js';
import type {
	CreateMediaRequestModel,
	PagedIReferenceResponseModel,
	UpdateMediaRequestModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';
import type { UmbMediaDetailModel } from '@umbraco-cms/backoffice/media';

export const detailHandlers = [
	rest.post(umbracoPath(`${UMB_SLUG}`), async (req, res, ctx) => {
		const requestBody = (await req.json()) as CreateMediaRequestModel;
		if (!requestBody) return res(ctx.status(400, 'no body found'));

		const id = umbMediaMockDb.detail.create(requestBody);

		return res(
			ctx.status(201),
			ctx.set({
				Location: req.url.href + '/' + id,
				'Umb-Generated-Resource': id,
			}),
		);
	}),

	rest.get(umbracoPath(`${UMB_SLUG}/:id/referenced-by`), (_req, res, ctx) => {
		const id = _req.params.id as string;
		if (!id) return;

		const PagedTrackedReference = {
			total: referenceData.length,
			items: referenceData,
		};

		return res(ctx.status(200), ctx.json<PagedIReferenceResponseModel>(PagedTrackedReference));
	}),

	rest.get(umbracoPath(`${UMB_SLUG}/:id`), (req, res, ctx) => {
		const id = req.params.id as string;
		if (!id) return res(ctx.status(400));
		const response = umbMediaMockDb.detail.read(id);
		return res(ctx.status(200), ctx.json(response));
	}),

	rest.put<UmbMediaDetailModel>(umbracoPath(`${UMB_SLUG}/:id/validate`), async (req, res, ctx) => {
		const id = req.params.id as string;
		if (!id) return res(ctx.status(400));
		const model = await req.json<UmbMediaDetailModel>();
		if (!model) return res(ctx.status(400));

		const hasMediaPickerOrFileUploadValue = model.values.some((v) => {
			return v.editorAlias === 'Umbraco.UploadField' && v.value;
		});

		if (!hasMediaPickerOrFileUploadValue) {
			return res(ctx.status(400, 'No media picker or file upload value found'));
		}

		return res(ctx.status(200));
	}),

	rest.put(umbracoPath(`${UMB_SLUG}/:id`), async (req, res, ctx) => {
		const id = req.params.id as string;
		if (!id) return res(ctx.status(400));
		const requestBody = (await req.json()) as UpdateMediaRequestModel;
		if (!requestBody) return res(ctx.status(400, 'no body found'));
		umbMediaMockDb.detail.update(id, requestBody);
		return res(ctx.status(200));
	}),

	rest.delete(umbracoPath(`${UMB_SLUG}/:id`), (req, res, ctx) => {
		const id = req.params.id as string;
		if (!id) return res(ctx.status(400));
		umbMediaMockDb.detail.delete(id);
		return res(ctx.status(200));
	}),
];
