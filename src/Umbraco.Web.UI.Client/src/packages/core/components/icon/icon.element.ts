import { extractUmbColorVariable } from '../../resources/extractUmbColorVariable.function.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { html, customElement, property, state, ifDefined } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-icon')
export class UmbIconElement extends UmbLitElement {
	@state()
	private icon?: string;

	@property({ type: String })
	color?: string;

	@property({ type: String })
	public set name(value: string | undefined) {
		const [icon, alias] = value ? value.replace('color-', '').split(' ') : [];

		const variable = extractUmbColorVariable(alias);
		this.color = alias ? (variable ? `--uui-icon-color: var(${variable})` : `--uui-icon-color: ${alias}`) : undefined;

		this.icon = icon;
	}
	public get name(): string | undefined {
		return this.icon;
	}

	render() {
		return html`<uui-icon name=${ifDefined(this.icon)} style=${ifDefined(this.color)}></uui-icon>`;
	}

	static styles = [UmbTextStyles];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-icon': UmbIconElement;
	}
}
