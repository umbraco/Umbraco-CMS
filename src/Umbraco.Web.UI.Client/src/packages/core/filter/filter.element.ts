import { UmbFilterContext } from './filter.context.js';
import { css, customElement, html, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

/**
 * Provides the filter context for the filter element it wraps, and frames it with the filter's label and a button
 * that clears the filter's values.
 */
@customElement('umb-filter')
export class UmbFilterElement extends UmbLitElement {
	#context = new UmbFilterContext(this);

	#alias?: string;
	public get alias(): string | undefined {
		return this.#alias;
	}
	public set alias(alias: string | undefined) {
		this.#alias = alias;
		if (alias) {
			this.#context.setAlias(alias);
		}
	}

	@property()
	label?: string;

	@state()
	private _hasValues = false;

	constructor() {
		super();
		this.observe(this.#context.values, (values) => (this._hasValues = values.length > 0), null);
	}

	#onClear() {
		this.#context.clearAllValues();
	}

	override render() {
		return html`
			<div id="header">
				<span id="label">${this.localize.string(this.label ?? '')}</span>
				<uui-button
					compact
					look="secondary"
					label=${this.localize.term('general_clear')}
					?disabled=${!this._hasValues}
					@click=${this.#onClear}></uui-button>
			</div>
			<slot></slot>
		`;
	}

	static override styles = [
		css`
			:host {
				display: flex;
				flex-direction: column;
				gap: var(--uui-size-space-2);
			}

			#header {
				display: flex;
				justify-content: space-between;
				align-items: center;
				min-height: var(--uui-size-8);
			}

			#label {
				font-weight: 700;
				font-size: var(--uui-type-default-size);
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-filter': UmbFilterElement;
	}
}
