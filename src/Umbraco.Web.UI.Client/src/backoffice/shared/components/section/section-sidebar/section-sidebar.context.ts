import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';
import { StringState, BooleanState } from '@umbraco-cms/backoffice/observable-api';

export class UmbSectionSidebarContext {
	#host: UmbControllerHostElement;
	#contextMenuIsOpen = new BooleanState(false);
	contextMenuIsOpen = this.#contextMenuIsOpen.asObservable();

	#entityType = new StringState<undefined>(undefined);
	entityType = this.#entityType.asObservable();

	#unique = new StringState<undefined>(undefined);
	unique = this.#unique.asObservable();

	#headline = new StringState<undefined>(undefined);
	headline = this.#headline.asObservable();

	constructor(host: UmbControllerHostElement) {
		this.#host = host;
	}

	toggleContextMenu(entityType: string, unique: string, headline: string) {
		this.#unique.getValue() === unique ? this.closeContextMenu() : this.openContextMenu(entityType, unique, headline);
	}

	// TODO: we wont get notified about tree item name changes because we don't have a subscription
	// we need to figure out how we best can handle this when we only know the entity and unique id
	openContextMenu(entityType: string, unique: string, headline: string) {
		this.#entityType.next(entityType);
		this.#unique.next(unique);
		this.#headline.next(headline);
		this.#contextMenuIsOpen.next(true);
	}

	closeContextMenu() {
		this.#contextMenuIsOpen.next(false);
		this.#entityType.next(undefined);
		this.#unique.next(undefined);
		this.#headline.next(undefined);
	}
}

export const UMB_SECTION_SIDEBAR_CONTEXT_TOKEN = new UmbContextToken<UmbSectionSidebarContext>(
	'UmbSectionSidebarContext'
);
