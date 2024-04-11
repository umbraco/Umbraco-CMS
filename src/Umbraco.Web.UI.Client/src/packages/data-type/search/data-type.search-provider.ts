import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export class UmbDataTypeSearchProvider implements UmbApi {
	constructor() {
		console.log('UmbDataTypeSearchProvider hello world');
	}

	search(): void {
		alert('search');
	}

	destroy(): void {
		throw new Error('Method not implemented.');
	}
}

export { UmbDataTypeSearchProvider as api };
