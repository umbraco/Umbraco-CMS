import { UMB_SHORTCUT_CONTEXT } from './shortcut.context-token.js';
import { UmbShortcutController } from './shortcut.controller.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbShortcutContext extends UmbShortcutController {
	constructor(host: UmbControllerHost) {
		super(host);
		this.provideContext(UMB_SHORTCUT_CONTEXT, this as unknown as UmbShortcutContext);
	}
}
