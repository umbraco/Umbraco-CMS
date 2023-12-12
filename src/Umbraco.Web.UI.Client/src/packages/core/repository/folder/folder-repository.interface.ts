import { UmbCreateFolderModel, UmbUpdateFolderModel } from './types.js';
import type { ProblemDetails } from '@umbraco-cms/backoffice/backend-api';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

// TODO add response types folder folders
export interface UmbFolderRepository extends UmbApi {
	createScaffold(parentUnique: string | null): Promise<{
		data?: any;
		error?: ProblemDetails;
	}>;

	create(args: UmbCreateFolderModel): Promise<{
		data?: string;
		error?: ProblemDetails;
	}>;

	request(unique: string): Promise<{
		data?: any;
		error?: ProblemDetails;
	}>;

	update(args: UmbUpdateFolderModel): Promise<{
		data?: any;
		error?: ProblemDetails;
	}>;

	delete(unique: string): Promise<{
		error?: ProblemDetails;
	}>;
}
