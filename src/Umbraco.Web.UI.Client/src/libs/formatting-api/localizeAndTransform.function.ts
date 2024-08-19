import { UmbFormattingController } from './formatting.controller.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 * @param host
 * @param input
 * @deprecated - Use the `<umb-ufm-render>` component instead. This method will be removed in Umbraco 15.
 */
export function localizeAndTransform(host: UmbControllerHost, input: string): string {
	return new UmbFormattingController(host).transform(input);
}
