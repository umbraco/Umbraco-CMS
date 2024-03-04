import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbStringState, UmbBooleanState } from '@umbraco-cms/backoffice/observable-api';

export class UmbSectionSidebarContext {
	#host: UmbControllerHost;
	#contextMenuIsOpen = new UmbBooleanState(false);
	contextMenuIsOpen = this.#contextMenuIsOpen.asObservable();

	#entityType = new UmbStringState<undefined>(undefined);
	entityType = this.#entityType.asObservable();

	#unique = new UmbStringState<null | undefined>(undefined);
	unique = this.#unique.asObservable();

	#headline = new UmbStringState<undefined>(undefined);
	headline = this.#headline.asObservable();

	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	toggleContextMenu(entityType: string, unique: string | null | undefined, headline: string | undefined) {
		this.openContextMenu(entityType, unique, headline);
	}

	// TODO: we wont get notified about tree item name changes because we don't have a subscription
	// we need to figure out how we best can handle this when we only know the entity and unique id
	openContextMenu(entityType: string, unique: string | null | undefined, headline: string | undefined) {
		this.#entityType.setValue(entityType);
		this.#unique.setValue(unique);
		this.#headline.setValue(headline);
		this.#contextMenuIsOpen.setValue(true);
	}

	closeContextMenu() {
		this.#contextMenuIsOpen.setValue(false);
		this.#entityType.setValue(undefined);
		this.#unique.setValue(undefined);
		this.#headline.setValue(undefined);
	}
}

export const UMB_SECTION_SIDEBAR_CONTEXT = new UmbContextToken<UmbSectionSidebarContext>('UmbSectionSidebarContext');
