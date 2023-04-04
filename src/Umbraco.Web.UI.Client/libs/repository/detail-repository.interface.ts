import type { ProblemDetailsModel } from '@umbraco-cms/backoffice/backend-api';

export interface UmbDetailRepository<DetailType> {
	createScaffold(parentId: string | null): Promise<{
		data?: DetailType;
		error?: ProblemDetailsModel;
	}>;

	requestById(id: string): Promise<{
		data?: DetailType;
		error?: ProblemDetailsModel;
	}>;

	create(data: DetailType): Promise<{
		error?: ProblemDetailsModel;
	}>;

	save(data: DetailType): Promise<{
		error?: ProblemDetailsModel;
	}>;

	delete(key: string): Promise<{
		error?: ProblemDetailsModel;
	}>;
}
