import { UmbFormattingController } from './formatting.controller.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export function localizeAndTransform(host: UmbControllerHost, input: string): string {
	return new UmbFormattingController(host).transform(input);
}
