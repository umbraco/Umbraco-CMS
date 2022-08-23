import { css, html, LitElement } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property, state } from 'lit/decorators.js';
import { UmbModalService } from '../../core/services/modal';
import { UmbModalLayoutElement } from '../../core/services/modal/layouts/modal-layout.element';
import { UmbModalContentPickerData } from '../../core/services/modal/layouts/content-picker/modal-layout-content-picker.element';

@customElement('umb-icon-selector')
class UmbIconSelector extends UmbModalLayoutElement<UmbModalContentPickerData> {
	static styles = [
		UUITextStyles,
		css`
			:host {
				position: relative;
			}

			#box {
				overflow-y: auto;
				max-height: 100vh;
			}

			#box .header-icon {
				font-size: 20px;
				height: 1.3em;
			}

			#box hr {
				height: 1px;
				border: none;
				background-color: #ccc;
				margin: 20px 0;
			}

			#searchbar {
				width: 100%;
			}
			#searchbar_icon {
				padding-left: 6px;
			}

			input[type='radio'] {
				display: none;
				position: absolute;
			}

			#palette {
				display: flex;
				flex-wrap: wrap;
			}

			#palette .colorspot {
				box-sizing: border-box;
				line-height: 0;
				padding: 4px;
				margin: 5px 5px 5px 0;
				height: 25px;
				width: 25px;
				display: inline-block;
				border-radius: 5px;
			}

			#palette .checkmark {
				height: 100%;
				width: 100%;
				display: none;
				color: white;
				background-color: rgba(0, 0, 0, 0.2);
				border-radius: 100%;
			}
			#palette input[type='radio']:checked ~ .checkmark {
				display: block;
			}

			#icon-selection {
				line-height: 0;
				display: grid;
				grid-template-columns: repeat(auto-fit, minmax(40px, auto));
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

			/*#icon-selection input[type='radio']:checked ~ .icon {
				background-color: rgba(0, 0, 0, 0.1);
				border: 1px solid #ffeeee;
			}*/

			#icon-selection .icon:focus,
			#icon-selection .icon:hover,
			#icon-selection .icon.selected {
				background-color: rgba(0, 0, 0, 0.1);
			}
		`,
	];

	@property({ type: Array })
	iconlist = [
		'add',
		'alert',
		'attachment',
		'calendar',
		'check',
		'clipboard',
		'code',
		'colorpicker',
		'copy',
		'delete',
		'document',
		'download',
		'edit',
		'favorite',
		'folder',
		'forbidden',
		'info',
		'link',
		'lock',
		'pause',
		'picture',
		'play',
		'remove',
		'search',
		'see',
		'settings',
		'subtract',
		'sync',
		'unlock',
		'unsee',
		'wand',
		'wrong',
		/* Duplicates */
		'add',
		'alert',
		'attachment',
		'calendar',
		'check',
		'clipboard',
		'code',
		'colorpicker',
		'copy',
		'delete',
		'document',
		'download',
		'edit',
		'favorite',
		'folder',
		'forbidden',
		'info',
		'link',
		'lock',
		'pause',
		'picture',
		'play',
		'remove',
		'search',
		'see',
		'settings',
		'subtract',
		'sync',
		'unlock',
		'unsee',
		'wand',
		'wrong',
		'add',
		'alert',
		'attachment',
		'calendar',
		'check',
		'clipboard',
		'code',
		'colorpicker',
		'copy',
		'delete',
		'document',
		'download',
		'edit',
		'favorite',
		'folder',
		'forbidden',
		'info',
		'link',
		'lock',
		'pause',
		'picture',
		'play',
		'remove',
		'search',
		'see',
		'settings',
		'subtract',
		'sync',
		'unlock',
		'unsee',
		'wand',
		'wrong',
		'add',
		'alert',
		'attachment',
		'calendar',
		'check',
		'clipboard',
		'code',
		'colorpicker',
		'copy',
		'delete',
		'document',
		'download',
		'edit',
		'favorite',
		'folder',
		'forbidden',
		'info',
		'link',
		'lock',
		'pause',
		'picture',
		'play',
		'remove',
		'search',
		'see',
		'settings',
		'subtract',
		'sync',
		'unlock',
		'unsee',
		'wand',
		'wrong',
		'add',
		'alert',
		'attachment',
		'calendar',
		'check',
		'clipboard',
		'code',
		'colorpicker',
		'copy',
		'delete',
		'document',
		'download',
		'edit',
		'favorite',
		'folder',
		'forbidden',
		'info',
		'link',
		'lock',
		'pause',
		'picture',
		'play',
		'remove',
		'search',
		'see',
		'settings',
		'subtract',
		'sync',
		'unlock',
		'unsee',
		'wand',
		'wrong',
		'add',
		'alert',
		'attachment',
		'calendar',
		'check',
		'clipboard',
		'code',
		'colorpicker',
		'copy',
		'delete',
		'document',
		'download',
		'edit',
		'favorite',
		'folder',
		'forbidden',
		'info',
		'link',
		'lock',
		'pause',
		'picture',
		'play',
		'remove',
		'search',
		'see',
		'settings',
		'subtract',
		'sync',
		'unlock',
		'unsee',
		'wand',
		'wrong',
		'add',
		'alert',
		'attachment',
		'calendar',
		'check',
		'clipboard',
		'code',
		'colorpicker',
		'copy',
		'delete',
		'document',
		'download',
		'edit',
		'favorite',
		'folder',
		'forbidden',
		'info',
		'link',
		'lock',
		'pause',
		'picture',
		'play',
		'remove',
		'search',
		'see',
		'settings',
		'subtract',
		'sync',
		'unlock',
		'unsee',
		'wand',
		'wrong',
		'add',
		'alert',
		'attachment',
		'calendar',
		'check',
		'clipboard',
		'code',
		'colorpicker',
		'copy',
		'delete',
		'document',
		'download',
		'edit',
		'favorite',
		'folder',
		'forbidden',
		'info',
		'link',
		'lock',
		'pause',
		'picture',
		'play',
		'remove',
		'search',
		'see',
		'settings',
		'subtract',
		'sync',
		'unlock',
		'unsee',
		'wand',
		'wrong',
		'add',
		'alert',
		'attachment',
		'calendar',
		'check',
		'clipboard',
		'code',
		'colorpicker',
		'copy',
		'delete',
		'document',
		'download',
		'edit',
		'favorite',
		'folder',
		'forbidden',
		'info',
		'link',
		'lock',
		'pause',
		'picture',
		'play',
		'remove',
		'search',
		'see',
		'settings',
		'subtract',
		'sync',
		'unlock',
		'unsee',
		'wand',
		'wrong',
	];

