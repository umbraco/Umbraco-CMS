import { UmbSectionSidebarMenuContext } from '../../menu/section-sidebar-menu/context/section-sidebar-menu.context.js';
import { UmbSectionContext } from '../section.context.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbDefaultSectionContext extends UmbSectionContext {
	constructor(host: UmbControllerHost) {
		super(host);
		new UmbSectionSidebarMenuContext(this);
	}
}
