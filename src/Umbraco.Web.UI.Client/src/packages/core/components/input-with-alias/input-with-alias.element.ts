import { css, customElement, html, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { UmbFormControlMixin } from '@umbraco-cms/backoffice/validation';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { type UUIInputElement, UUIInputEvent } from '@umbraco-cms/backoffice/external/uui';
import { generateAlias } from '@umbraco-cms/backoffice/utils';

@customElement('umb-input-with-alias')
export class UmbInputWithAliasElement extends UmbFormControlMixin<string>(UmbLitElement) {
	@property({ type: String })
	label: string = '';

	@property({ type: String, reflect: false })
	alias?: string;

	@state()
	private _aliasLocked = true;

	firstUpdated() {
		this.shadowRoot?.querySelectorAll<UUIInputElement>('uui-input').forEach((x) => this.addFormControlElement(x));
	}

	focus() {
		return this.shadowRoot?.querySelector<UUIInputElement>('uui-input')?.focus();
	}

	#onNameChange(event: UUIInputEvent) {
		if (event instanceof UUIInputEvent) {
			const target = event.composedPath()[0] as UUIInputElement;

			if (typeof target?.value === 'string') {
				const oldName = this.value;
				const oldAlias = this.alias ?? '';
				this.value = event.target.value.toString();
				if (this._aliasLocked) {
					// If locked we will update the alias, but only if it matches the generated alias of the old name [NL]
					const expectedOldAlias = generateAlias(oldName ?? '');
					// Only update the alias if the alias matches a generated alias of the old name (otherwise the alias is considered one written by the user.) [NL]
					if (expectedOldAlias === oldAlias) {
						this.alias = generateAlias(this.value);
					}
				}
				this.dispatchEvent(new UmbChangeEvent());
			}
		}
	}

	#onAliasChange(e: UUIInputEvent) {
		if (event instanceof UUIInputEvent) {
			const target = event.composedPath()[0] as UUIInputElement;
			if (typeof target?.value === 'string') {
				this.alias = target.value;
				this.dispatchEvent(new UmbChangeEvent());
			}
		}
		e.stopPropagation();
	}

	#onToggleAliasLock() {
		this._aliasLocked = !this._aliasLocked;
	}

	render() {
		// Localizations: [NL]
		return html`
			<uui-input id="name" label=${this.label} .value=${this.value} @input="${this.#onNameChange}">
				<!-- TODO: should use UUI-LOCK-INPUT, but that does not fire an event when its locked/unlocked -->
				<uui-input
					name="alias"
					slot="append"
					label="alias"
					@input=${this.#onAliasChange}
					.value=${this.alias}
					placeholder="Enter alias..."
					?disabled=${this._aliasLocked}>
					<!-- TODO: validation for bad characters -->
					<div @click=${this.#onToggleAliasLock} @keydown=${() => ''} id="alias-lock" slot="prepend">
						<uui-icon name=${this._aliasLocked ? 'icon-lock' : 'icon-unlocked'}></uui-icon>
					</div>
				</uui-input>
			</uui-input>
		`;
	}

	static styles = css`
		#name {
			width: 100%;
			flex: 1 1 auto;
			align-items: center;
		}

		:host(:invalid:not([pristine])) {
			color: var(--uui-color-danger);
		}
		:host(:invalid:not([pristine])) > uui-input {
			border-color: var(--uui-color-danger);
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
	`;
}

export default UmbInputWithAliasElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-with-alias': UmbInputWithAliasElement;
	}
}
