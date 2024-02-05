import type {
	UmbCompositionPickerModalData,
	UmbCompositionPickerModalValue,
} from './composition-picker-modal.token.js';
import { css, html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import type { UmbTreeElement } from '@umbraco-cms/backoffice/tree';

@customElement('umb-composition-picker-modal')
export class UmbCompositionPickerModalElement extends UmbModalBaseElement<
	UmbCompositionPickerModalData,
	UmbCompositionPickerModalValue
> {
	//

	@state()
	private _selectionConfiguration = {
		multiple: true,
		selectable: true,
		selection: [],
	};

	constructor() {
		super();
	}

	firstUpdated() {
		this._selectionConfiguration = { ...this._selectionConfiguration, selection: (this.data?.selection as []) ?? [] };
	}

	#onSelectionChange(e: CustomEvent) {
		const values = (e.target as UmbTreeElement).getSelection() ?? [];
		this.value = { selection: values as string[] };
		this._selectionConfiguration = { ...this._selectionConfiguration, selection: values as [] };
	}

	render() {
		return html`
			<umb-body-layout headline="${this.localize.term('contentTypeEditor_compositions')}">
				<umb-localize key="contentTypeEditor_compositionsDescription">
					Inherit tabs and properties from an existing Document Type. New tabs will be<br />added to the current
					Document Type or merged if a tab with an identical name exists.<br />
				</umb-localize>
				<uui-input id="search" placeholder=${this.localize.term('placeholders_filter')}>
					<uui-icon name="icon-search" slot="prepend"></uui-icon>
				</uui-input>
				<umb-tree
					hide-tree-root
					alias="Umb.Tree.DocumentType"
					@selection-change=${this.#onSelectionChange}
					.selectionConfiguration=${this._selectionConfiguration}
					.selectableFilter=${(item: any) => item.unique !== this.data?.unique}></umb-tree>
				<div slot="actions">
					<uui-button label=${this.localize.term('general_close')} @click=${this._rejectModal}></uui-button>
					<uui-button
						label=${this.localize.term('general_submit')}
						look="primary"
						color="positive"
						@click=${this._submitModal}></uui-button>
				</div>
			</umb-body-layout>
		`;
	}

	static styles = [
		css`
			uui-input {
				margin: var(--uui-size-6) 0;
				display: flex;
				align-items: center;
			}
			uui-icon {
				padding-left: var(--uui-size-3);
			}
		`,
	];
}

export default UmbCompositionPickerModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-composition-picker-modal': UmbCompositionPickerModalElement;
	}
}
