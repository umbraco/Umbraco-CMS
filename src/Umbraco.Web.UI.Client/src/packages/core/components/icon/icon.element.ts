import { extractUmbColorVariable } from '../../resources/extractUmbColorVariable.function.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { html, customElement, property, state, ifDefined, css, styleMap } from '@umbraco-cms/backoffice/external/lit';
import type { StyleInfo } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

/**
 * @element umb-icon
 * @description A wrapper for the uui-icon component with color alias support
 * @augments {UmbLitElement}
 */
@customElement('umb-icon')
export class UmbIconElement extends UmbLitElement {
	#color?: string;
	#fallbackColor?: string;

	@state()
	private _icon?: string;

	@state()
	private _style: StyleInfo = {};

	/**
	 * Color alias or a color code directly.\
	 * If a color has been set via the name property, this property will override it.
	 */
	@property({ type: String })
	public set color(value: string) {
		this.#color = value;
		this.#updateColorStyle();
	}
	public get color(): string | undefined {
		return this.#color || this.#fallbackColor;
	}

	/**
	 * The icon name. Can be appended with a color.\
	 * Example **icon-heart color-red**
	 */
	@property({ type: String })
	public set name(value: string | undefined) {
		const [icon, color] = value ? value.split(' ') : [];
		this.#fallbackColor = color;
		this._icon = icon;
		this.#updateColorStyle();
	}
	public get name(): string | undefined {
		return this._icon;
	}

	#updateColorStyle() {
		const value = this.#color || this.#fallbackColor;

		if (!value) {
			this._style = { '--uui-icon-color': 'inherit' };
			return;
		}

		const color = value.replace('color-', '');
		const variable = extractUmbColorVariable(color);
		const styling = variable ? `var(${variable})` : color;

		this._style = { '--uui-icon-color': styling };
	}

	override render() {
		return html`<uui-icon name=${ifDefined(this._icon)} style=${styleMap(this._style)}></uui-icon>`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				display: flex;
				justify-content: center;
				align-items: center;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-icon': UmbIconElement;
	}
}
