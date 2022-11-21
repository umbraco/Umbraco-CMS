import { html, css } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, state } from 'lit/decorators.js';
import { UmbModalLayoutElement } from '@umbraco-cms/services';

export interface UmbModalFieldsSettingsData {
	name: string;
	exposed: boolean;
}

@customElement('umb-modal-layout-fields-settings')
export class UmbModalLayoutFieldsSettingsElement extends UmbModalLayoutElement<UmbModalFieldsSettingsData> {
	static styles = [
		UUITextStyles,
		css`
			:host {
				display: relative;
			}

			uui-dialog-layout {
				display: flex;
				flex-direction: column;
				height: 100%;
				background-color: white;
				box-shadow: var(--uui-shadow-depth-1, 0 1px 3px rgba(0, 0, 0, 0.12), 0 1px 2px rgba(0, 0, 0, 0.24));
				border-radius: var(--uui-border-radius);
				padding: var(--uui-size-space-5);
				box-sizing: border-box;
			}

			uui-scroll-container {
				overflow-y: scroll;
				max-height: 100%;
				min-height: 0;
				flex: 1;
			}

			div {
				margin-top: var(--uui-size-space-5);
				display: flex;
				flex-direction: row-reverse;
			}
		`,
	];

	@state()
	private _fields?: UmbModalFieldsSettingsData[];

	private _handleClose() {
		this.modalHandler?.close({ fields: this._fields });
	}

	disconnectedCallback(): void {
		super.disconnectedCallback();
		this._handleClose();
	}

	firstUpdated() {
		this.data
			? (this._fields = Object.values(this.data).map((field) => {
					return { name: field.name, exposed: field.exposed };
			  }))
			: '';
	}

	render() {
		if (this._fields) {
			return html`
				<uui-dialog-layout headline="Show fields">
					<uui-scroll-container id="field-settings">
						<span>
							${Object.values(this._fields).map((field, index) => {
								return html`<uui-toggle
										name="${field.name}"
										label="${field.name}"
										.checked="${field.exposed}"
										@change="${() => {
											this._fields ? (this._fields[index].exposed = !field.exposed) : '';
										}}"></uui-toggle>
									<br />`;
							})}
						</span>
					</uui-scroll-container>
					<div>
						<uui-button look="primary" label="Close sidebar" @click="${this._handleClose}">Close</uui-button>
					</div>
				</uui-dialog-layout>
			`;
		} else return html``;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-modal-layout-fields-settings': UmbModalLayoutFieldsSettingsElement;
	}
}
