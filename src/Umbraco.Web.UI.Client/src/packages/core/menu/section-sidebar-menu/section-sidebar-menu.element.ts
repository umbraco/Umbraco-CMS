import type { ManifestMenu } from '../menu.extension.js';
import type { ManifestSectionSidebarAppBaseMenu, ManifestSectionSidebarAppMenuKind } from './types.js';
import { UMB_SECTION_SIDEBAR_MENU_CONTEXT } from './context/section-sidebar-menu.context.token.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import {
	UmbExpansionEntityCollapsedEvent,
	UmbExpansionEntityExpandedEvent,
	type UmbEntityExpansionModel,
} from '@umbraco-cms/backoffice/utils';

// TODO: Move to separate file:
const manifest: UmbExtensionManifestKind = {
	type: 'kind',
	alias: 'Umb.Kind.SectionSidebarAppMenu',
	matchKind: 'menu',
	matchType: 'sectionSidebarApp',
	manifest: {
		type: 'sectionSidebarApp',
		elementName: 'umb-section-sidebar-menu',
	},
};
umbExtensionsRegistry.register(manifest);

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

	@state()
	private _sectionSidebarMenuExpansion: UmbEntityExpansionModel = [];

	#localExpansionState: UmbEntityExpansionModel = [];

	constructor() {
		super();
		this.consumeContext(UMB_SECTION_SIDEBAR_MENU_CONTEXT, (context) => {
			this.#sectionSidebarMenuContext = context;
			this.#observeExpansion();
		});
	}

	#observeExpansion() {
		this.observe(this.#sectionSidebarMenuContext?.expansion.expansion, (items) => {
			const allEntriesExists =
				this.#localExpansionState.length > 0 &&
				this.#localExpansionState.every((localItem) =>
					items?.some((item) => item.unique === localItem.unique && item.entityType === localItem.entityType),
				);

			// Ensure that we only updates the expansion (and rerenders) if the state has been changed outside of the component.
			if (allEntriesExists) return;
			this._sectionSidebarMenuExpansion = items || [];
		});
	}

	#onEntityExpansionChange(event: UmbExpansionEntityExpandedEvent | UmbExpansionEntityCollapsedEvent) {
		event.stopPropagation();
		const eventEntity = event.entity;

		if (!eventEntity) {
			throw new Error('Entity is required to toggle expansion.');
		}

		if (event.type === UmbExpansionEntityExpandedEvent.TYPE) {
			this.#localExpansionState.push(eventEntity);
			this.#sectionSidebarMenuContext?.expansion.expandItem(eventEntity);
		} else if (event.type === UmbExpansionEntityCollapsedEvent.TYPE) {
			this.#localExpansionState = this.#localExpansionState.filter(
				(entity) => !(entity.unique === eventEntity.unique && entity.entityType === eventEntity.entityType),
			);
			this.#sectionSidebarMenuContext?.expansion.collapseItem(eventEntity);
		}
	}

	override render() {
		return html`
			${this.renderHeader()}
			<umb-extension-slot
				type="menu"
				.filter="${(menu: ManifestMenu) => menu.alias === this.manifest?.meta?.menu}"
				.props="${{
					expansion: this._sectionSidebarMenuExpansion,
				}}"
				.events="${{
					'expansion-entity-expanded': this.#onEntityExpansionChange.bind(this),
					'expansion-entity-collapsed': this.#onEntityExpansionChange.bind(this),
				}}"
				default-element="umb-menu"></umb-extension-slot>
		`;
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
