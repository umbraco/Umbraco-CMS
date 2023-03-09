import type { ProblemDetailsModel } from '@umbraco-cms/backend-api';

export interface UmbDetailRepository<DetailType> {
	createScaffold(parentKey: string | null): Promise<{
		data?: DetailType;
		error?: ProblemDetailsModel;
	}>;

	requestByKey(key: string): Promise<{
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
