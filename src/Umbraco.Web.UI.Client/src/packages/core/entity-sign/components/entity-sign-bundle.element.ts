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

	private _hoverTimer?: number;
	private _closeTimer?: number;
	private _overPanel = false;

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

	#animateOpen() {
		const panel = this.renderRoot.querySelector<HTMLElement>('.panel');
		if (!panel) return;
		panel.getAnimations().forEach((a) => a.cancel());
		panel.animate(
			[
				{ opacity: 0, transform: 'translateY(-10px)' },
				{ opacity: 1, transform: 'translateY(0)' },
			],
			{ duration: 300, easing: 'ease', fill: 'forwards' },
		);
	}

	#openDropdown = () => {
		console.log('hover timer', this._hoverTimer);
		if (this._closeTimer) {
			clearTimeout(this._closeTimer);
			this._closeTimer = undefined;
		}
		if (this._hoverTimer) {
			clearTimeout(this._hoverTimer);
		}
		this._hoverTimer = window.setTimeout(() => {
			const pop = this.renderRoot.querySelector<HTMLElement>('#popover');
			if (!pop || pop.matches(':popover-open')) return;
			(pop as any).showPopover?.();
			this.#animateOpen();
			this._hoverTimer = undefined;
		}, 1000);
	};

	#closeDropdown = () => {
		if (this._hoverTimer) {
			clearTimeout(this._hoverTimer);
			this._hoverTimer = undefined;
		}

		if (this._closeTimer) {
			clearTimeout(this._closeTimer);
		}

		this._closeTimer = window.setTimeout(() => {
			if (this._overPanel) return;
			const pop = this.renderRoot.querySelector<HTMLElement>('#popover');
			if (pop && pop.matches(':popover-open')) {
				(pop as any).hidePopover?.();
			}
			this._closeTimer = undefined;
		}, 120);
	};

	#onPanelEnter = () => {
		this._overPanel = true;
	};
	#onPanelLeave = () => {
		this._overPanel = false;
		this.#closeDropdown();
	};

	override render() {
		if (!this._signs || this._signs.length === 0) return nothing;

		const first = this._signs?.[0];
		console.log(first);
		if (!first) return nothing;

		return html`
			<button
				id="sign-icon"
				popovertarget="popover"
				type="button"
				@mouseenter=${this.#openDropdown}
				@mouseleave=${this.#closeDropdown}>
				<umb-icon name="icon-info"></umb-icon>
			</button>
			<uui-popover-container
				id="popover"
				popover
				placement="bottom-start"
				@mouseenter=${this.#onPanelEnter}
				@mouseleave=${this.#onPanelLeave}>
				<umb-popover-layout class="panel">
					<div class="labels-pop">${this.#renderOptions()}</div>
				</umb-popover-layout>
			</uui-popover-container>
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
			#sign-icon {
				width: 14px;
				display: flex;
				align-items: center;
				justify-content: center;
				height: 14px;
				font-size: 10px;
				border-radius: 50%;
				cursor: pointer;
				border: none;
			}

			umb-icon {
				font-size: 10px;
			}
			.labels-pop {
				padding: 5px;
			}
			.label {
				display: flex;
				align-items: center;
				gap: 5px;
			}

			.panel {
				opacity: 0;
				transform: translateY(-10px);
				transition:
					opacity 300ms ease,
					transform 300ms ease;
				will-change: opacity, transform;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-entity-sign-bundle': UmbEntitySignBundleElement;
	}
}
