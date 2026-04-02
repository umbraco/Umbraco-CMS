const { http, HttpResponse } = window.MockServiceWorker;
import { umbracoPath } from '@umbraco-cms/backoffice/utils';
import type { GetTemporaryFileConfigurationResponse } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbId } from '@umbraco-cms/backoffice/id';

const UMB_SLUG = 'temporary-file';

export const handlers = [
	http.post(umbracoPath(`/${UMB_SLUG}`), async () => {
		const guid = UmbId.new();
		return new HttpResponse(null, { status: 201, headers: { 'Umb-Generated-Resource': guid } });
	}),

	http.get(umbracoPath(`/${UMB_SLUG}/configuration`), async () => {
		return HttpResponse.json<GetTemporaryFileConfigurationResponse>({
			allowedUploadedFileExtensions: [],
			disallowedUploadedFilesExtensions: ['exe', 'dll', 'bat', 'msi'],
			maxFileSize: 1468007,
			imageFileTypes: ['jpg', 'png', 'gif', 'jpeg', 'svg'],
		});
	}),
];
