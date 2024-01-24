import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import {
	UmbPartialViewPickerModalData,
	UmbPartialViewPickerModalValue,
	UmbModalBaseElement,
} from '@umbraco-cms/backoffice/modal';
import { UmbTreeElement, type UmbTreeSelectionConfiguration } from '@umbraco-cms/backoffice/tree';

@customElement('umb-partial-view-picker-modal')
export default class UmbPartialViewPickerModalElement extends UmbModalBaseElement<
	UmbPartialViewPickerModalData,
	UmbPartialViewPickerModalValue
> {
	@state()
	_selectionConfiguration: UmbTreeSelectionConfiguration = {
		multiple: false,
		selectable: true,
		selection: [],
	};

	connectedCallback() {
		super.connectedCallback();
		this._selectionConfiguration.selection = this.value?.selection ?? [];
		this._selectionConfiguration.multiple = this.data?.multiple ?? true;
	}

	private _handleSelectionChange(e: CustomEvent) {
		e.stopPropagation();
		const element = e.target as UmbTreeElement;
		this.value = { selection: element.getSelection() };
		this._submit();
	}

	private _submit() {
		this.modalContext?.submit();
	}

	private _close() {
		this.modalContext?.reject();
	}

	render() {
		return html`
			<umb-body-layout headline="Insert Partial view">
				<div id="main">
					<uui-box>
						<umb-tree
							alias="Umb.Tree.PartialViews"
							@selection-change=${this._handleSelectionChange}
							.selectionConfiguration=${this._selectionConfiguration}></umb-tree>
					</uui-box>
				</div>
				<div slot="actions">
					<uui-button @click=${this._close} look="secondary">Close</uui-button>
				</div>
			</umb-body-layout>
		`;
	}

	static styles = [
		UmbTextStyles,
		css`
			:host {
				display: block;
				color: var(--uui-color-text);
				--umb-header-layout-height: 70px;
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
