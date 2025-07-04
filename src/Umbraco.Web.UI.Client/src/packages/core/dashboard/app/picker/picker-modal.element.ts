import type { UmbDashboardAppDetailModel } from '../types.js';
import type { UmbDashboardAppPickerModalValue, UmbDashboardAppPickerModalData } from './picker-modal.token.js';
import { UmbDashboardAppPickerContext } from './picker.context.js';
import { html, customElement, state, repeat, nothing, type PropertyValues } from '@umbraco-cms/backoffice/external/lit';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';

@customElement('umb-dashboard-app-picker-modal')
export class UmbDashboardAppPickerModalElement extends UmbModalBaseElement<
	UmbDashboardAppPickerModalData,
	UmbDashboardAppPickerModalValue
> {
	@state()
	private _items: Array<UmbDashboardAppDetailModel> = [];

	@state()
	private _searchQuery: string | undefined;

	#pickerContext = new UmbDashboardAppPickerContext(this);

	constructor() {
		super();
		this.#observeSelection();
	}

	override connectedCallback(): void {
		super.connectedCallback();
		this.#pickerContext.selection.setSelectable(true);
		this.#pickerContext.selection.setMultiple(this.data?.multiple ?? false);
		this.#pickerContext.selection.setSelection(this.value?.selection ?? []);

		this.observe(this.#pickerContext.items, (options) => {
			this._items = options;
		});
	}

	protected override firstUpdated(_changedProperties: PropertyValues): void {
		super.firstUpdated(_changedProperties);
		this.#pickerContext.loadData();
	}

	#observeSelection() {
		this.observe(
			this.#pickerContext.selection.selection,
			(selection) => {
				this.updateValue({ selection });
			},
			'uopObserveSelection',
		);
	}
	override render() {
		return html`<umb-body-layout headline="Select item(s)">
			<uui-box>${this.#renderMenu()}</uui-box>
			${this.#renderActions()}
		</umb-body-layout> `;
	}

	#renderMenu() {
		if (this._searchQuery) {
			return nothing;
		}

		return html`
			${repeat(
				this._items,
				(item) => item.unique,
				(item) => html`
					<uui-menu-item
						label=${this.localize.string(item.name ?? '')}
						selectable
						@selected=${() => this.#pickerContext.selection.select(item.unique)}
						@deselected=${() => this.#pickerContext.selection.deselect(item.unique)}
						?selected=${this.value.selection.includes(item.unique)}>
					</uui-menu-item>
				`,
			)}
		`;
	}

	#renderActions() {
		return html`
			<div slot="actions">
				<uui-button label="Close" @click=${this._rejectModal}></uui-button>
				<uui-button label="Submit" look="primary" color="positive" @click=${this._submitModal}></uui-button>
			</div>
		`;
	}
}

export { UmbDashboardAppPickerModalElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'uop-data-configuration-type-picker-modal': UmbDashboardAppPickerModalElement;
	}
}
