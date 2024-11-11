import type { UmbOpenContextMenuArgs } from './types.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbStringState, UmbBooleanState } from '@umbraco-cms/backoffice/observable-api';

export class UmbSectionSidebarContext extends UmbContextBase<UmbSectionSidebarContext> {
	#contextMenuIsOpen = new UmbBooleanState(false);
	contextMenuIsOpen = this.#contextMenuIsOpen.asObservable();

	#entityType = new UmbStringState<undefined>(undefined);
	entityType = this.#entityType.asObservable();

	#unique = new UmbStringState<null | undefined>(undefined);
	unique = this.#unique.asObservable();

	#headline = new UmbStringState<undefined>(undefined);
	headline = this.#headline.asObservable();

	#contextElement: Element | undefined = undefined;

	constructor(host: UmbControllerHost) {
		super(host, UMB_SECTION_SIDEBAR_CONTEXT);
	}

	toggleContextMenu(host: Element, args: UmbOpenContextMenuArgs) {
		this.openContextMenu(host, args);
	}

	// TODO: we wont get notified about tree item name changes because we don't have a subscription
	// we need to figure out how we best can handle this when we only know the entity and unique id
	openContextMenu(host: Element, args: UmbOpenContextMenuArgs) {
		this.#entityType.setValue(args.entityType);
		this.#unique.setValue(args.unique);
		this.#headline.setValue(args.headline);
		this.#contextMenuIsOpen.setValue(true);
		this.#contextElement = host;
	}

	closeContextMenu() {
		this.#contextMenuIsOpen.setValue(false);
		this.#entityType.setValue(undefined);
		this.#unique.setValue(undefined);
		this.#headline.setValue(undefined);
		this.#contextElement = undefined;
	}

	getContextElement() {
		return this.#contextElement;
	}
}

export const UMB_SECTION_SIDEBAR_CONTEXT = new UmbContextToken<UmbSectionSidebarContext>('UmbSectionSidebarContext');
