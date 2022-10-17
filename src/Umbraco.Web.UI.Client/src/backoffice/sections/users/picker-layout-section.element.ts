import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { UmbModalLayoutElement } from '../../../core/services/modal/layouts/modal-layout.element';

@customElement('umb-picker-layout-section')
export class UmbPickerLayoutSectionElement extends UmbModalLayoutElement {
	static styles = [
		UUITextStyles,
		css`
			.section {
				color: var(--uui-color-interactive);
				display: flex;
				align-items: center;
				padding: var(--uui-size-2);
				gap: var(--uui-size-space-2);
				cursor: pointer;
			}
			.section:hover {
				background-color: var(--uui-color-surface-emphasis);
				color: var(--uui-color-interactive-emphasis);
			}
			uui-icon[name='check'] {
				color: var(--uui-color-positive);
			}
		`,
	];
	@state()
	private _selection: Array<string> = [];

	private _submit() {
		this.modalHandler?.close({ selection: this._selection });
	}

	private _close() {
		this.modalHandler?.close({ selection: this._selection });
	}

	private tempData = [
		{
			key: '1',
			name: 'Content',
		},
		{
			key: '2',
			name: 'Media',
		},
		{
			key: '3',
			name: 'Settings',
		},
	];

	private _handleKeydown(e: KeyboardEvent, sectionKey: string) {
		if (e.key === 'Enter') {
			this._handleSectionClick(sectionKey);
		}
	}

	private _handleSectionClick(sectionKey: string) {
		if (this._isSectionSelected(sectionKey)) {
			this._selection = this._selection.filter((key) => key !== sectionKey);
		} else {
			this._selection.push(sectionKey);
		}
		this.requestUpdate('_selection');
	}

	private _isSectionSelected(sectionKey: string): boolean {
		return this._selection.includes(sectionKey);
	}

	render() {
		return html`
			<umb-editor-entity-layout headline="Select sections">
				<uui-box>
					${this.tempData.map(
						(section) => html`
							<div
								@click=${() => this._handleSectionClick(section.key)}
								@keydown=${(e: KeyboardEvent) => this._handleKeydown(e, section.key)}
								class="section">
								<uui-icon name="${this._isSectionSelected(section.key) ? 'check' : 'add'}"></uui-icon>
								<span>${section.name}</span>
							</div>
						`
					)}
				</uui-box>
				<div slot="actions">
					<uui-button label="Close" @click=${this._close}></uui-button>
					<uui-button label="Submit" look="primary" color="positive" @click=${this._submit}></uui-button>
				</div>
			</umb-editor-entity-layout>
		`;
	}
}

export default UmbPickerLayoutSectionElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-picker-layout-section': UmbPickerLayoutSectionElement;
	}
}
