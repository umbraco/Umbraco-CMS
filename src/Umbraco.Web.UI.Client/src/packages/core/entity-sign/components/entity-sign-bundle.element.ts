import { customElement, html, nothing, property, repeat, state, css } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '../../lit-element/lit-element.element';
import type { ManifestEntitySign } from '../types';
import { UmbExtensionsElementAndApiInitializer } from '@umbraco-cms/backoffice/extension-api';
import type { UmbObserverController } from '@umbraco-cms/backoffice/observable-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import type { UUIMenuItemEvent } from '@umbraco-cms/backoffice/external/uui';
import type { UmbDropdownElement } from '@umbraco-cms/backoffice/components';

@customElement('umb-entity-sign-bundle')
export class UmbEntitySignBundleElement extends UmbLitElement {
	@property({ type: String, attribute: 'entity-type', reflect: false })
	get entityType(): string | undefined {
		return this._entityType;
	}

	set entityType(value: string | undefined) {
		this._entityType = value ?? undefined;
		this.#gotEntityType();
	}

	@state() private _entityType?: string;
	@state() private _signs?: Array<any>;
	@state() private _labels: Map<string, string> = new Map();

	#signLabelObservations: Array<UmbObserverController<string>> = [];

	#manifestFilter = (manifest: ManifestEntitySign) => {
		if (manifest.forEntityTypes && !manifest.forEntityTypes.includes(this._entityType!)) return false;
		return true;
	};

	#gotEntityType() {
		if (!this._entityType) {
			this.removeUmbControllerByAlias('extensionsInitializer');
			this._signs = [];
			return;
		}

		new UmbExtensionsElementAndApiInitializer(
			this,
			umbExtensionsRegistry,
			'entitySign',
			(manifest: ManifestEntitySign) => [{ meta: manifest.meta }],
			this.#manifestFilter,
			(signs) => {
				// Clean up old observers
				this.#signLabelObservations.forEach((o) => this.removeUmbController(o));
				this.#signLabelObservations = [];

				// Setup label observers
				signs.forEach((sign) => {
					if (sign.api?.label) {
						const obs = this.observe(sign.api.label, (label) => {
							this._labels.set(sign.alias, label);
							this.requestUpdate('_labels');
						});
						this.#signLabelObservations.push(obs);
					} else if (sign.api?.getLabel) {
						this._labels.set(sign.alias, sign.api.getLabel() ?? '');
						this.requestUpdate('_labels');
					}
				});

				this._signs = signs;
			},
			'extensionsInitializer',
		);
	}

	#openDropdown() {
		const dd = this.renderRoot.querySelector<UmbDropdownElement>('#myDropdown');
		dd?.openDropdown();
	}

	#closeDropdown() {
		const dd = this.renderRoot.querySelector<UmbDropdownElement>('#myDropdown');
		dd?.closeDropdown();
	}

	override render() {
		if (!this._signs || this._signs.length === 0) return nothing;

		const first = this._signs[0];
		console.log(first);
		if (!first) return nothing;

		return html`
			<umb-dropdown id="myDropdown" hide-expand compact look="placeholder" placement="right">
				<span
					id="sign-icon"
					slot="label"
					@mouseenter=${this.#openDropdown}
					@mouseleave=${this.#closeDropdown}
					@click=${(e: MouseEvent) => {
						e.preventDefault();
						e.stopPropagation();
						console.log('click stopped');
					}}
					>${first.component}</span
				>
				<div class="labels-pop">${this.#renderOptions()}</div>
			</umb-dropdown>
		`;
	}
	#renderOptions() {
		return this._signs
			? repeat(
					this._signs,
					(c) => c.alias,
					(c) => {
						return html`<div class="label"><span>${c.component}</span><span>${this._labels.get(c.alias)}</span></div>`;
					},
				)
			: nothing;
	}

	static override styles = [
		css`
			umb-dropdown::part(dropdown-button) {
				pointer-events: none;
			}
			#sign-icon {
				right: -5px;
				width: 14px;
				height: 14px;
				font-size: 10px;
				border-radius: 50%;
				background: var(--uui-color-surface);
				line-height: 14px;
				display: flex;
				align-items: center;
				justify-content: center;
				cursor: pointer;
			}

			#sign-icon:hover {
				background: red;
			}
			umb-dropdown {
				position: relative;
			}
			.labels-pop {
				position: absolute;
				left: -10px;
				background: var(--uui-color-surface);
				border: 1px solid black;
				padding: 10px;
				display: flex;
				flex-direction: column;
				gap: 3px;
			}
			.label {
				display: flex;
				align-items: center;
				gap: 5px;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-entity-sign-bundle': UmbEntitySignBundleElement;
	}
}
