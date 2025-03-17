import { UMB_NOTIFICATION_CONTEXT } from '../../notification.context.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbPeekErrorArgs } from '../../types.js';

import './peek-error-notification.element.js';

export class UmbPeekErrorController extends UmbControllerBase {
	async open(args: UmbPeekErrorArgs): Promise<void> {
		const context = await this.getContext(UMB_NOTIFICATION_CONTEXT);
		if (!context) {
			throw new Error('Could not get notification context');
		}

		context.peek('danger', {
			elementName: 'umb-peek-error-notification',
			data: args,
		});

		// This is a one time off, so we can destroy our selfs.
		this.destroy();

		return;
	}
}

/**
 *
 * @param host {UmbControllerHost} - The host controller
 * @param args {UmbPeekErrorArgs} - The data to pass to the notification
 * @returns {UmbPeekErrorController} The notification peek controller instance
 */
export function umbPeekError(host: UmbControllerHost, args: UmbPeekErrorArgs) {
	return new UmbPeekErrorController(host).open(args);
}
