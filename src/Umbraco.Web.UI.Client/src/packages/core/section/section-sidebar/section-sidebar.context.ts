import { UMB_SECTION_SIDEBAR_CONTEXT } from './section-sidebar.context.token.js';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbSectionSidebarContext extends UmbContextBase {
	constructor(host: UmbControllerHost) {
		super(host, UMB_SECTION_SIDEBAR_CONTEXT);
	}
}
