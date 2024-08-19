import {
	type PropertyValueMap,
	LitElement,
	css,
	customElement,
	html,
	property,
	query,
	state,
} from '@umbraco-cms/backoffice/external/lit';
import { clamp } from '@umbraco-cms/backoffice/utils';

/**
 * Custom element for a split panel with adjustable divider.
 * @element umb-split-panel
 * @slot start - Content for the start panel.
 * @slot end - Content for the end panel.
 * @cssprop --umb-split-panel-initial-position - Initial position of the divider.
 * @cssprop --umb-split-panel-start-min-width - Minimum width of the start panel.
 * @cssprop --umb-split-panel-end-min-width - Minimum width of the end panel.
 * @cssprop --umb-split-panel-divider-touch-area-width - Width of the divider touch area.
 * @cssprop --umb-split-panel-divider-width - Width of the divider.
 * @cssprop --umb-split-panel-divider-color - Color of the divider.
 */
@customElement('umb-split-panel')
export class UmbSplitPanelElement extends LitElement {
	@query('#main') mainElement!: HTMLElement;
	@query('#divider-touch-area') dividerTouchAreaElement!: HTMLElement;
	@query('#divider') dividerElement!: HTMLElement;

	/**
	 * Snap points for the divider position.
	 * Pixel or percent space-separated values: e.g., "100px 50% -75% -200px".
	 * Negative values are relative to the end of the container.
	 */
	@property({ type: String }) snap?: string; //TODO: Consider using css variables for snap points.

	/**
	 * Locking mode for the split panel.
	 * Possible values: "start", "end", "none" (default).
	 */
	@property({ type: String }) lock: 'start' | 'end' | 'none' = 'none';

	/**
	 * Initial position of the divider.
	 * Pixel or percent value: e.g., "100px" or "25%".
	 * Defaults to a CSS variable if not set: "var(--umb-split-panel-initial-position) which defaults to 50%".
	 */
	@property({ type: String, reflect: true }) position = 'var(--umb-split-panel-initial-position)';
	//TODO: Add support for negative values (relative to end of container) similar to snap points.

	/** Width of the locked panel when in "start" or "end" lock mode */
	#lockedPanelWidth: number = 0;
	/** Pixel value for the snap threshold. Determines how close the divider needs to be to a snap point to snap to it. */
	readonly #SNAP_THRESHOLD = 25 as const;

