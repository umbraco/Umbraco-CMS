import { UmbLitElement } from '../../lit-element/lit-element.element.js';
import type { ManifestEntitySign } from '../types.js';
import { customElement, html, nothing, property, repeat, state, css } from '@umbraco-cms/backoffice/external/lit';
import { UmbExtensionsElementAndApiInitializer } from '@umbraco-cms/backoffice/extension-api';
import type { UmbObserverController } from '@umbraco-cms/backoffice/observable-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbEntityFlag } from '@umbraco-cms/backoffice/entity-flag';

@customElement('umb-entity-sign-bundle')
export class UmbEntitySignBundleElement extends UmbLitElement {
	#entityType?: string;
	#entityFlagAliases?: Array<string>;

	@property({ type: String, attribute: 'entity-type', reflect: false })
	get entityType(): string | undefined {
		return this.#entityType;
	}

	set entityType(value: string | undefined) {
		if (this.#entityType === value) return;
		this.#entityType = value;
		this.#gotProperties();
	}

	@property({ type: Array, attribute: false })
	get entityFlags(): Array<UmbEntityFlag> | undefined {
		return this.#entityFlagAliases?.map((x) => ({ alias: x }));
	}

	set entityFlags(value: Array<UmbEntityFlag> | undefined) {
		const entityFlagAliases = value?.map((x) => x.alias);
		// If they are equal return:
		if (this.#entityFlagAliases?.join(',') === entityFlagAliases?.join(',')) return;
		this.#entityFlagAliases = entityFlagAliases;
		this.#gotProperties();
	}

	@state()
	private _signs?: Array<any>;

	@state()
	private _labels: Map<string, string> = new Map();

	private _hoverTimer?: number;

	#signLabelObservations: Array<UmbObserverController<string>> = [];

	constructor() {
		super();
		this.addEventListener('mouseenter', this.#openTooltip);
		this.addEventListener('mouseleave', this.#cancelOpen);
	}

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

	#handleHoverTimer(open: boolean, delay: number) {
		if (this._hoverTimer) clearTimeout(this._hoverTimer);
		this._hoverTimer = window.setTimeout(() => {
			const popover = this.shadowRoot?.querySelector('#entity-sign-popover') as HTMLElement;
			if (popover) {
				if (open) {
					// 1) Get the host element's position
					const hostRect = this.getBoundingClientRect();
					// 2) Position the popover
					popover.style.top = `${hostRect.bottom}px`;
					popover.style.left = `${hostRect.right}px`;
					// 3) Show the popover
					popover.showPopover();
				} else {
					popover.hidePopover();
				}
			}
			this._hoverTimer = undefined;
		}, delay);
	}

	#openTooltip = () => {
		const popover = this.shadowRoot?.querySelector('#entity-sign-popover') as HTMLElement;
		if (popover && !popover.matches(':popover-open')) {
			this.#handleHoverTimer(true, 240);
		}
	};

	#cancelOpen = () => {
		const popover = this.shadowRoot?.querySelector('#entity-sign-popover') as HTMLElement;
		if (popover?.matches(':popover-open')) {
			this.#handleHoverTimer(false, 360);
		} else if (this._hoverTimer) {
			clearTimeout(this._hoverTimer);
			this._hoverTimer = undefined;
		}
	};

	override render() {
		return html`
			<slot></slot>
			${this.#renderBundle()}
		`;
	}
	#renderBundle() {
		if (!this._signs || this._signs.length === 0) return nothing;

		const first = this._signs?.[0];
		if (!first) return nothing;
		return html`<div id="entity-sign-popover" popover="hint" class="infobox" style=${`--count:${this._signs.length}`}>
			${this.#renderOptions()}
		</div>`;
	}

	#renderOptions() {
		return this._signs
			? repeat(
					this._signs,
					(c) => c.alias,
					(c, i) => {
						return html`<div class="sign-container ${i > 1 ? 'hide-in-overview' : ''}" style=${`--i:${i}`}>
							<span class="badge-icon">${c.component}</span
							><span class="label">${this.localize.string(this._labels.get(c.alias) ?? '')}</span>
						</div>`;
					},
				)
			: nothing;
	}

	static override styles = [
		css`
			:host {
				position: relative;
				--offset-h: 12px;
				--icon-w: 0.75rem;
				--pad-x: 0.25rem;
				--ease: cubic-bezier(0.1, 0, 0.3, 1);
				--ease-bounce: cubic-bezier(0.175, 0.885, 0.32, 1.275);
			}

			.infobox {
				position: absolute;
				background-color: transparent;
				padding: var(--uui-size-2) var(--uui-size-3);
				font-size: 8px;
				clip-path: inset(-10px calc(100% - 30px) calc(100% - 10px) -20px);
				transition:
					background-color 80ms 40ms linear,
					clip-path 120ms var(--ease-bounce),
					font-size 120ms var(--ease);
				min-height: fit-content;
				border: none;
				margin: 0;
			}

			.infobox > .sign-container {
				display: flex;
				align-items: start;
				gap: 3px;
				position: relative;
				transform: translate(calc((var(--i) * -5px) - 10px), calc((-1 * var(--i) * var(--row-h)) - var(--offset-h)));
				transition:
					transform 120ms var(--ease),
					visibility 0ms linear 120ms opacity 120ms linear;
				z-index: calc(var(--count) - var(--i));
				pointer-events: none;
			}
			.infobox > .sign-container.hide-in-overview {
				visibility: hidden;
			}

			.infobox .sign-container .label {
				opacity: 0;
				transition: opacity 120ms;
			}

			.infobox:popover-open {
				font-size: 12px;
				color: var(--uui-color-text);
				clip-path: inset(-12px);
				background-color: var(--uui-color-surface); /* Move from ::before */
				box-shadow: var(--uui-shadow-depth-2); /* Move from ::before */
				border-radius: 3px;
				--umb-sign-bundle-bg: var(--uui-color-surface);
			}

			.infobox:popover-open > .sign-container {
				transform: none;
				align-items: center;
				transition: transform 120ms var(--ease);
				visibility: visible;
			}
			.infobox:popover-open .sign-container .label {
				opacity: 1;
				pointer-events: auto;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-entity-sign-bundle': UmbEntitySignBundleElement;
	}
}
