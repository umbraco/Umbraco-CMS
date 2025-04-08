import { UMB_ICON_REGISTRY_CONTEXT } from '../icon-registry.context-token.js';
import type { UmbIconDefinition } from '../types.js';
import type { UmbIconPickerModalData, UmbIconPickerModalValue } from './icon-picker-modal.token.js';
import { css, customElement, html, nothing, query, repeat, state } from '@umbraco-cms/backoffice/external/lit';
import { extractUmbColorVariable, umbracoColors } from '@umbraco-cms/backoffice/resources';
import { umbFocus } from '@umbraco-cms/backoffice/lit-element';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { UUIColorSwatchesEvent } from '@umbraco-cms/backoffice/external/uui';

@customElement('umb-icon-picker-modal')
export class UmbIconPickerModalElement extends UmbModalBaseElement<UmbIconPickerModalData, UmbIconPickerModalValue> {
	#icons?: Array<UmbIconDefinition>;

	@query('#search')
	private _searchInput?: HTMLInputElement;

	@state()
	private _iconsFiltered?: Array<UmbIconDefinition>;

	@state()
	private _colorList = umbracoColors.filter((color) => !color.legacy);

	@state()
	private _currentIcon?: string;

	@state()
	private _currentColor = 'text';

	constructor() {
		super();
		this.consumeContext(UMB_ICON_REGISTRY_CONTEXT, (context) => {
			this.observe(context.approvedIcons, (icons) => {
				this.#icons = icons;
				this.#filterIcons();
			});
		});
	}

	#filterIcons() {
		if (!this.#icons) return;
		const value = this._searchInput?.value;
		if (value) {
			this._iconsFiltered = this.#icons.filter((icon) => icon.name.toLowerCase().includes(value.toLowerCase()));
		} else {
			this._iconsFiltered = this.#icons;
		}
	}

	override connectedCallback() {
		super.connectedCallback();
		this._iconsFiltered = this.#icons;

		if (this.modalContext) {
			this.observe(
				this.modalContext?.value,
				(newValue) => {
					this._currentIcon = newValue?.icon;
					this._currentColor = newValue?.color ?? 'text';
				},
				'_observeModalContextValue',
			);
		}
	}

	#changeIcon(e: InputEvent | KeyboardEvent, iconName: string) {
		if (e.type == 'click' || (e.type == 'keyup' && (e as KeyboardEvent).key == 'Enter')) {
			this.modalContext?.updateValue({ icon: iconName });
		}
	}

	#onColorChange(e: UUIColorSwatchesEvent) {
		const colorAlias = e.target.value;
		this.modalContext?.updateValue({ color: colorAlias });
		this._currentColor = colorAlias;
	}

	override render() {
		// TODO: Missing localization in general. [NL]
		return html`
			<umb-body-layout headline="Select Icon">
				<div id="container">
					${this.renderSearch()}
					<hr />
					<uui-color-swatches
						.value=${this._currentColor}
						label="Color switcher for icons"
						@change=${this.#onColorChange}>
						${
							// TODO: Missing localization for the color aliases. [NL]
							this._colorList.map(
								(color) => html`
									<uui-color-swatch
										label=${color.alias}
										title=${color.alias}
										value=${color.alias}
										style="--uui-swatch-color: var(${color.varName})"></uui-color-swatch>
								`,
							)
						}
					</uui-color-swatches>
					<hr />
					<uui-scroll-container id="icons">${this.renderIcons()}</uui-scroll-container>
				</div>
				<uui-button
					slot="actions"
					label=${this.localize.term('general_close')}
					@click=${this._rejectModal}></uui-button>
				<uui-button
					slot="actions"
					color="positive"
					look="primary"
					@click=${this._submitModal}
					label=${this.localize.term('general_submit')}></uui-button>
			</umb-body-layout>
		`;
	}

	renderSearch() {
		return html` <uui-input
			type="search"
			placeholder=${this.localize.term('placeholders_filter')}
			label=${this.localize.term('placeholders_filter')}
			id="search"
			@keyup=${this.#filterIcons}
			${umbFocus()}>
			<uui-icon name="search" slot="prepend" id="search_icon"></uui-icon>
		</uui-input>`;
	}

	renderIcons() {
		return this._iconsFiltered
			? repeat(
					this._iconsFiltered,
					(icon) => icon.name,
					(icon) => html`
						<uui-button
							label=${icon.name}
							title=${icon.name}
							class=${icon.name === this._currentIcon ? 'selected' : ''}
							@click=${(e: InputEvent) => this.#changeIcon(e, icon.name)}
							@keyup=${(e: KeyboardEvent) => this.#changeIcon(e, icon.name)}>
							<uui-icon
								style="--uui-icon-color: var(${extractUmbColorVariable(this._currentColor)})"
								name=${icon.name}></uui-icon>
						</uui-button>
					`,
				)
			: nothing;
	}

	static override styles = [
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

			#search {
				width: 100%;
				align-items: center;
			}
			#search_icon {
				padding-left: var(--uui-size-space-2);
			}

			#icons {
				line-height: 0;
				display: grid;
				grid-template-columns: repeat(auto-fit, minmax(40px, calc((100% / 12) - 10px)));
				gap: 10px;
				overflow-y: scroll;
				max-height: 100%;
				min-height: 0;
				padding: 2px;
			}

			#icons uui-button {
				border-radius: var(--uui-border-radius);
				font-size: 16px; /* specific for icons */
			}
			#icons uui-button:focus,
			#icons uui-button:hover,
			#icons uui-button.selected {
				outline: 2px solid var(--uui-color-selected);
			}

			uui-button[slot='actions'] {
				margin-left: var(--uui-size-space-4);
			}

			uui-color-swatches {
				margin: 0;
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
