import { css, customElement, html, property, state } from '@umbraco-cms/backoffice/external/lit';
import { generateAlias } from '@umbraco-cms/backoffice/utils';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { UMB_VALIDATION_EMPTY_LOCALIZATION_KEY, UmbFormControlMixin } from '@umbraco-cms/backoffice/validation';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UUIInputEvent } from '@umbraco-cms/backoffice/external/uui';
import type { UUIInputElement } from '@umbraco-cms/backoffice/external/uui';

@customElement('umb-input-with-alias')
export class UmbInputWithAliasElement extends UmbFormControlMixin<string, typeof UmbLitElement, undefined>(
	UmbLitElement,
) {
	@property({ type: String })
	label: string = '';

	@property({ type: String, reflect: false })
	alias = '';

	@property({ type: Boolean, reflect: true })
	required: boolean = false;

	@property({ type: Boolean, reflect: true })
	readonly = false;

	@property({ type: Boolean, reflect: true, attribute: 'alias-readonly' })
	aliasReadonly = false;

	@property({ type: Boolean, attribute: 'auto-generate-alias' })
	autoGenerateAlias?: boolean;

	@state()
	private _aliasLocked = true;

	protected override firstUpdated(): void {
		this.addValidator(
			'valueMissing',
			() => UMB_VALIDATION_EMPTY_LOCALIZATION_KEY,
			() => this.required && !this.value,
		);

		this.shadowRoot?.querySelectorAll<UUIInputElement>('uui-input').forEach((x) => this.addFormControlElement(x));
	}

	override focus() {
		return this.shadowRoot?.querySelector<UUIInputElement>('uui-input')?.focus();
	}

	#onNameChange(e: UUIInputEvent) {
		if (!(e instanceof UUIInputEvent)) return;

		const target = e.composedPath()[0] as UUIInputElement;

		if (typeof target?.value === 'string') {
			this.value = e.target.value.toString();
			if (this.autoGenerateAlias && this._aliasLocked) {
				// Generate alias if it's locked and auto-generate is enabled
				this.alias = generateAlias(this.value);
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
	#onAliasBlur() {
		// If the alias is empty, then try to generate one [NL]
		if (!this.alias && this._aliasLocked === false) {
			this.alias = generateAlias(this.value ?? '');
			this.dispatchEvent(new UmbChangeEvent());
		}
	}

	#onToggleAliasLock(event: CustomEvent) {
		this._aliasLocked = !this._aliasLocked;
		if (!this.alias && this._aliasLocked) {
			// Reenable auto-generate if alias is empty and locked.
			this.autoGenerateAlias = true;
		} else {
			this.autoGenerateAlias = false;
		}

		if (!this._aliasLocked) {
			(event.target as UUIInputElement)?.focus();
		}
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
				@input=${this.#onNameChange}
				?required=${this.required}
				?readonly=${this.readonly}>
				${!this.readonly
					? html`
							<uui-input-lock
								id="alias"
								name="alias"
								slot="append"
								label=${aliasLabel}
								placeholder=${aliasLabel}
								.value=${this.alias}
								?auto-width=${!!this.value}
								?locked=${this._aliasLocked && !this.aliasReadonly}
								?readonly=${this.aliasReadonly}
								?required=${this.required}
								@input=${this.#onAliasChange}
								@blur=${this.#onAliasBlur}
								@lock-change=${this.#onToggleAliasLock}>
							</uui-input-lock>
						`
					: html`<span id="alias" class="muted" slot="append">${this.alias}</span>`}
			</uui-input>
		`;
	}

	static override readonly styles = css`
		:host {
			display: contents;
		}
		#name {
			width: 100%;
			flex: 1 1 auto;
			align-items: center;
		}

		#alias {
			&.muted {
				opacity: 0.55;
				padding: var(--uui-size-1, 3px) var(--uui-size-space-3, 9px);
			}
		}

		:host(:invalid:not([pristine])) {
			color: var(--uui-color-invalid);
		}
		:host(:invalid:not([pristine])) > uui-input {
			border-color: var(--uui-color-invalid);
		}
	`;
}

export { UmbInputWithAliasElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-with-alias': UmbInputWithAliasElement;
	}
}
