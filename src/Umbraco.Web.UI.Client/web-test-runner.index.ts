import { startMockServiceWorker } from './mocks/index.js';
import { umbHttpClient } from '@umbraco-cms/backoffice/http-client';

await startMockServiceWorker({
	quiet: true,
});

// Mirror the Umb-Generated-Resource interceptor from UmbApiInterceptorController.
// In the full app this is wired up by auth.context.ts, but unit tests skip auth entirely,
// so we register it here once for all tests.
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
