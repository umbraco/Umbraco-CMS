const { http, HttpResponse } = window.MockServiceWorker;
import { umbElementMockDb } from '../../data/element/element.db.js';
import { UMB_SLUG } from './slug.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';
import type {
	CreateFolderRequestModel,
	UpdateFolderResponseModel,
	MoveElementRequestModel,
} from '@umbraco-cms/backoffice/external/backend-api';

export const folderHandlers = [
	http.post<object, CreateFolderRequestModel>(umbracoPath(`${UMB_SLUG}/folder`), async ({ request }) => {
		const requestBody = await request.json();
		if (!requestBody) return new HttpResponse(null, { status: 400 });

		const id = umbElementMockDb.folder.create(requestBody);

		return new HttpResponse(null, {
			status: 201,
			headers: {
				Location: request.url + '/' + id,
				'Umb-Generated-Resource': id,
			},
		});
	}),

	http.get(umbracoPath(`${UMB_SLUG}/folder/:id`), ({ params }) => {
		const id = params.id as string;
		if (!id) return new HttpResponse(null, { status: 400 });
		const response = umbElementMockDb.folder.read(id);
		if (!response) return new HttpResponse(null, { status: 404 });
		return HttpResponse.json(response);
	}),

	http.put<{ id: string }, UpdateFolderResponseModel>(
		umbracoPath(`${UMB_SLUG}/folder/:id`),
		async ({ request, params }) => {
			const id = params.id;
			if (!id) return new HttpResponse(null, { status: 400 });
			const requestBody = await request.json();
			if (!requestBody) return new HttpResponse(null, { status: 400 });
			umbElementMockDb.folder.update(id, requestBody);
			return new HttpResponse(null, { status: 200 });
		},
	),

	http.delete(umbracoPath(`${UMB_SLUG}/folder/:id`), ({ params }) => {
		const id = params.id as string;
		if (!id) return new HttpResponse(null, { status: 400 });
		umbElementMockDb.folder.delete(id);
		return new HttpResponse(null, { status: 200 });
	}),

	http.put<{ id: string }, MoveElementRequestModel>(
		umbracoPath(`${UMB_SLUG}/folder/:id/move`),
		async ({ request, params }) => {
			const id = params.id;
			if (!id) return new HttpResponse(null, { status: 400 });
			const requestBody = await request.json();
			if (!requestBody) return new HttpResponse(null, { status: 400 });
			umbElementMockDb.tree.move([id], requestBody.target?.id ?? '');
			return new HttpResponse(null, { status: 200 });
		},
	),

	http.put(umbracoPath(`${UMB_SLUG}/folder/:id/move-to-recycle-bin`), async ({ params }) => {
		const id = params.id as string;
		if (!id) return new HttpResponse(null, { status: 400 });
		umbElementMockDb.recycleBin.trash([id]);
		return new HttpResponse(null, { status: 200 });
	}),
];
