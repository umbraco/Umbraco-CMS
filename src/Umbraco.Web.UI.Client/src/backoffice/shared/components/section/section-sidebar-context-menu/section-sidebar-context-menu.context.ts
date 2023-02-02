import { UmbContextToken } from '@umbraco-cms/context-api';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';
import { BasicState } from 'libs/observable-api/basic-state';

export class UmbSectionSidebarContextMenuController {
	#host: UmbControllerHostInterface;
	#isOpen = new BasicState<boolean>(false);
	isOpen = this.#isOpen.asObservable();

	constructor(host: UmbControllerHostInterface) {
		this.#host = host;
	}

	open() {
		this.#isOpen.next(true);
	}

	close() {
		this.#isOpen.next(false);
	}
}

// TODO: that was a long name
export const UMB_SECTION_SIDEBAR_CONTEXT_MENU_CONTROLLER_CONTEXT_TOKEN =
	new UmbContextToken<UmbSectionSidebarContextMenuController>(UmbSectionSidebarContextMenuController.name);
