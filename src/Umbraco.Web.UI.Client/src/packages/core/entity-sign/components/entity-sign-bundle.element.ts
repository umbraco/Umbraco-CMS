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

	private _open = false;
	private _hoverTimer?: number;

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

	#armOpen = () => {
		if (this._hoverTimer) clearTimeout(this._hoverTimer);
		this._hoverTimer = window.setTimeout(() => {
			this._open = true;
			this.requestUpdate();
			this._hoverTimer = undefined;
		}, 500);
	};

	#cancelOpen = () => {
		if (this._hoverTimer) {
			clearTimeout(this._hoverTimer);
			this._hoverTimer = undefined;
		}
		if (this._open) {
			this._open = false;
			this.requestUpdate();
		}
	};

	override render() {
		if (!this._signs || this._signs.length === 0) return nothing;

		const first = this._signs?.[0];
		if (!first) return nothing;

		return html`
			<div
				class="infobox ${this._open ? 'is-open' : ''}"
				@mouseenter=${this.#armOpen}
				@mouseleave=${this.#cancelOpen}
				style=${`--count:${this._signs.length}`}>
				${this.#renderOptions()}
			</div>
		`;
	}
	#renderOptions() {
		return this._signs
			? repeat(
					this._signs,
					(c) => c.alias,
					(c, i) => {
						return html`<div class="sign-container ${i === 0 ? 'first-icon' : ''}">
							<span class="badge-icon" style=${`--i:${i}`}>${c.component}</span
							><span class="label">${this._labels.get(c.alias)}</span>
						</div>`;
					},
				)
			: nothing;
	}

	static override styles = [
		css`
			:host {
				anchor-name: --entity-sign;
				--row-h: 22px;
				--dur: 220ms;
				--fade: 160ms;
				--ease: cubic-bezier(0.2, 0.8, 0.2, 1);
			}
			.infobox {
				position: fixed;
				position-anchor: --entity-sign;
				bottom: anchor(bottom);
				left: anchor(right);
				font-size: 8px;
				display: grid;
				background: transparent;
				box-shadow: none;
				transition:
					transform var(--dur) var(--ease),
					background-color var(--fade) ease,
					box-shadow var(--fade) ease;
			}

			.infobox > .sign-container {
				grid-area: 1 / 1;
				display: flex;
				align-items: center;
				gap: 3px;
			}

			.infobox > .sign-container.first-icon {
				z-index: 1;
			}

			.infobox .sign-container .label {
				opacity: 0;
				transition: opacity var(--fade) ease;
			}

			.badge-icon {
				display: grid;
				place-items: center;
				background: var(--sign-bundle-bg, transparent);
				border-radius: 50%;
				transition: transform var(--dur) var(--ease);
			}

			.infobox .sign-container .badge-icon {
				transform: translateX(-5px);
			}
			.infobox .sign-container.first-icon .badge-icon {
				transform: none;
			}

			.infobox.is-open {
				display: flex;
				flex-direction: column;
				background: white;
				color: black;
				padding: 4px;
				font-size: 12px;
				box-shadow:
					0 1px 2px rgba(0, 0, 0, 0.25),
					0 0 0 1px rgba(0, 0, 0, 0.06);
			}

			.infobox.is-open .sign-container .label {
				opacity: 1;
			}

			.infobox.is-open .sign-container .badge-icon {
				transform: none;
				background: white;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-entity-sign-bundle': UmbEntitySignBundleElement;
	}
}
