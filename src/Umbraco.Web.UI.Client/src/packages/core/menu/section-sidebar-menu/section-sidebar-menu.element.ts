import type { ManifestMenu } from '../menu.extension.js';
import { isMenuItemExpansionEntry } from '../components/menu-item/expansion/is-menu-item-expansion-entry.guard.js';
import type { ManifestSectionSidebarAppBaseMenu, ManifestSectionSidebarAppMenuKind } from './types.js';
import { UMB_SECTION_SIDEBAR_MENU_SECTION_CONTEXT } from './section-context/section-sidebar-menu.section-context.token.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, customElement, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbExpansionEntryCollapsedEvent, UmbExpansionEntryExpandedEvent } from '@umbraco-cms/backoffice/utils';
import { UmbExtensionSlotElement } from '@umbraco-cms/backoffice/extension-registry';

@customElement('umb-section-sidebar-menu')
export class UmbSectionSidebarMenuElement<
	ManifestType extends ManifestSectionSidebarAppBaseMenu = ManifestSectionSidebarAppMenuKind,
> extends UmbLitElement {
	@property({ type: Object, attribute: false })
	manifest?: ManifestType;

	renderHeader() {
		/*
		Notice we are not using the UUI-H3 here, we would need to wrap it into uui-text for this to take action,
		but it does look odd so lets return to this later. I have made a few corrections especially for this component. [NL]
		*/
		return html`<h3>${this.localize.string(this.manifest?.meta?.label ?? '')}</h3>`;
	}

	#sectionSidebarMenuContext?: typeof UMB_SECTION_SIDEBAR_MENU_SECTION_CONTEXT.TYPE;

	#extensionSlotElement = new UmbExtensionSlotElement();
	#muteStateUpdate = false;

	constructor() {
		super();
		this.#initExtensionSlotElement();
		this.consumeContext(UMB_SECTION_SIDEBAR_MENU_SECTION_CONTEXT, (context) => {
			this.#sectionSidebarMenuContext = context;
			this.#observeExpansion();
		});
	}

	#initExtensionSlotElement() {
		/* For better performance and UX we prevent lit from doing unnecessary rerenders we programmatically create the element,
		and manually update the props when needed. */

		this.#extensionSlotElement.type = 'menu';
		this.#extensionSlotElement.filter = (menu: ManifestMenu) => menu.alias === this.manifest?.meta?.menu;
		this.#extensionSlotElement.defaultElement = 'umb-menu';
		this.#extensionSlotElement.events = {
			[UmbExpansionEntryExpandedEvent.TYPE]: this.#onExpansionChange.bind(this),
			[UmbExpansionEntryCollapsedEvent.TYPE]: this.#onExpansionChange.bind(this),
		};
	}

	#observeExpansion() {
		this.observe(this.#sectionSidebarMenuContext?.expansion.expansion, (items) => {
			if (this.#muteStateUpdate) return;

			this.#extensionSlotElement.props = {
				expansion: items || [],
			};
		});
	}

	#onExpansionChange(e: Event) {
		const event = e as UmbExpansionEntryExpandedEvent | UmbExpansionEntryCollapsedEvent;
		event.stopPropagation();
		const eventEntry = event.entry;

		if (!eventEntry) {
			throw new Error('Entity is required to toggle expansion.');
		}

		// Only react to the event if it is a valid Menu Item Expansion Entry
		if (isMenuItemExpansionEntry(eventEntry) === false) return;

		if (event.type === UmbExpansionEntryExpandedEvent.TYPE) {
			this.#muteStateUpdate = true;
			this.#sectionSidebarMenuContext?.expansion.expandItem(eventEntry);
			this.#muteStateUpdate = false;
		} else if (event.type === UmbExpansionEntryCollapsedEvent.TYPE) {
			this.#muteStateUpdate = true;
			this.#sectionSidebarMenuContext?.expansion.collapseItem(eventEntry);
			this.#muteStateUpdate = false;
		}
	}

	override render() {
		return html` ${this.renderHeader()} ${this.#extensionSlotElement}`;
	}

	override disconnectedCallback(): void {
		super.disconnectedCallback();
		this.#extensionSlotElement.destroy();
	}

	static override styles = [
		UmbTextStyles,
		css`
			h3 {
				display: flex;
				align-items: center;
				height: var(--umb-header-layout-height);
				margin: 0;
				padding: var(--uui-size-4) var(--uui-size-8);
				box-sizing: border-box;
				font-size: 14px;
			}
		`,
	];
}

export default UmbSectionSidebarMenuElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-section-sidebar-menu': UmbSectionSidebarMenuElement;
	}
}
