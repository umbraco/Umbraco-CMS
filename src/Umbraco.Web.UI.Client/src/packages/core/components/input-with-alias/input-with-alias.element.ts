import { css, customElement, html, nothing, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { UmbFormControlMixin } from '@umbraco-cms/backoffice/validation';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { type UUIInputElement, UUIInputEvent } from '@umbraco-cms/backoffice/external/uui';
import { generateAlias } from '@umbraco-cms/backoffice/utils';

@customElement('umb-input-with-alias')
export class UmbInputWithAliasElement extends UmbFormControlMixin<string, typeof UmbLitElement>(UmbLitElement) {
	@property({ type: String })
	label: string = '';

	@property({ type: String, reflect: false })
	alias?: string;

	@property({ type: Boolean, reflect: true, attribute: 'alias-readonly' })
	aliasReadonly = false;

	@property({ type: Boolean, attribute: 'auto-generate-alias' })
	autoGenerateAlias?: boolean;

	@state()
	private _aliasLocked = true;

	override firstUpdated() {
		this.shadowRoot?.querySelectorAll<UUIInputElement>('uui-input').forEach((x) => this.addFormControlElement(x));
	}

	override focus() {
		return this.shadowRoot?.querySelector<UUIInputElement>('uui-input')?.focus();
	}

	#onNameChange(e: UUIInputEvent) {
		if (!(e instanceof UUIInputEvent)) return;

		const target = e.composedPath()[0] as UUIInputElement;

		if (typeof target?.value === 'string') {
			const oldName = this.value;
			const oldAlias = this.alias ?? '';
			this.value = e.target.value.toString();
			if (this.autoGenerateAlias && this._aliasLocked) {
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

	#onAliasChange(e: UUIInputEvent) {
		e.stopPropagation();
		if (!(e instanceof UUIInputEvent)) return;

		const target = e.composedPath()[0] as UUIInputElement;

		if (typeof target?.value === 'string') {
			this.alias = target.value;
			this.dispatchEvent(new UmbChangeEvent());
		}
	}

	#onToggleAliasLock() {
		this._aliasLocked = !this._aliasLocked;
	}

	override render() {
		const nameLabel = this.label ?? this.localize.term('placeholders_entername');
		const aliasLabel = this.localize.term('placeholders_enterAlias');

		return html`
			<uui-input
				id="name"
				placeholder=${nameLabel}
				label=${nameLabel}
				.value=${this.value}
				@input=${this.#onNameChange}>
				<!-- TODO: should use UUI-LOCK-INPUT, but that does not fire an event when its locked/unlocked -->
				<uui-input
					auto-width
					name="alias"
					slot="append"
					label=${aliasLabel}
					.value=${this.alias}
					placeholder=${aliasLabel}
					?disabled=${this._aliasLocked && !this.aliasReadonly}
					?readonly=${this.aliasReadonly}
					@input=${this.#onAliasChange}>
					<!-- TODO: validation for bad characters -->
					${this.aliasReadonly
						? nothing
						: html`
								<div @click=${this.#onToggleAliasLock} @keydown=${() => ''} id="alias-lock" slot="prepend">
									<uui-icon name=${this._aliasLocked ? 'icon-lock' : 'icon-unlocked'}></uui-icon>
								</div>
							`}
				</uui-input>
			</uui-input>
		`;
	}

	static override styles = css`
		#name {
			width: 100%;
			flex: 1 1 auto;
			align-items: center;
		}

		#name > uui-input {
			max-width: 50%;
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
