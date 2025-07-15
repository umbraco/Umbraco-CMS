const { rest } = window.MockServiceWorker;
import { umbracoPath } from '@umbraco-cms/backoffice/utils';
import type { GetTemporaryFileConfigurationResponse } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbId } from '@umbraco-cms/backoffice/id';

const UMB_SLUG = 'temporary-file';

export const handlers = [
	rest.post(umbracoPath(`/${UMB_SLUG}`), async (_req, res, ctx) => {
		const guid = UmbId.new();
		return res(ctx.delay(), ctx.status(201), ctx.set('Umb-Generated-Resource', guid));
	}),

	rest.get(umbracoPath(`/${UMB_SLUG}/configuration`), async (_req, res, ctx) => {
		return res(
			ctx.delay(),
			ctx.json<GetTemporaryFileConfigurationResponse>({
				allowedUploadedFileExtensions: [],
				disallowedUploadedFilesExtensions: ['exe', 'dll', 'bat', 'msi'],
				maxFileSize: 1468007,
				imageFileTypes: ['jpg', 'png', 'gif', 'jpeg', 'svg'],
			}),
		);
	}),
];
