import type { UmbDuplicateRequestArgs } from '@umbraco-cms/backoffice/entity-action';

export * from './duplicate-to-data-source.interface.js';
export * from './duplicate-to-repository.interface.js';

export interface UmbDuplicateToRequestArgs extends UmbDuplicateRequestArgs {
	destination: {
		unique: string | null;
	};
}
