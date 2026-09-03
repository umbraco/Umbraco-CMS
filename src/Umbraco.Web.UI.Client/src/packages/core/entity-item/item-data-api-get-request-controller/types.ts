import type { UmbDataApiResponse } from '@umbraco-cms/backoffice/resources';

export interface UmbItemDataApiGetRequestControllerArgs<ResponseModelType extends UmbDataApiResponse> {
	api: (args: { uniques: Array<string> }) => Promise<ResponseModelType>;
	uniques: Array<string>;
	/**
	 * Suppress the error notification that is otherwise shown when a request fails.
	 * Set this when the caller recovers from the failure itself, so the user is not told about an error that did not
	 * affect them. The error is still returned to the caller either way.
	 * @default false
	 */
	disableNotifications?: boolean;
}
