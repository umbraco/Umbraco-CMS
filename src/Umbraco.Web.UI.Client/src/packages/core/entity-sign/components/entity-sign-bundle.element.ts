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
	@state()
	private _popoverOpen = false;

	private _hoverTimer?: number;

	#signLabelObservations: Array<UmbObserverController<string>> = [];
	#previewElements: Map<string, HTMLElement> = new Map();

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
				this.#previewElements.clear();

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
					this._popoverOpen = true;
				} else {
					popover.hidePopover();
					this._popoverOpen = false;
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

	#getOrCreatePreviewElement(sign: any): HTMLElement | undefined {
		if (!sign.manifest) return undefined;

		const existing = this.#previewElements.get(sign.alias);
		if (existing) {
			return existing;
		}

		const elementName = sign.manifest.elementName ?? 'umb-entity-sign-icon';
		const el = document.createElement(elementName);
		(el as any).manifest = sign.manifest;
		this.#previewElements.set(sign.alias, el);
		return el;
	}

	override render() {
		return html`
			<slot></slot>
			${this.#renderBundle()}
		`;
	}
	#renderBundle() {
		if (!this._signs || this._signs.length === 0) return nothing;

		return html` ${this.#renderPreview()} ${this.#renderPopover()} `;
	}

	#renderPreview() {
		if (!this._signs || this._signs.length === 0) return nothing;

		const previewSigns = this._signs.slice(0, 2);

		return html`<div class="preview ${this._popoverOpen ? 'hidden' : ''}">
			${repeat(
				previewSigns,
				(c) => c.alias,
				(c, i) => html`<span class="preview-icon" style=${`--i:${i}`}>${this.#getOrCreatePreviewElement(c)}</span>`,
			)}
		</div>`;
	}

	#renderPopover() {
		return html`<div id="entity-sign-popover" popover="hint" class="infobox">${this.#renderOptions()}</div>`;
	}

	#renderOptions() {
		return this._signs
			? repeat(
					this._signs,
					(c) => c.alias,
					(c) => {
						return html`<div class="sign-container">
							<span class="badge-icon">${c.component}</span>
							<span class="label">${this.localize.string(this._labels.get(c.alias) ?? '')}</span>
						</div>`;
					},
				)
			: nothing;
	}

	static override styles = [
		css`
			:host {
				position: relative;
				--ease: cubic-bezier(0.1, 0, 0.3, 1);
				--ease-bounce: cubic-bezier(0.175, 0.885, 0.32, 1.275);
			}

			.infobox {
				position: absolute;
				padding: var(--uui-size-2) var(--uui-size-3);
				font-size: 12px;
				color: var(--uui-color-text);
				background-color: var(--uui-color-surface);
				box-shadow: var(--uui-shadow-depth-2);
				border-radius: 3px;
				border: none;
				margin: 0;
				--umb-sign-bundle-bg: var(--uui-color-surface);
			}

			.infobox > .sign-container {
				display: flex;
				align-items: center;
				gap: 3px;
				position: relative;
				pointer-events: none;
			}

			.preview {
				pointer-events: none;
				transition: opacity 120ms var(--ease);
			}

			.preview.hidden {
				opacity: 0;
			}

			.preview-icon {
				position: absolute;
				top: 10px;
				left: 12px;
				transform: translateX(calc(var(--i) * -5px));
				font-size: 8px;
				z-index: calc(2 - var(--i));
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-entity-sign-bundle': UmbEntitySignBundleElement;
	}
}
