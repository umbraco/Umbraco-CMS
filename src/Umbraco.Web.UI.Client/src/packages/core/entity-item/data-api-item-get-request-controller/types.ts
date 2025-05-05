import type { UmbDataApiResponse } from '@umbraco-cms/backoffice/resources';

export interface UmbDataApiItemGetRequestControllerArgs<ResponseModelType extends UmbDataApiResponse> {
	api: (args: { uniques: Array<string> }) => Promise<ResponseModelType>;
	uniques: Array<string>;
}