	@state() _hasStartPanel = false;
	@state() _hasEndPanel = false;
	get #hasBothPanels() {
		return this._hasStartPanel && this._hasEndPanel;
	}

	#hasInitialized = false;

	override disconnectedCallback() {
		super.disconnectedCallback();
		this.#disconnect();
	}

	protected override updated(_changedProperties: PropertyValueMap<any> | Map<PropertyKey, unknown>): void {
		super.updated(_changedProperties);

		if (!this.#hasInitialized) return;

		if (_changedProperties.has('position')) {
			if (this.lock !== 'none') {
				const { width } = this.mainElement.getBoundingClientRect();

				let pos = parseFloat(this.position);

				if (this.position.endsWith('%')) {
					pos = (pos / 100) * width;
				}

				const lockedPanelWidth = this.lock === 'start' ? pos : width - pos;
				this.#lockedPanelWidth = lockedPanelWidth;
			}

			this.#updateSplit();
		}
	}

	#setPosition(pos: number) {
		const { width } = this.mainElement.getBoundingClientRect();
		const localPos = clamp(pos, 0, width);
		const percentagePos = (localPos / width) * 100;
		this.position = percentagePos + '%';
	}

	#updateSplit() {
		// If lock is none
		let maxStartWidth = this.position;
		let maxEndWidth = '1fr';

		if (this.lock === 'start') {
			maxStartWidth = this.#lockedPanelWidth + 'px';
			maxEndWidth = `1fr`;
		}
		if (this.lock === 'end') {
			maxStartWidth = `1fr`;
			maxEndWidth = this.#lockedPanelWidth + 'px';
		}

		this.mainElement.style.gridTemplateColumns = `
      minmax(var(--umb-split-panel-start-min-width), ${maxStartWidth})
      0px
      minmax(var(--umb-split-panel-end-min-width), ${maxEndWidth})
    `;
	}

	#onDragStart = (event: PointerEvent | TouchEvent) => {
		event.preventDefault();

		const move = (event: PointerEvent) => {
			const { clientX } = event;
			const { left, width } = this.mainElement.getBoundingClientRect();
			const localPos = clamp(clientX - left, 0, width);
			const mappedPos = mapXAxisToSnap(localPos, width);

			this.#lockedPanelWidth = this.lock === 'start' ? mappedPos : width - mappedPos;
			this.#setPosition(mappedPos);
		};

		const stop = () => {
			document.removeEventListener('pointermove', move);
			document.removeEventListener('pointerup', stop);

			this.dispatchEvent(new CustomEvent('position-changed', { detail: { position: this.position } }));
		};

		const mapXAxisToSnap = (xPos: number, containerWidth: number) => {
			const snaps = this.snap?.split(' ');
			if (!snaps) return xPos;

			const snapsInPixels = snaps.map((snap) => {
				let snapPx = parseFloat(snap);

				if (snap.endsWith('%')) {
					snapPx = (snapPx / 100) * containerWidth;
				}

				if (snap.startsWith('-')) {
					snapPx = containerWidth + snapPx;
				}

				return snapPx;
			});

			const closestSnap = snapsInPixels.reduce((prev, curr) => {
				return Math.abs(curr - xPos) < Math.abs(prev - xPos) ? curr : prev;
			});

			if (closestSnap < xPos + this.#SNAP_THRESHOLD && closestSnap > xPos - this.#SNAP_THRESHOLD) {
				xPos = closestSnap;
			}

			return xPos;
		};

		document.addEventListener('pointermove', move, { passive: true });
		document.addEventListener('pointerup', stop);
	};

	#disconnect() {
		this.dividerTouchAreaElement.removeEventListener('pointerdown', this.#onDragStart);
		this.dividerTouchAreaElement.removeEventListener('touchstart', this.#onDragStart);
		this.dividerElement.style.display = 'none';
		this.mainElement.style.display = 'flex';
		this.#hasInitialized = false;
	}

	async #connect() {
		this.#hasInitialized = true;

		this.mainElement.style.display = 'grid';
		this.mainElement.style.gridTemplateColumns = `${this.position} 0px 1fr`;
		this.dividerElement.style.display = 'unset';

		this.dividerTouchAreaElement.addEventListener('pointerdown', this.#onDragStart);
		this.dividerTouchAreaElement.addEventListener('touchstart', this.#onDragStart, { passive: false });

		// Wait for the next frame to get the correct position of the divider.
		await new Promise((resolve) => requestAnimationFrame(resolve));

		const { left: dividerLeft } = this.shadowRoot!.querySelector('#divider')!.getBoundingClientRect();
		const { left: mainLeft, width: mainWidth } = this.mainElement.getBoundingClientRect();
		const percentagePos = ((dividerLeft - mainLeft) / mainWidth) * 100;
		this.position = `${percentagePos}%`;
	}

	#onSlotChanged(event: Event) {
		const slot = event.target as HTMLSlotElement;
		const name = slot.name;

		if (name === 'start') {
			this._hasStartPanel = slot.assignedElements().length > 0;
		}
		if (name === 'end') {
			this._hasEndPanel = slot.assignedElements().length > 0;
		}

		if (!this.#hasBothPanels) {
			if (this.#hasInitialized) {
				this.#disconnect();
			}
			return;
		}

		this.#connect();
	}

	#onKeydown(event: KeyboardEvent) {
		if (event.key === 'ArrowLeft' || event.key === 'ArrowRight') {
			const { width } = this.mainElement.getBoundingClientRect();
			const divider = this.dividerElement.getBoundingClientRect();

			const dividerPos = divider.left - this.mainElement.getBoundingClientRect().left;
			const percentageFromWidth = (dividerPos / width) * 100;

			const multiplier = event.shiftKey ? 10 : 1;
			const step = 1 * multiplier * (event.key === 'ArrowLeft' ? -1 : 1);

			const newPercent = percentageFromWidth + step;
			const toPixels = (newPercent / 100) * this.mainElement.getBoundingClientRect().width;

			this.#setPosition(toPixels);
		}

		if (event.key === 'Home' || event.key === 'End') {
			event.preventDefault();
			const { width } = this.mainElement.getBoundingClientRect();
			const newPos = event.key === 'Home' ? 0 : width;
			this.#setPosition(newPos);
		}
	}

	override render() {
		return html`
			<div id="main">
				<slot
					name="start"
					@slotchange=${this.#onSlotChanged}
					style="width: ${this._hasStartPanel ? '100%' : '0'}"></slot>
				<div id="divider">
					<div id="divider-touch-area" tabindex="0" @keydown=${this.#onKeydown}></div>
				</div>
				<slot name="end" @slotchange=${this.#onSlotChanged} style="width: ${this._hasEndPanel ? '100%' : '0'}"></slot>
			</div>
		`;
	}
	static override styles = css`
		:host {
			display: contents;
			--umb-split-panel-initial-position: 50%;
			--umb-split-panel-start-min-width: 0;
			--umb-split-panel-end-min-width: 0;
			--umb-split-panel-divider-touch-area-width: 20px;
			--umb-split-panel-divider-width: 1px;
			--umb-split-panel-divider-color: transparent;
			--umb-split-panel-slot-overflow: hidden;
		}
		slot {
			overflow: var(--umb-split-panel-slot-overflow);
			display: block;
			min-height: 0;
		}
		#main {
			width: 100%;
			height: 100%;
			display: flex;
			position: relative;
			z-index: 0;
			overflow: hidden;
		}
		#divider {
			height: 100%;
			position: relative;
			z-index: 999999;
			display: none;
		}
		#divider-touch-area {
			position: absolute;
			top: 0;
			left: 5px;
			height: 100%;
			width: var(--umb-split-panel-divider-touch-area-width);
			transform: translateX(-50%);
			cursor: col-resize;
		}
		/* Do we want a line that shows the divider? */
		#divider::after {
			content: '';
			position: absolute;
			top: 0;
			left: 50%;
			width: var(--umb-split-panel-divider-width);
			height: 100%;
			transform: round(translateX(-50%));
			background-color: var(--umb-split-panel-divider-color);
			z-index: -1;
		}
	`;
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-split-panel': UmbSplitPanelElement;
	}
}
