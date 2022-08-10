import { css, html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, state } from 'lit/decorators.js';
import { UmbModalLayoutElement } from '../modal-layout.element';

export interface UmbModalContentPickerData {
	multiple: boolean;
}

@customElement('umb-modal-layout-content-picker')
export class UmbModalLayoutContentPickerElement extends UmbModalLayoutElement<UmbModalContentPickerData> {
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

			#content-list {
				display: flex;
				flex-direction: column;
				gap: 8px;
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

	private _tempContent = [
		{
			id: 1,
			name: 'Content 1',
			description: 'Content 1 description',
			icon: 'icon-umb-content',
		},
		{
			id: 2,
			name: 'Content 2',
			description: 'Content 2 description',
			icon: 'icon-umb-content',
		},
		{
			id: 3,
			name: 'Content 3',
			description: 'Content 3 description',
			icon: 'icon-umb-content',
		},
	];

	@state()
	_selectedContent: any[] = [];

	private _clickContent(content: any) {
		if (this._selectedContent.includes(content)) {
			this._selectedContent = this._selectedContent.filter((c) => c !== content);
		} else {
			this._selectedContent.push(content);
		}

		this.requestUpdate('_selectedContent');
	}

	private _submit() {
		this.modalHandler?.close({ selection: this._selectedContent });
	}

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
					<div id="content-list">
						${this._tempContent.map(
							(content) =>
								// eslint-disable-next-line lit-a11y/click-events-have-key-events
								html`<div
									class=${`content-item ${this._selectedContent.includes(content) ? 'selected' : ''}`}
									@click=${() => this._clickContent(content)}>
									${content.name}
								</div>`
						)}
					</div>
				</uui-box>
				<div slot="actions">
					<uui-button label="Close" @click=${this._close}></uui-button>
					<uui-button label="Submit" look="primary" color="positive" @click=${this._submit}></uui-button>
				</div>
			</umb-editor-layout>
		`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-modal-layout-content-picker': UmbModalLayoutContentPickerElement;
	}
}
