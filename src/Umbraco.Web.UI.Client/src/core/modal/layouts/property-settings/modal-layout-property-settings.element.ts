import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html } from 'lit';
import { customElement } from 'lit/decorators.js';
import { UmbModalLayoutElement } from '../modal-layout.element';

@customElement('umb-modal-layout-property-settings')
export class UmbModalLayoutPropertySettingsElement extends UmbModalLayoutElement {
	static styles = [
		UUITextStyles,
		css`
			#content {
				padding: var(--uui-size-space-6);
			}
		`,
	];

	private _close() {
		this.modalHandler?.close();
	}

	private _submit() {
		this.modalHandler?.close();
	}

	render() {
		return html` <umb-workspace-layout headline="Select Property Editor UI">
			<div id="content">
				<uui-box> content here </uui-box>
			</div>
			<div slot="actions">
				<uui-button label="Close" @click=${this._close}></uui-button>
				<uui-button label="Submit" look="primary" color="positive" @click=${this._submit}></uui-button>
			</div>
		</umb-workspace-layout>`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-modal-layout-property-settings': UmbModalLayoutPropertySettingsElement;
	}
}
