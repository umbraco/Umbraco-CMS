import type { UUIColorSwatchesEvent } from '@umbraco-ui/uui-color-swatches';

import { css, html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property, state } from 'lit/decorators.js';
import { UmbModalLayoutElement } from '../modal-layout.element';

import icons from '../../../../../public-assets/icons/icons.json';

import '@umbraco-ui/uui-color-swatch';
import '@umbraco-ui/uui-color-swatches';

export interface UmbModalIconPickerData {
	multiple: boolean;
	selection: string[];
}

// TODO: Make use of UmbPickerLayoutBase
@customElement('umb-modal-layout-icon-picker')
export class UmbModalLayoutIconPickerElement extends UmbModalLayoutElement<UmbModalIconPickerData> {
	static styles = [
		UUITextStyles,
		css`
			:host {
				position: relative;
			}

			#container {
				display: flex;
				flex-direction: column;
				height: 100%;
				background-color: white;
				box-shadow: var(--uui-shadow-depth-1, 0 1px 3px rgba(0, 0, 0, 0.12), 0 1px 2px rgba(0, 0, 0, 0.24));
				border-radius: var(--uui-border-radius);
				padding: var(--uui-size-space-5);
				box-sizing: border-box;
			}
			#container hr {
				height: 1px;
				border: none;
				background-color: var(--uui-color-divider);
				margin: 20px 0;
			}

			#searchbar {
				width: 100%;
			}
			#searchbar_icon {
				padding-left: 6px;
			}

			#icon-selection {
				line-height: 0;
				display: grid;
				grid-template-columns: repeat(auto-fit, minmax(40px, 40px));
				overflow-y: scroll;
				max-height: 100%;
				min-height: 0;
			}

			#icon-selection .icon {
				display: inline-block;
				border-radius: 2px;
				width: 100%;
				height: 100%;
				padding: 8px;
				box-sizing: border-box;
			}

			#icon-selection .icon-container {
				display: inline-block;
			}

			#icon-selection .icon:focus,
			#icon-selection .icon:hover,
			#icon-selection .icon.selected {
				background-color: rgba(0, 0, 0, 0.1);
			}

			uui-button {
				margin-left: var(--uui-size-space-4);
			}

			uui-color-swatches {
				margin: -0.75rem;
			}
		`,
	];

	@property({ type: Array })
	iconlist = icons.map((icon) => icon.name);

	@property({ type: Array })
	iconlistFiltered: Array<string>;

	@property({ type: Array })
	colorlist = [
		'#000000',
		'#373737',
		'#9e9e9e',
		'#607d8b',
		'#2196f3',
		'#03a9f4',
		'#3f51b5',
		'#9c27b0',
		'#673ab7',
		'#00bcd4',
		'#4caf50',
		'#8bc34a',
		'#cddc39',
		'#ffeb3b',
		'#ffc107',
		'#ff9800',
		'#ff5722',
		'#f44336',
		'#e91e63',
		'#795548',
	];

	@state()
	private _currentColor: string;

	@state()
	private _currentIcon: string;

	private _changeIcon(e: { target: HTMLInputElement; type: any; key: unknown }) {
		if (e.type == 'click' || (e.type == 'keyup' && e.key == 'Enter')) {
			this._currentIcon = e.target.id;
		}
	}

	private _filterIcons(e: { target: HTMLInputElement }) {
		if (e.target.value) {
			this.iconlistFiltered = this.iconlist.filter((icon) => icon.includes(e.target.value));
		} else {
			this.iconlistFiltered = this.iconlist;
		}
	}

	private _setColor(color: string) {
		return 'color: ' + color;
	}

	private _close() {
		this.modalHandler?.close();
	}

	private _save() {
		this.modalHandler?.close({ color: this._currentColor, icon: this._currentIcon });
	}

	constructor() {
		super();
		this._currentColor = '';
		this._currentIcon = '';
		this.iconlistFiltered = [];
	}

	connectedCallback(): void {
		super.connectedCallback();
		this._currentColor = this.colorlist[0];
		this._currentIcon = this.iconlist[0];
		this.iconlistFiltered = this.iconlist;
	}

	render() {
		return html`
			<umb-workspace-entity headline="Select Icon">
				<div id="container">
					${this.renderSearchbar()}
					<hr />
					<uui-color-swatches
						.swatches="${this.colorlist}"
						@change="${(e: UUIColorSwatchesEvent) => (this._currentColor = e.target.value)}"></uui-color-swatches>

					<hr />
					<uui-scroll-container id="icon-selection">${this.renderIconSelection()}</uui-scroll-container>
				</div>
				<uui-button slot="actions" look="secondary" label="close" @click="${this._close}">Close</uui-button>
				<uui-button slot="actions" color="positive" look="primary" @click="${this._save}" label="save">
					Save
				</uui-button>
			</umb-workspace-entity>
		`;
	}

	renderSearchbar() {
		return html` <uui-input
			@keyup="${this._filterIcons}"
			placeholder="Type to filter..."
			label="Type to filter icons"
			id="searchbar">
			<uui-icon name="search" slot="prepend" id="searchbar_icon"></uui-icon>
		</uui-input>`;
	}

	renderIconSelection() {
		return html`${this.iconlistFiltered.map((icon) => {
			return html`
				<uui-icon
					tabindex="0"
					.style="${this._setColor(this._currentColor)}"
					class="icon ${icon === this._currentIcon ? 'selected' : ''}"
					name="${icon}"
					label="${icon}"
					id="${icon}"
					@click="${this._changeIcon}"
					@keyup="${this._changeIcon}"></uui-icon>
			`;
		})}`;
	}
}

export default UmbModalLayoutIconPickerElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-modal-layout-icon-picker': UmbModalLayoutIconPickerElement;
	}
}
