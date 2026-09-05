import { startMockServiceWorker } from './mocks/index.js';
import { umbHttpClient } from '@umbraco-cms/backoffice/http-client';

await startMockServiceWorker({
	quiet: true,
});

// Mirror interceptors from UmbApiInterceptorController.
// In the full app these are wired up by auth.context.ts, but unit tests skip auth entirely,
// so we register them here once for all tests.
// TODO: Investigate whether some of these interceptors belong somewhere else, since they are not auth-specific.
umbHttpClient.interceptors.response.use((response: Response): Response => {
	const generatedResource = response.headers.get('Umb-Generated-Resource');
	if (!generatedResource) return response;
	return new Response(generatedResource, {
		status: response.status,
		statusText: response.statusText,
		headers: { 'Content-Type': 'text/plain' },
	});
});

umbHttpClient.interceptors.response.use(async (response: Response): Promise<Response> => {
	if (response.ok) return response;
	if (response.status === 401 || response.status === 403) return response;

	let problemDetails: Record<string, unknown>;

	try {
		const errorBody = await response.clone().json();
		if (errorBody && typeof errorBody === 'object' && 'status' in errorBody) {
			problemDetails = errorBody;
		} else {
			throw new Error();
		}
	} catch {
		problemDetails = {
			type: 'Error',
			title: response.statusText || 'An error occurred.',
			status: response.status,
		};
	}

	return new Response(JSON.stringify(problemDetails), {
		status: response.status,
		statusText: response.statusText,
		headers: { 'Content-Type': 'application/json' },
	});
});
