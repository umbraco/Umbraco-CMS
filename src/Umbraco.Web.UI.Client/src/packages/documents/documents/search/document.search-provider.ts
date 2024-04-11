import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export class UmbDocumentSearchProvider implements UmbApi {
	constructor() {
		console.log('UmbDocumentSearchProvider hello world');
	}

	destroy(): void {
		throw new Error('Method not implemented.');
	}
}
