import { UmbContextToken } from '@umbraco-cms/context-api';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';
import { BasicState } from 'libs/observable-api/basic-state';

export class UmbSectionSidebarContext {
	#host: UmbControllerHostInterface;
	#contextMenuIsOpen = new BasicState<boolean>(false);
	contextMenuIsOpen = this.#contextMenuIsOpen.asObservable();

	#entityType?: string;
	#unique?: string;

	getUnique() {
		return this.#unique;
	}

	getEntityType() {
		return this.#entityType;
	}

	constructor(host: UmbControllerHostInterface) {
		this.#host = host;
	}

	openContextMenu(entityType: string, unique: string) {
		this.#entityType = entityType;
		this.#unique = unique;
		this.#contextMenuIsOpen.next(true);
	}

	closeContextMenu() {
		this.#contextMenuIsOpen.next(false);
		this.#entityType = undefined;
		this.#unique = undefined;
	}
}

export const UMB_SECTION_SIDEBAR_CONTEXT_TOKEN = new UmbContextToken<UmbSectionSidebarContext>(
	UmbSectionSidebarContext.name
);
