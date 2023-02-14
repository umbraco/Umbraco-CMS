import type { ProblemDetailsModel } from '@umbraco-cms/backend-api';

export interface UmbDetailRepository<DetailType> {
	createDetailsScaffold(parentKey: string | null): Promise<{
		data?: DetailType;
		error?: ProblemDetailsModel;
	}>;

	requestByKey(key: string): Promise<{
		data?: DetailType;
		error?: ProblemDetailsModel;
	}>;

	createDetail(data: DetailType): Promise<{
		error?: ProblemDetailsModel;
	}>;

	saveDetail(data: DetailType): Promise<{
		error?: ProblemDetailsModel;
	}>;

	delete(key: string): Promise<{
		error?: ProblemDetailsModel;
	}>;
}
