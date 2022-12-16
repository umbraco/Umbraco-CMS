import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { UmbModalLayoutPickerBase } from '../modal-layout-picker-base';
import { UmbObserverMixin } from '@umbraco-cms/observable-api';
import { UmbContextConsumerMixin } from '@umbraco-cms/context-api';
import { umbExtensionsRegistry } from '@umbraco-cms/extensions-registry';
import type { ManifestSection } from '@umbraco-cms/models';

@customElement('umb-picker-layout-section')
export class UmbPickerLayoutSectionElement extends UmbContextConsumerMixin(UmbObserverMixin(UmbModalLayoutPickerBase)) {
	static styles = [
		UUITextStyles,
		css`
			uui-input {
				width: 100%;
			}
			hr {
				border: none;
				border-bottom: 1px solid var(--uui-color-divider);
				margin: 16px 0;
			}
			#item-list {
				display: flex;
				flex-direction: column;
				gap: var(--uui-size-1);
			}
			.item {
				color: var(--uui-color-interactive);
				display: grid;
				grid-template-columns: var(--uui-size-8) 1fr;
				padding: var(--uui-size-4) var(--uui-size-2);
				gap: var(--uui-size-space-5);
				align-items: center;
				border-radius: var(--uui-size-2);
				cursor: pointer;
			}
			.item.selected {
				background-color: var(--uui-color-selected);
				color: var(--uui-color-selected-contrast);
			}
			.item:not(.selected):hover {
				background-color: var(--uui-color-surface-emphasis);
				color: var(--uui-color-interactive-emphasis);
			}
			.item.selected:hover {
				background-color: var(--uui-color-selected-emphasis);
			}
			.item uui-icon {
				width: 100%;
				box-sizing: border-box;
				display: flex;
				height: fit-content;
			}
		`,
	];

	@state()
	private _sections: Array<ManifestSection> = [];

	connectedCallback(): void {
		super.connectedCallback();
		umbExtensionsRegistry.extensionsOfType('section').subscribe((sections: Array<ManifestSection>) => {
			this._sections = sections;
		});
	}

	render() {
		return html`
			<umb-workspace-entity-layout headline="Select sections">
				<uui-box>
					<uui-input label="search"></uui-input>
					<hr />
					<div id="item-list">
						${this._sections.map(
							(item) => html`
								<div
									@click=${() => this._handleItemClick(item.alias)}
									@keydown=${(e: KeyboardEvent) => this._handleKeydown(e, item.alias)}
									class=${this._isSelected(item.alias) ? 'item selected' : 'item'}>
									<span>${item.meta.label}</span>
								</div>
							`
						)}
					</div>
				</uui-box>
				<div slot="actions">
					<uui-button label="Close" @click=${this._close}></uui-button>
					<uui-button label="Submit" look="primary" color="positive" @click=${this._submit}></uui-button>
				</div>
			</umb-workspace-entity-layout>
		`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-picker-layout-section': UmbPickerLayoutSectionElement;
	}
}
