import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html } from '@umbraco-cms/backoffice/external/lit';
import { customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbPartialViewPickerModalData, UmbPartialViewPickerModalResult } from '@umbraco-cms/backoffice/modal';
import { UmbModalBaseElement } from '@umbraco-cms/internal/modal';
import { UmbTreeElement } from '@umbraco-cms/backoffice/tree';

@customElement('umb-partial-view-picker-modal')
export default class UmbPartialViewPickerModalElement extends UmbModalBaseElement<
	UmbPartialViewPickerModalData,
	UmbPartialViewPickerModalResult
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
			<umb-body-layout headline="Insert Partial view">
				<div id="main">
					<uui-box>
						<umb-tree
							alias="Umb.Tree.PartialViews"
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
		'umb-partial-view-picker-modal': UmbPartialViewPickerModalElement;
	}
}
