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
		if (!first) return nothing;

		return html`
			<!-- <button id="sign-icon" type="button">${this.#renderIcons()}</button> -->
			<div class="infobox">${this.#renderOptions()}</div>
		`;
	}
	#renderIcons() {
		return this._signs
			? repeat(
					this._signs,
					(c) => c.alias,
					(c, i) => {
						return html`<span class="badge-icon ${i === 0 ? 'inner' : ''}">${c.component}</span>`;
					},
				)
			: nothing;
	}
	#renderOptions() {
		return this._signs
			? repeat(
					this._signs,
					(c) => c.alias,
					(c, i) => {
						return html`<div class="signs-container">
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
				--row-h: 18px;
				--open-delay: 300ms;
				--scale-dur: 300ms;
				--fade-dur: 160ms;
				--ease: cubic-bezier(0.2, 0.8, 0.2, 1);
			}
			/* #sign-icon {
				position: relative;
				display: inline-flex;
				border: 0;
				background: red;
				cursor: pointer;
				padding: 1px;
				border-radius: 50%;
				color: var(--sign-bundle-text-color);
			} */

			/* .badge-icon {
				position: absolute;
				right: 7px;
				bottom: -2px;
				font-size: 8px;
				border-radius: 50%;
				background: var(--sign-bundle-bg, transparent);
			}
			.badge-icon > * {
				filter: drop-shadow(0 1px 1px rgba(0, 0, 0, 0.35));
			} */
			/* .inner {
				z-index: 1;
				right: 1px;
				bottom: -2px;
			} */

			.infobox {
				position: fixed;
				position-anchor: --entity-sign;
				bottom: anchor(bottom);
				left: anchor(right);
				border-radius: 3px;
				font-size: 12px;
				transform: scale(0.7);
				transform-origin: bottom left;
				opacity: 0.85;
				overflow: hidden;
				max-height: var(--row-h);
				background: transparent;
				box-shadow: none;
				transition:
					transform var(--scale-dur) var(--ease) var(--open-delay),
					opacity var(--fade-dur) ease var(--open-delay);
				will-change: transform, opacity;
			}

			.signs-container {
				display: flex;
				align-items: center;
				gap: 3px;
				padding: 4px;
				height: var(--row-h);
			}

			.infobox .signs-container .label {
				display: none;
			}
			.infobox .badge-icon {
				font-size: 10px;
				border-radius: 50%;
				background: var(--sign-bundle-bg, transparent);
			}
			.infobox .badge-icon > * {
				filter: drop-shadow(0 1px 1px rgba(0, 0, 0, 0.35));
			}
			.infobox:hover {
				transform: scale(1);
				opacity: 1;
				overflow: visible;
				max-height: none;
				background: white;
				box-shadow:
					0 1px 2px rgba(0, 0, 0, 0.25),
					0 0 0 1px rgba(0, 0, 0, 0.06);
				transition-delay: 0ms, 0ms, 0ms, 0ms;
			}

			.infobox:hover .signs-container .label {
				display: inline;
			}
			.infobox:hover .badge-icon {
				font-size: 12px;
				background: transparent;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-entity-sign-bundle': UmbEntitySignBundleElement;
	}
}
