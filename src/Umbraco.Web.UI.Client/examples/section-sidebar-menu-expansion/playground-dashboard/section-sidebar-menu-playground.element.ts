import { html, customElement, LitElement, css, state, repeat } from '@umbraco-cms/backoffice/external/lit';
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';
import {
	UMB_SECTION_SIDEBAR_MENU_GLOBAL_CONTEXT,
	UMB_SECTION_SIDEBAR_MENU_SECTION_CONTEXT,
	type UmbSectionMenuItemExpansionEntryModel,
} from '@umbraco-cms/backoffice/menu';

@customElement('example-section-sidebar-menu-playground-dashboard')
export class ExampleSectionSidebarMenuPlaygroundDashboard extends UmbElementMixin(LitElement) {
	#globalContext?: typeof UMB_SECTION_SIDEBAR_MENU_GLOBAL_CONTEXT.TYPE;
	#sectionContext?: typeof UMB_SECTION_SIDEBAR_MENU_SECTION_CONTEXT.TYPE;

	@state()
	private _globalExpansion: Array<UmbSectionMenuItemExpansionEntryModel> = [];

	@state()
	private _sectionExpansion: Array<UmbSectionMenuItemExpansionEntryModel> = [];

	constructor() {
		super();

		this.consumeContext(UMB_SECTION_SIDEBAR_MENU_GLOBAL_CONTEXT, (section) => {
			this.#globalContext = section;
			this.#observeGlobalExpansion();
		});

		this.consumeContext(UMB_SECTION_SIDEBAR_MENU_SECTION_CONTEXT, (section) => {
			this.#sectionContext = section;
			this.#observeSectionExpansion();
		});
	}

	#observeGlobalExpansion() {
		this.observe(this.#globalContext?.expansion.expansion, (items) => {
			this._globalExpansion = items || [];
		});
	}

	#observeSectionExpansion() {
		this.observe(this.#sectionContext?.expansion.expansion, (items) => {
			this._sectionExpansion = items || [];
		});
	}

	#onCloseItem(event: PointerEvent, item: UmbSectionMenuItemExpansionEntryModel) {
		event.stopPropagation();
		this.#sectionContext?.expansion.collapseItem(item);
	}

	#onCollapseSection() {
		this.#sectionContext?.expansion.collapseAll();
	}

	override render() {
		return html` <umb-stack>
			<uui-box headline="Open Items for this section">
				<uui-button slot="header-actions" @click=${this.#onCollapseSection} compact>
					<uui-icon name="icon-wand"></uui-icon>
					Collapse All</uui-button
				>
				${repeat(
					this._sectionExpansion,
					(item) => item.entityType + item.unique,
					(item) => this.#renderItem(item),
				)}
			</uui-box>
			<uui-box headline="Open Items for all sections">
				${repeat(
					this._globalExpansion,
					(item) => item.entityType + item.unique,
					(item) => this.#renderItem(item),
				)}
			</uui-box>
		</umb-stack>`;
	}

	#renderItem(item: UmbSectionMenuItemExpansionEntryModel) {
		return html` <uui-ref-node
			name=${item.entityType}
			detail=${item.unique + ', ' + item.menuItemAlias + ', ' + item.sectionAlias}>
			<uui-button slot="actions" @click=${(event: PointerEvent) => this.#onCloseItem(event, item)}>Close</uui-button>
		</uui-ref-node>`;
	}

	static override styles = [
		css`
			:host {
				display: block;
				padding: var(--uui-size-layout-1);
			}
		`,
	];
}

export { ExampleSectionSidebarMenuPlaygroundDashboard as element };

declare global {
	interface HTMLElementTagNameMap {
		'example-section-sidebar-menu-playground-dashboard': ExampleSectionSidebarMenuPlaygroundDashboard;
	}
}