	@property({ type: Array })
	iconlistFiltered: Array<string>;

	@property({ type: Array })
	colorlist = [
		'#000000',
		'#f79c37',
		'#fad634',
		'#f5c1bc',
		'#162335',
		'#000001',
		'#f79c38',
		'#fad633',
		'#f5c1bb',
		'#162336',
		'#000002',
		'#f79c36',
		'#fad632',
		'#f5c1bf',
		'#162330',
	];

	@state()
	private _currentColor: string;

	@state()
	private _currentIcon: string;

	private _changeIconColor(e: { target: HTMLInputElement; type: any; key: unknown }) {
		if (e.type == 'click') {
			this._currentColor = e.target.id;
		} else if (e.type == 'keyup' && e.key == 'Enter') {
			e.target.children[0].setAttribute('checked', 'true');
			this._currentColor = e.target.children[0].id;
		}
	}

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

	private _setBackground(color: string) {
		return 'background-color: ' + color;
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
		return html` <uui-icon-registry-essential>
			<uui-box headline="Select icon" id="box">
				<uui-icon
					class="header-icon"
					slot="headline"
					.name="${this._currentIcon}"
					.style="${this._setColor(this._currentColor)}"></uui-icon>

				${this.renderSearchbar()}
				<hr />

				<div id="palette">${this.renderPalette()}</div>

				<hr />
				<uui-scroll-container id="icon-selection">${this.renderIconSelection()}</uui-scroll-container>
				<hr />

				<uui-button look="secondary" label="close" @click="${this._close}">Close</uui-button>
				<uui-button color="positive" look="primary" @click="${this._save}" label="save">Save</uui-button>
			</uui-box>
		</uui-icon-registry-essential>`;
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

	renderPalette() {
		return html`${this.colorlist.map((color) => {
			return html`<label
				@keyup="${this._changeIconColor}"
				tabindex="0"
				for="${color}"
				class="colorspot"
				.style="${this._setBackground(color)}">
				<input
					type="radio"
					name="color"
					label="${color}"
					@click="${this._changeIconColor}"
					id="${color}"
					?checked="${color === this._currentColor ? true : false}" />
				<span class="checkmark">
					<uui-icon name="check"></uui-icon>
				</span>
			</label>`;
		})}`;
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

declare global {
	interface HTMLElementTagNameMap {
		'umb-icon-selector': UmbIconSelector;
	}
}
