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
	_globalExpansion: Array<UmbSectionMenuItemExpansionEntryModel> = [];

	@state()
	_sectionExpansion: Array<UmbSectionMenuItemExpansionEntryModel> = [];

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

	override render() {
		return html` <umb-stack>
			<uui-box headline="Open Items for this section">
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
		return html`<div class="expansion-entry">
			<div>
				<span class="label">Entity Type:</span>
				${item.entityType}
			</div>
			<div>
				<span class="label">Unique:</span>
				${item.unique}
			</div>
			<div>
				<span class="label">Menu Item Alias:</span>
				${item.menuItemAlias}
			</div>
			<div>
				<span class="label">Section Alias:</span>
				${item.sectionAlias}
			</div>
		</div>`;
	}

	static override styles = [
		css`
			:host {
				display: block;
				padding: var(--uui-size-layout-1);
			}

			.expansion-entry {
				border-bottom: 1px solid var(--uui-color-border);
				padding: var(--uui-size-space-2);
				margin-bottom: var(--uui-size-space-2);
			}

			.label {
				font-weight: bold;
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
