const { http, HttpResponse } = window.MockServiceWorker;

// TODO: add schema
export const handlers = [
	http.get('/umbraco/management/api/v1/images/getprocessedimageurl', () => {
		return HttpResponse.json('/url/to/processed/image');
	}),
];
