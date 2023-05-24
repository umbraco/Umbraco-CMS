import { UUITextStyles } from '@umbraco-cms/backoffice/external/uui';
import { css, html } from '@umbraco-cms/backoffice/external/lit';
import { customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbTreeElement } from '@umbraco-cms/backoffice/tree';
import { UmbModalBaseElement } from '@umbraco-cms/internal/modal';
import { UmbDictionaryItemPickerModalData, UmbDictionaryItemPickerModalResult } from '@umbraco-cms/backoffice/modal';

@customElement('umb-dictionary-item-picker-modal')
export default class UmbDictionaryItemPickerModalElement extends UmbModalBaseElement<
	UmbDictionaryItemPickerModalData,
	UmbDictionaryItemPickerModalResult
> {
	@state()
	_selection: Array<string | null> = [];

	@state()
	_multiple = false;

	connectedCallback() {
		super.connectedCallback();
		this._selection = this.data?.selection ?? [];
		this._multiple = this.data?.multiple ?? true;
	}

	private _handleSelectionChange(e: CustomEvent) {
		e.stopPropagation();
		const element = e.target as UmbTreeElement;
		this._selection = this._multiple ? element.selection : [element.selection[element.selection.length - 1]];
		this._submit();
	}

	private _submit() {
		this.modalHandler?.submit({ selection: this._selection });
	}

	private _close() {
		this.modalHandler?.reject();
	}

	render() {
		return html`
			<umb-body-layout headline="Dictionary item">
				<div id="main">
					<uui-box>
						<umb-tree
							alias="Umb.Tree.Dictionary"
							@selected=${this._handleSelectionChange}
							.selection=${this._selection}
							selectable></umb-tree>
					</uui-box>
				</div>
				<div slot="actions">
					<uui-button @click=${this._close} look="secondary">Close</uui-button>
				</div>
			</umb-body-layout>
		`;
	}

	static styles = [
		UUITextStyles,
		css`
			:host {
				display: block;
				color: var(--uui-color-text);
			}

			#main {
				box-sizing: border-box;
				padding: var(--uui-size-space-5);
				height: calc(100vh - 124px);
			}

			#main uui-button {
				width: 100%;
			}

			h3,
			p {
				text-align: left;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-dictionary-item-picker-modal': UmbDictionaryItemPickerModalElement;
	}
}
