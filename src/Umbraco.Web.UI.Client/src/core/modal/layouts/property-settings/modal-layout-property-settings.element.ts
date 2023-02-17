import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html, nothing } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { UmbModalLayoutElement } from '../modal-layout.element';

@customElement('umb-modal-layout-property-settings')
export class UmbModalLayoutPropertySettingsElement extends UmbModalLayoutElement {
	static styles = [
		UUITextStyles,
		css`
			#content {
				padding: var(--uui-size-space-6);
			}
			#appearances {
				display: flex;
				gap: var(--uui-size-space-6);
			}
			.appearance {
				position: relative;
				display: flex;
				border: 2px solid var(--uui-color-border-standalone);
				padding: 0 16px;
				align-items: center;
				border-radius: 6px;
				opacity: 0.8;
			}
			.appearance:not(.selected):hover {
				border-color: var(--uui-color-border-emphasis);
				cursor: pointer;
				opacity: 1;
			}
			.appearance.selected {
				border-color: var(--uui-color-selected);
				opacity: 1;
			}
			.appearance.selected::after {
				content: '';
				position: absolute;
				inset: 0;
				border-radius: 6px;
				opacity: 0.1;
				background-color: var(--uui-color-selected);
			}
			.appearance.left {
				flex-grow: 1;
			}
			.appearance.top {
				flex-shrink: 1;
			}
			.appearance svg {
				display: flex;
				width: 100%;
				color: var(--uui-color-text);
			}
			hr {
				border: none;
				border-top: 1px solid var(--uui-color-divider);
				margin-top: var(--uui-size-space-6);
				margin-bottom: var(--uui-size-space-5);
			}
			uui-input {
				width: 100%;
			}
			#alias-lock {
				display: flex;
				align-items: center;
				justify-content: center;
				cursor: pointer;
			}
			#alias-lock uui-icon {
				margin-bottom: 2px;
			}
			.container {
				display: flex;
				flex-direction: column;
			}
		`,
	];

	@state()
	private _appearanceIsLeft = true;

	@state()
	private _aliasLocked = true;

	#close() {
		this.modalHandler?.close();
	}

	#submit() {
		this.modalHandler?.close();
	}

	#onAppearanceChange(event: MouseEvent) {
		const target = event.target as HTMLElement;
		const alreadySelected = target.classList.contains(this._appearanceIsLeft ? 'left' : 'top');

		if (alreadySelected) return;

		this._appearanceIsLeft = !this._appearanceIsLeft;

		console.log('appearance changed to: ', this._appearanceIsLeft ? 'left' : 'top');
	}

	#renderLeftSVG() {
		return html`<div
			@click=${this.#onAppearanceChange}
			@keydown=${() => ''}
			class="appearance left ${this._appearanceIsLeft ? 'selected' : ''}">
			<svg width="260" height="60" viewBox="0 0 260 60" fill="none" xmlns="http://www.w3.org/2000/svg">
				<rect width="89" height="14" rx="7" fill="currentColor" />
				<rect x="121" width="139" height="10" rx="5" fill="currentColor" fill-opacity="0.4" />
				<rect x="121" y="46" width="108" height="10" rx="5" fill="currentColor" fill-opacity="0.4" />
				<rect x="121" y="23" width="139" height="10" rx="5" fill="currentColor" fill-opacity="0.4" />
			</svg>
		</div>`;
	}

	#renderTopSVG() {
		return html`
			<div
				@click=${this.#onAppearanceChange}
				@keydown=${() => ''}
				class="appearance top ${this._appearanceIsLeft ? '' : 'selected'}">
				<svg width="139" height="90" viewBox="0 0 139 90" fill="none" xmlns="http://www.w3.org/2000/svg">
					<rect width="89" height="14" rx="7" fill="currentColor" />
					<rect y="30" width="139" height="10" rx="5" fill="currentColor" fill-opacity="0.4" />
					<rect y="76" width="108" height="10" rx="5" fill="currentColor" fill-opacity="0.4" />
					<rect y="53" width="139" height="10" rx="5" fill="currentColor" fill-opacity="0.4" />
				</svg>
			</div>
		`;
	}

	#toggleAliasLock() {
		this._aliasLocked = !this._aliasLocked;
	}

	render() {
		return html` <umb-workspace-layout headline="Property settings">
			<div id="content">
				<uui-box>
					<div class="container">
						<uui-input id="name-input" placeholder="Enter a name..."> </uui-input>
						<uui-input id="alias-input" placeholder="Enter alias..." ?disabled=${this._aliasLocked}>
							<div @click=${this.#toggleAliasLock} @keydown=${() => ''} id="alias-lock" slot="prepend">
								<uui-icon name=${this._aliasLocked ? 'umb:lock' : 'umb:unlocked'}></uui-icon>
							</div>
						</uui-input>
						<uui-textarea id="description-input" placeholder="Enter description..."></uui-textarea>
					</div>
					<uui-button label="Select editor" look="outline"></uui-button>
					<hr />
					<div class="container">
						<b>Validation</b>
						<div style="display: flex; justify-content: space-between">
							<label for="mandatory">Field is mandatory</label>
							<uui-toggle id="mandatory" slot="editor"></uui-toggle>
						</div>
						<p style="margin-bottom: 0">Custom validation</p>
						<uui-select></uui-select>
					</div>
					<hr />
					<div class="container">
						<b style="margin-bottom: var(--uui-size-space-3)">Appearance</b>
						<div id="appearances">${this.#renderLeftSVG()} ${this.#renderTopSVG()}</div>
					</div>
				</uui-box>
			</div>
			<div slot="actions">
				<uui-button label="Close" @click=${this.#close}></uui-button>
				<uui-button label="Submit" look="primary" color="positive" @click=${this.#submit}></uui-button>
			</div>
		</umb-workspace-layout>`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-modal-layout-property-settings': UmbModalLayoutPropertySettingsElement;
	}
}
