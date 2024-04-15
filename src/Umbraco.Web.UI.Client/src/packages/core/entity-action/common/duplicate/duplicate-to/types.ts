import type { UmbDuplicateRequestArgs } from '../duplicate/types.js';

export * from './duplicate-to-data-source.interface.js';
export * from './duplicate-to-repository.interface.js';

export interface UmbDuplicateToRequestArgs extends UmbDuplicateRequestArgs {
	destination: {
		unique: string | null;
	};
}
