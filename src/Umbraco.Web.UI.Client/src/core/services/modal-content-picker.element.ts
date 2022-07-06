import { css, html, LitElement } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property } from 'lit/decorators.js';
import UmbModalHandler from './modalHandler';

@customElement('umb-modal-content-picker')
class UmbModalContentPicker extends LitElement {
	static styles = [
		UUITextStyles,
		css`
			h3 {
				margin-left: 16px;
				margin-right: 16px;
			}

			uui-input {
				width: 100%;
			}

			hr {
				border: none;
				border-bottom: 1px solid var(--uui-color-divider);
				margin: 16px 0;
			}
		`,
	];

	@property({ attribute: false })
	modalHandler?: UmbModalHandler;

	private _close() {
		this.modalHandler?.close();
	}

	render() {
		return html`
			<umb-editor-layout>
				<h3 slot="name">Select content</h3>
				<uui-box>
					<uui-input></uui-input>
					<hr />
					Lorem ipsum dolor sit amet consectetur adipisicing elit. Ab minima et praesentium rem, nesciunt, blanditiis
					culpa esse tempore perspiciatis recusandae magni voluptas tempora officiis commodi nihil deserunt quidem
					aliquid sed?
				</uui-box>
				<div slot="actions">
					<uui-button label="close" look="primary" @click=${this._close}></uui-button>
				</div>
			</umb-editor-layout>
		`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-modal-content-picker': UmbModalContentPicker;
	}
}
