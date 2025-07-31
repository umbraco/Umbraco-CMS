import type { ManifestMenu } from '../menu.extension.js';
import type { ManifestSectionSidebarAppBaseMenu, ManifestSectionSidebarAppMenuKind } from './types.js';
import { UMB_SECTION_SIDEBAR_MENU_CONTEXT } from './section-context/section-sidebar-menu.context.token.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, customElement, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbExpansionEntityCollapsedEvent, UmbExpansionEntityExpandedEvent } from '@umbraco-cms/backoffice/utils';
import { UmbExtensionSlotElement } from '../../extension-registry/components/index.js';

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

	#sectionSidebarMenuContext?: typeof UMB_SECTION_SIDEBAR_MENU_CONTEXT.TYPE;

	#extensionSlotElement = new UmbExtensionSlotElement();
	#muteStateUpdate = false;

	constructor() {
		super();
		this.#initExtensionSlotElement();
		this.consumeContext(UMB_SECTION_SIDEBAR_MENU_CONTEXT, (context) => {
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
			'expansion-entity-expanded': this.#onEntityExpansionChange.bind(this),
			'expansion-entity-collapsed': this.#onEntityExpansionChange.bind(this),
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

	#onEntityExpansionChange(event: UmbExpansionEntityExpandedEvent | UmbExpansionEntityCollapsedEvent) {
		event.stopPropagation();
		const eventEntity = event.entity;

		if (!eventEntity) {
			throw new Error('Entity is required to toggle expansion.');
		}

		if (event.type === UmbExpansionEntityExpandedEvent.TYPE) {
			this.#muteStateUpdate = true;
			this.#sectionSidebarMenuContext?.expansion.expandItem(eventEntity);
			this.#muteStateUpdate = false;
		} else if (event.type === UmbExpansionEntityCollapsedEvent.TYPE) {
			this.#muteStateUpdate = true;
			this.#sectionSidebarMenuContext?.expansion.collapseItem(eventEntity);
			this.#muteStateUpdate = false;
		}
	}

	override render() {
		return html` ${this.renderHeader()} ${this.#extensionSlotElement}`;
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
