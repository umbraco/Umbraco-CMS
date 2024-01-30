import {
	type PropertyValueMap,
	LitElement,
	css,
	customElement,
	html,
	property,
	query,
} from '@umbraco-cms/backoffice/external/lit';

/**
 * Custom element for a split panel with adjustable divider.
 * @element umb-split-panel
 * @slot start - Content for the start panel.
 * @slot end - Content for the end panel.
 * @cssprop --umb-split-panel-start-position - Initial position of the divider.
 * @cssprop --umb-split-panel-start-min-width - Minimum width of the start panel.
 * @cssprop --umb-split-panel-end-min-width - Minimum width of the end panel.
 */
@customElement('umb-split-panel')
export class UmbSplitPanelElement extends LitElement {
	@query('#main') mainElement!: HTMLElement;

	/**
	 * Snap points for the divider position.
	 * Pixel or percent space-separated values: e.g., "100px 50% -75% -200px".
	 * Negative values are relative to the end of the container.
	 */
	@property({ type: String }) snap?: string;

	/**
	 * Locking mode for the split panel.
	 * Possible values: "start", "end", "none" (default).
	 */
	@property({ type: String }) lock: 'start' | 'end' | 'none' = 'none';

	/**
	 * Initial position of the divider.
	 * Pixel or percent value: e.g., "100px" or "25%".
	 * Defaults to a CSS variable if not set: "var(--umb-split-panel-start-position, 50%)".
	 */
	@property({ type: String, reflect: true }) position = 'var(--umb-split-panel-start-position, 50%)';
	//TODO: Add support for negative values (relative to end of container) similar to snap points.

	/** Width of the locked panel when in "start" or "end" lock mode */
	#lockedPanelWidth: number = 0;
	/** Resize observer for tracking container size changes. */
	#resizeObserver?: ResizeObserver;
	/** Pixel value for the snap threshold. Determines how close the divider needs to be to a snap point to snap to it. */
	readonly #SNAP_THRESHOLD = 25 as const;
	/** Pixel value for the divider width. */
	readonly #DIVIDER_WIDTH = 20 as const;

	connectedCallback() {
		super.connectedCallback();
		this.#resizeObserver = new ResizeObserver(this.#onResize.bind(this));
		this.updateComplete.then(async () => {
			this.mainElement.style.gridTemplateColumns = `${this.position} 0px 1fr`;

			// Wait for the next frame to get the correct position of the divider.
			await new Promise((resolve) => requestAnimationFrame(resolve));

			const { left: dividerLeft } = this.shadowRoot!.querySelector('#divider')!.getBoundingClientRect();
			const { left: mainLeft, width: mainWidth } = this.mainElement.getBoundingClientRect();
			const percentagePos = ((dividerLeft - mainLeft) / mainWidth) * 100;
			this.position = `${percentagePos}%`;

			this.#resizeObserver?.observe(this);
		});
	}

	disconnectedCallback() {
		super.disconnectedCallback();
		this.#resizeObserver?.unobserve(this);
	}

	protected updated(_changedProperties: PropertyValueMap<any> | Map<PropertyKey, unknown>): void {
		super.updated(_changedProperties);

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

	#clamp(value: number, min: number, max: number) {
		return Math.min(Math.max(value, min), max);
	}

	#onResize(entries: ResizeObserverEntry[]) {
		const mainContainerWidth = entries[0].contentRect.width;

		if (this.lock === 'start') {
			this.#setPosition(this.#lockedPanelWidth);
		}
		if (this.lock === 'end') {
			this.#setPosition(mainContainerWidth - this.#lockedPanelWidth);
		}
	}

	#setPosition(pos: number) {
		const { width } = this.mainElement.getBoundingClientRect();
		const localPos = this.#clamp(pos, 0, width);
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
      minmax(var(--umb-split-panel-start-min-width, 0), ${maxStartWidth})
      0px
      minmax(var(--umb-split-panel-end-min-width, 0), ${maxEndWidth})
    `;
	}

	#onDragStart(event: PointerEvent) {
		event.preventDefault();

		const move = (event: PointerEvent) => {
			const { clientX } = event;
			const { left, width } = this.mainElement.getBoundingClientRect();
			const localPos = this.#clamp(clientX - left, 0, width);
			const mappedPos = mapXAxisToSnap(localPos, width);

			this.#lockedPanelWidth = this.lock === 'start' ? mappedPos : width - mappedPos;
			this.#setPosition(mappedPos);
		};

		function stop() {
			document.removeEventListener('pointermove', move);
			document.removeEventListener('pointerup', stop);
		}

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
	}

	render() {
		return html`
			<div id="main">
				<slot name="start"></slot>
				<div id="divider">
					<div
						id="divider-touch-area"
						tabindex="0"
						style="width: ${this.#DIVIDER_WIDTH}px; transform: translateX(-${this.#DIVIDER_WIDTH / 2}px)"
						@mousedown=${this.#onDragStart}
						@touchstart=${this.#onDragStart}></div>
				</div>
				<slot name="end"></slot>
			</div>
		`;
	}
	static styles = css`
		:host {
			display: contents;
		}
		slot {
			overflow: hidden;
		}
		#main {
			width: 100%;
			height: 100%;
			display: grid;
			position: relative;
			z-index: 0;
		}
		#divider {
			height: 100%;
			position: relative;
			z-index: 999999;
		}
		#divider-touch-area {
			position: absolute;
			top: 0;
			left: 0;
			height: 100%;
			transform: translateX(-50%);
			cursor: col-resize;
		}
		/* Do we want a line that shows the divider? */
		/* #divider::after {
			content: '';
			position: absolute;
			top: 0;
			left: 50%;
			width: 2px;
			height: 100%;
			transform: translateX(-50%);
			background-color: black;
			z-index: -1;
		} */
	`;
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-split-panel': UmbSplitPanelElement;
	}
}
