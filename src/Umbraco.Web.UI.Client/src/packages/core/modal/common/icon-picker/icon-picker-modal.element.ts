import icons from '../../../../../shared/icon-registry/icons/icons.json' assert { type: 'json' };
import type { UUIColorSwatchesEvent } from '@umbraco-cms/backoffice/external/uui';

import { css, html, styleMap, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';

import { UmbIconPickerModalData, UmbIconPickerModalValue } from '@umbraco-cms/backoffice/modal';
import { UmbModalBaseElement } from '@umbraco-cms/internal/modal';

// TODO: Make use of UmbPickerLayoutBase
// TODO: to prevent element extension we need to move the Picker logic into a separate class we can reuse across all pickers
@customElement('umb-icon-picker-modal')
export class UmbIconPickerModalElement extends UmbModalBaseElement<UmbIconPickerModalData, UmbIconPickerModalValue> {
	private _iconList = icons.map((icon) => icon.name);

	@state()
	private _iconListFiltered: Array<string> = [];

	@state()
	private _colorList = [
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
	private _currentColor?: string;

	@state()
	private _currentIcon?: string;

	private _changeIcon(e: { target: HTMLInputElement; type: any; key: unknown }) {
		if (e.type == 'click' || (e.type == 'keyup' && e.key == 'Enter')) {
			this._currentIcon = e.target.id;
		}
	}

	private _filterIcons(e: { target: HTMLInputElement }) {
		if (e.target.value) {
			this._iconListFiltered = this._iconList.filter((icon) => icon.includes(e.target.value));
		} else {
			this._iconListFiltered = this._iconList;
		}
	}

	private _close() {
		this.modalContext?.reject();
	}

	private _submit() {
		this.modalContext?.submit({ color: this._currentColor, icon: this._currentIcon });
	}

	private _onColorChange(e: UUIColorSwatchesEvent) {
		this._currentColor = e.target.value;
	}

	connectedCallback() {
		super.connectedCallback();
		this._currentColor = this.data?.color ?? this._colorList[0];
		this._currentIcon = this.data?.icon ?? this._iconList[0];
		this._iconListFiltered = this._iconList;
	}

	render() {
		return html`
			<umb-body-layout headline="Select Icon">
				<div id="container">
					${this.renderSearchbar()}
					<hr />
					<uui-color-swatches
						.value="${this._currentColor || ''}"
						label="Color switcher for icons"
						@change="${this._onColorChange}">
						${this._colorList.map(
							(color) => html`
								<uui-color-swatch label="${color}" title="${color}" value="${color}"></uui-color-swatch>
							`,
						)}
					</uui-color-swatches>

					<hr />
					<uui-scroll-container id="icon-selection">${this.renderIconSelection()}</uui-scroll-container>
				</div>
				<uui-button slot="actions" label="close" @click="${this._close}">Close</uui-button>
				<uui-button slot="actions" color="positive" look="primary" @click="${this._submit}" label="Submit">
					Submit
				</uui-button>
			</umb-body-layout>
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
		return html`${this._iconListFiltered.map((icon) => {
			return html`
				<uui-icon
					tabindex="0"
					style=${styleMap({ color: this._currentColor })}
					class="icon ${icon === this._currentIcon ? 'selected' : ''}"
					title="${icon}"
					name="${icon}"
					label="${icon}"
					id="${icon}"
					@click="${this._changeIcon}"
					@keyup="${this._changeIcon}"></uui-icon>
			`;
		})}`;
	}

	static styles = [
		UmbTextStyles,
		css`
			:host {
				position: relative;
			}

			#container {
				display: flex;
				flex-direction: column;
				height: 100%;
				background-color: var(--uui-color-surface);
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
				padding-left: var(--uui-size-space-2);
			}

			#icon-selection {
				line-height: 0;
				display: grid;
				grid-template-columns: repeat(auto-fit, minmax(40px, calc(100% / 8)));
				overflow-y: scroll;
				max-height: 100%;
				min-height: 0;
				padding: 2px;
			}

			#icon-selection .icon {
				display: inline-block;
				border-radius: var(--uui-border-radius);
				width: 100%;
				height: 100%;
				padding: var(--uui-size-space-3);
				box-sizing: border-box;
			}

			#icon-selection .icon-container {
				display: inline-block;
			}

			#icon-selection .icon:focus,
			#icon-selection .icon:hover,
			#icon-selection .icon.selected {
				outline: 2px solid var(--uui-color-selected);
			}

			uui-button {
				margin-left: var(--uui-size-space-4);
			}

			uui-color-swatches {
				margin: -0.75rem;
			}
		`,
	];
}

export default UmbIconPickerModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-icon-picker-modal': UmbIconPickerModalElement;
	}
}
