import { css, html, LitElement } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, state, query } from 'lit/decorators.js';

import '@umbraco-ui/uui-modal';
import '@umbraco-ui/uui-modal-container';
import '@umbraco-ui/uui-modal-dialog';
import '@umbraco-ui/uui-modal-sidebar';
import { UUIModalSidebarElement } from '@umbraco-ui/uui-modal-sidebar';

@customElement('umb-property-editor-content-picker')
class UmbPropertyEditorContentPicker extends LitElement {
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

	@state()
	open = false;

	@query('uui-modal-sidebar')
	sidebar?: UUIModalSidebarElement;

	private _renderModal() {
		return this.open
			? html`<uui-modal-sidebar data-modal-size="small" @closed=${() => (this.open = false)}>
					<umb-editor-layout>
						<h3 slot="name">Select content</h3>
						<uui-box>
							<uui-input></uui-input>
							<hr />
							Lorem ipsum dolor sit amet consectetur adipisicing elit. Ab minima et praesentium rem, nesciunt,
							blanditiis culpa esse tempore perspiciatis recusandae magni voluptas tempora officiis commodi nihil
							deserunt quidem aliquid sed?
						</uui-box>
						<div slot="actions">
							<uui-button look="secondary" label="close" @click=${(e: Event) => this.sidebar?.close(e)}
								>Close</uui-button
							>
						</div>
					</umb-editor-layout>
			  </uui-modal-sidebar>`
			: '';
	}

	render() {
		return html`
			<uui-button look="primary" @click=${() => (this.open = !this.open)} label="open">Open</uui-button>
			<uui-modal-container>${this._renderModal()}</uui-modal-container>
		`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-content-picker': UmbPropertyEditorContentPicker;
	}
}
