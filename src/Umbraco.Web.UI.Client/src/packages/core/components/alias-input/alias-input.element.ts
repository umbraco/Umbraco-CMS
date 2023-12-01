import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { css, html, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import { FormControlMixin, UUIInputEvent } from '@umbraco-cms/backoffice/external/uui';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

@customElement('umb-alias-input')
export class UmbAliasInput2Element extends FormControlMixin(UmbLitElement) {
	protected getFormElement() {
		return undefined;
	}

	@property()
	public set value(val: string | null) {
		if (val === null) return;
		this.locked = true;
		super.value = val;
		this.pristine = false;
	}
	public get value(): string {
		return super.value as string;
	}

	private _listenToElement: HTMLElement | null = null;

	public set listenToElement(el: HTMLElement | null) {
		if (this._listenToElement) {
			this._listenToElement.removeEventListener('input', this.#parentChange);
		}
		this._listenToElement = el;
		this._listenToElement?.addEventListener('input', this.#parentChange);
	}
	public get listenToElement(): HTMLElement | null {
		return this._listenToElement;
	}

	@state()
	locked = true;

	#parentIsInput: boolean;

	constructor() {
		super();
		this.listenToElement = (this.getHostElement() as Element).parentElement;
		this.#parentIsInput = this.listenToElement?.nodeName === 'UUI-INPUT';
	}

	disconnectedCallback(): void {
	  this.listenToElement?.removeEventListener('input', this.#parentChange);
	}

	#onToggleAliasLock() {
		this.locked = !this.locked;
	}

	#onChange(e: UUIInputEvent) {
		super.value = e.target.value as string;
		this.dispatchEvent(new UmbChangeEvent());
	}

	#parentChange = (e: Event) => {
		if (!this.pristine) return;
		super.value = (e as UUIInputEvent).target.value as string;
		this.dispatchEvent(new UmbChangeEvent());
	};

	render() {
		return html`<uui-input
			@change=${this.#onChange}
			value="${this.value}"
			placeholder=${this.localize.term('placeholders_enterAlias')}
			?disabled=${this.locked}
			label=${this.localize.term('placeholders_enterAlias')}
			class="${this.#parentIsInput ? 'inside' : 'outside'}">
			<div role="button" id="alias-lock" @click=${this.#onToggleAliasLock} @keydown=${() => ''} slot="prepend">
				<uui-icon name="${this.locked ? 'icon-lock' : 'icon-unlocked'}"></uui-icon>
			</div>
		</uui-input>`;
	}

	static styles = [
		css`
			:host {
				display: contents;
			}
			uui-input {
				--uui-input-border-width: 0;
				height: 100%;
			}
			uui-input.outside:disabled {
				background-color: transparent;
			}

			#alias-lock {
				display: flex;
				height: 100%;
				align-items: center;
				justify-content: center;
				cursor: pointer;
			}
		`,
	];
}

export default UmbAliasInput2Element;

declare global {
	interface HTMLElementTagNameMap {
		'umb-alias-input': UmbAliasInput2Element;
	}
}
