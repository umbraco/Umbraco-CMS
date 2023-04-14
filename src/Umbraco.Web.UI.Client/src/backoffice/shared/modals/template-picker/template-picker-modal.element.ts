import { css, html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, state } from 'lit/decorators.js';
import { UmbTreeElement } from '../../components/tree/tree.element';
import { UmbTemplatePickerModalData, UmbTemplatePickerModalResult } from '@umbraco-cms/backoffice/modal';
import { UmbModalBaseElement } from '@umbraco-cms/internal/modal';

//TODO: make a default tree-picker that can be used across multiple pickers
// TODO: make use of UmbPickerLayoutBase
@customElement('umb-template-picker-modal')
export class UmbTemplatePickerModalElement extends UmbModalBaseElement<
	UmbTemplatePickerModalData,
	UmbTemplatePickerModalResult
> {
	@state()
	_selection: Array<string> = [];

	@state()
	_multiple = true;

	connectedCallback() {
		super.connectedCallback();
		this._selection = this.data?.selection ?? [];
		this._multiple = this.data?.multiple ?? true;
	}

	private _handleSelectionChange(e: CustomEvent) {
		e.stopPropagation();
		const element = e.target as UmbTreeElement;
		this._selection = this._multiple ? element.selection : [element.selection[element.selection.length - 1]];
	}

	private _submit() {
		this.modalHandler?.submit({ selection: this._selection });
	}

	private _close() {
		this.modalHandler?.reject();
	}

	// TODO: implement search
	// TODO: make umb-tree have a disabled option (string array like selection)?
	render() {
		return html`
			<umb-workspace-layout headline="Select Content">
				<uui-box>
					<uui-input></uui-input>
					<hr />
					<umb-tree
						alias="Umb.Tree.Templates"
						@selected=${this._handleSelectionChange}
						.selection=${this._selection}
						selectable></umb-tree>
				</uui-box>
				<div slot="actions">
					<uui-button label="Close" @click=${this._close}></uui-button>
					<uui-button label="Submit" look="primary" color="positive" @click=${this._submit}></uui-button>
				</div>
			</umb-workspace-layout>
		`;
	}

	static styles = [
		UUITextStyles,
		css`
			h3 {
				margin-left: var(--uui-size-space-5);
				margin-right: var(--uui-size-space-5);
			}

			uui-input {
				width: 100%;
			}

			hr {
				border: none;
				border-bottom: 1px solid var(--uui-color-divider);
				margin: 16px 0;
			}

			#content-list {
				display: flex;
				flex-direction: column;
				gap: var(--uui-size-space-3);
			}

			.content-item {
				cursor: pointer;
			}

			.content-item.selected {
				background-color: var(--uui-color-selected);
				color: var(--uui-color-selected-contrast);
			}
		`,
	];
}

export default UmbTemplatePickerModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-template-picker-modal': UmbTemplatePickerModalElement;
	}
}
