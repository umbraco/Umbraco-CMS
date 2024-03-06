import { extractUmbColorVariable } from '../../resources/extractUmbColorVariable.function.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { html, customElement, property, state, ifDefined } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

/**
 * @element umb-icon
 * @description A wrapper for the uui-icon component with color alias support
 * @extends {UmbLitElement}
 */
@customElement('umb-icon')
export class UmbIconElement extends UmbLitElement {
	@state()
	private _icon?: string;

	@state()
	private _color?: string;

	@property({ type: String })
	public set color(value: string) {
		if (!value) return;
		this.#setColorStyle(value);
	}
	public get color(): string {
		return this._color ?? '';
	}

	#setColorStyle(value: string) {
		const alias = value.replace('color-', '');
		const variable = extractUmbColorVariable(alias);
		this._color = alias ? (variable ? `--uui-icon-color: var(${variable})` : `--uui-icon-color: ${alias}`) : undefined;
	}

	@property({ type: String })
	public set name(value: string | undefined) {
		const [icon, alias] = value ? value.split(' ') : [];
		if (alias) this.#setColorStyle(alias);
		this._icon = icon;
	}
	public get name(): string | undefined {
		return this._icon;
	}

	render() {
		return html`<uui-icon name=${ifDefined(this._icon)} style=${ifDefined(this._color)}></uui-icon>`;
	}

	static styles = [UmbTextStyles];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-icon': UmbIconElement;
	}
}
