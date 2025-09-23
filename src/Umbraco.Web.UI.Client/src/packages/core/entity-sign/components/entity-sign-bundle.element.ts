import { customElement, html, nothing, property, repeat, state, css } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '../../lit-element/lit-element.element';
import type { ManifestEntitySign } from '../types';
import { UmbExtensionsElementAndApiInitializer } from '@umbraco-cms/backoffice/extension-api';
import type { UmbObserverController } from '@umbraco-cms/backoffice/observable-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';

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

	override render() {
		if (!this._signs || this._signs.length === 0) return nothing;

		const first = this._signs?.[0];
		console.log(first);
		if (!first) return nothing;

		return html`
			<button id="sign-icon" type="button">
				<umb-icon class="inner" name="icon-lock"></umb-icon>
				<umb-icon name="icon-grid"></umb-icon>
			</button>
			<div class="infobox">${this.#renderOptions()}</div>
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
			:host {
				position: absolute;
				anchor-name: --entity-sign;
			}
			#sign-icon {
				position: relative;
				display: inline-flex;
				border: 0;
				background: transparent;
				cursor: pointer;
				padding: 1px;
				border-radius: 50%;
				color: var(--sign-bundle-text-color);
			}

			umb-icon {
				position: absolute;
				right: 7px;
				bottom: -2px;
				font-size: 8px;
				border-radius: 50%;
				background: var(--sign-bundle-bg, transparent);
				padding: 1px;
			}
			.inner {
				z-index: 1;
				right: 1px;
				bottom: -2px;
			}

			@supports (position-anchor: --my-name) {
				.infobox {
					position: fixed;
					position-anchor: --entity-sign;
					margin-left: -15px;
					margin-top: -55px;
					width: 550px;
					height: 120px;
					border: 1px solid black;
					top: anchor(top);
					left: anchor(right);
					border-radius: 6px;
					background: white;
					opacity: 0;
					transform: translateY(-4px);
					pointer-events: none;
					transition:
						opacity 120ms ease,
						transform 120ms ease;
				}
			}

			:host(:hover) .infobox {
				opacity: 1;
				transform: translateY(0);
				pointer-events: auto;
			}

			.label {
				display: flex;
				align-items: center;
				gap: 6px;
				padding: 4px 6px;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-entity-sign-bundle': UmbEntitySignBundleElement;
	}
}
