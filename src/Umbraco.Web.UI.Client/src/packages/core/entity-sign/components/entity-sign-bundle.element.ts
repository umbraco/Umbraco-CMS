import { UmbLitElement } from '../../lit-element/lit-element.element.js';
import type { ManifestEntitySign } from '../types.js';
import { customElement, html, nothing, property, repeat, state, css } from '@umbraco-cms/backoffice/external/lit';
import { UmbExtensionsElementAndApiInitializer } from '@umbraco-cms/backoffice/extension-api';
import type { UmbObserverController } from '@umbraco-cms/backoffice/observable-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbEntityFlag } from '../../entity/types.js';

@customElement('umb-entity-sign-bundle')
export class UmbEntitySignBundleElement extends UmbLitElement {
	#entityType?: string;
	#entityFlagAliases?: Array<string>;

	@property({ type: String, attribute: 'entity-type', reflect: false })
	get entityType(): string | undefined {
		return this.#entityType;
	}

	set entityType(value: string | undefined) {
		this.#entityType = value;
		this.#gotProperties();
	}

	@property({ type: Array, attribute: false })
	get entityFlags(): Array<UmbEntityFlag> | undefined {
		return this.#entityFlagAliases?.map((x) => ({ alias: x }));
	}

	set entityFlags(value: Array<UmbEntityFlag> | undefined) {
		this.#entityFlagAliases = value?.map((x) => x.alias);
		this.#gotProperties();
	}

	@state()
	private _signs?: Array<any>;

	@state()
	private _labels: Map<string, string> = new Map();

	private _open = false;
	private _hoverTimer?: number;

	#signLabelObservations: Array<UmbObserverController<string>> = [];

	#manifestFilter = (manifest: ManifestEntitySign) => {
		if (manifest.forEntityTypes && !manifest.forEntityTypes.includes(this.#entityType!)) return false;
		if (manifest.forEntityFlags && !manifest.forEntityFlags.some((x) => this.#entityFlagAliases?.includes(x)))
			return false;
		return true;
	};

	#gotProperties() {
		if (!this.#entityType || !this.#entityFlagAliases) {
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
						const obs = this.observe(
							sign.api.label,
							(label) => {
								this._labels.set(sign.alias, label);
								this.requestUpdate('_labels');
							},
							'_observeSignLabelOf_' + sign.alias,
						);
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

	#openTooltip = () => {
		if (this._hoverTimer) clearTimeout(this._hoverTimer);
		this._hoverTimer = window.setTimeout(() => {
			this._open = true;
			this.requestUpdate();
			this._hoverTimer = undefined;
		}, 600);
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
				@mouseenter=${this.#openTooltip}
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
						return html`<div class="sign-container ${i === 0 ? 'first-icon' : ''}" style=${`--i:${i}`}>
							<span class="badge-icon">${c.component}</span><span class="label">${this._labels.get(c.alias)}</span>
						</div>`;
					},
				)
			: nothing;
	}

	static override styles = [
		css`
			:host {
				anchor-name: --entity-sign;
				position: relative;
				--row-h: 22px; /*TODO:Change to ems*/
				--icon-w: 12px;
				--pad-x: 4px; /*TODO:Change to ems*/
				--ease: cubic-bezier(0.2, 0.8, 0.2, 1);
			}

			.infobox {
				position: absolute;
				left: 100%;
				top: 0;
				background: transparent;
				padding: 0;
				font-size: 8px;
				clip-path: inset(0 calc(100% - (var(--icon-w) + 2 * var(--pad-x))) calc((var(--count) - 1) * var(--row-h)) 0);
				transition: clip-path 220ms var(--ease);
				will-change: clip-path;
				min-height: fit-content;
				z-index: 1;
			}

			.infobox > .sign-container {
				display: flex;
				align-items: start;
				gap: 3px;
				position: relative;
				z-index: 0;
				transform: translateY(calc(-1 * var(--i, 0) * var(--row-h)));
				transition: transform 220ms var(--ease);
				will-change: transform;
			}
			.infobox > .sign-container.first-icon {
				margin-left: 3px;
				z-index: 1;
			}
			.infobox .sign-container .badge-icon {
				background: var(--sign-bundle-bg, transparent);
				border-radius: 50%;
			}

			.infobox .sign-container .label {
				opacity: 0;
				transition: opacity 160ms ease;
			}

			/*OPEN STATE -- Prevent the hover state in firefox(until support of the position-anchor)*/
			@supports (position-anchor: --any-check) {
				.infobox.is-open {
					position: fixed;
					position-anchor: --entity-sign;
					top: anchor(bottom);
					left: anchor(right);
					z-index: 2;
					background: var(--uui-color-surface);
					font-size: 12px;
					border-radius: 3px;
					color: var(--uui-color-text);
					padding: 4px;
					box-shadow:
						0px 0px 15px 0px rgba(0, 0, 0, 0.1),
						0px 10px 15px -3px rgba(0, 0, 0, 0.1);
					clip-path: inset(0);
					--sign-bundle-bg: var(--uui-color-surface);
				}
				.infobox.is-open > .sign-container {
					transform: none;
					align-items: center;
				}
				.infobox.is-open > .sign-container.first-icon {
					margin-left: 0;
				}
				.infobox.is-open .sign-container .label {
					opacity: 1;
					pointer-events: auto;
				}
				.infobox.is-open .sign-container .badge-icon {
					background: transparent;
				}
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-entity-sign-bundle': UmbEntitySignBundleElement;
	}
}
