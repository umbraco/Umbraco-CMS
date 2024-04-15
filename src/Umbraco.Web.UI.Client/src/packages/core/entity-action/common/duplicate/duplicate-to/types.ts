import type { UmbDuplicateRequestArgs } from '../duplicate/types.js';

export interface UmbDuplicateToRequestArgs extends UmbDuplicateRequestArgs {
	destination: {
		unique: string | null;
	};
}
