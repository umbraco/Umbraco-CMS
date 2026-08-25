import { defineElement } from '@umbraco-ui/uui';
import { property, query, css, html, LitElement } from '../lit';
import { UUIPopoverEvent } from './UUIPopoverEvent.js';

export type PopoverPlacement =
	| 'auto'
	| 'top'
	| 'top-start'
	| 'top-end'
	| 'bottom'
	| 'bottom-start'
	| 'bottom-end'
	| 'right'
	| 'right-start'
	| 'right-end'
	| 'left'
	| 'left-start'
	| 'left-end';

// ------------------- Math extensions ---------------------
function mathClamp(value: number, min: number, max: number) {
	if (value < min) {
		return min;
	} else if (value > max) {
		return max;
	}
	return value;
}

/**
 * @deprecated This component has been deprecated and will be removed in future releases. It is being replaced by popover-container.
 * @element uui-popover
 * @description Open a modal aligned with the opening element. This does not jet work within two layers of scroll containers.
 * @fires close - When popover is closed by user interaction.
 * @slot trigger - The element that triggers the popover.
 * @slot popover - The content of the popover.
 * @cssprop --uui-popover-z-index - overwrite the z-index of the popover container.
 */
@defineElement('uui-popover')
export class UUIPopoverElement extends LitElement {
	// Cashed non-state variables //////////////////////////////
	private intersectionObserver?: IntersectionObserver;
	private scrollEventHandler = this._updatePlacement.bind(this);
	////////////////////////////////////////////////////////////

	@query('#container') private containerElement!: HTMLElement;

	private _open = false;
	private _placement: PopoverPlacement = 'bottom-start';
	private _currentPlacement: PopoverPlacement | null = null;
	private _trigger?: Element;
	private _scrollParents: Element[] = [];
	private _positionX?: number;
	private _positionY?: number;

	/**
	 * Set the distance between popover-modal and trigger.
	 * @type {number}
	 * @attr margin
	 * @default 0
	 */
	@property({ type: Number })
	margin = 0;

	/**
	 * Define the placement of the popover-modal.
	 * @attr placement
	 * @default 'bottom-start'
	 */
	@property({ type: String, reflect: true })
	get placement(): PopoverPlacement {
		return this._placement;
	}
	set placement(newValue: PopoverPlacement) {
		const oldValue = this._placement;
		this._placement = newValue || 'bottom-start';
		this._currentPlacement = null;
		this._updatePlacement();
		this.requestUpdate('placement', oldValue);
	}

	/**
	 * Opens the popover-modal.
	 * @type {boolean}
	 * @attr open
	 * @default false
	 */
	@property({ type: Boolean, reflect: true })
	get open() {
		return this._open;
	}
	set open(newValue: boolean) {
		const oldValue = this._open;
		this._open = newValue;
		if (newValue) {
			this._openPopover();
		} else {
			this._closePopover();
		}
		this.requestUpdate('open', oldValue);
	}

	override connectedCallback(): void {
		super.connectedCallback();
		this._getScrollParents();
	}

	override disconnectedCallback() {
		super.disconnectedCallback();
		document.removeEventListener('mousedown', this._onDocumentClick);
		document.removeEventListener('keyup', this._onKeyUp);
		document.removeEventListener('scroll', this.scrollEventHandler);

		if (this.intersectionObserver) {
			this.intersectionObserver.disconnect();
			delete this.intersectionObserver;
		}

		this._scrollParents = [];
	}

	private _onTriggerSlotChanged = (event: any) => {
		this._trigger = event.target.assignedElements({
			flatten: true,
		})[0] as HTMLElement;
	};

	private _openPopover() {
		if (this.containerElement) {
			this.containerElement!.style.opacity = '0'; // Hide while measuring popover size.
			document.addEventListener('mousedown', this._onDocumentClick);
			document.addEventListener('keyup', this._onKeyUp);
			this._currentPlacement = null;

			requestAnimationFrame(() => {
				this._updatePlacement();
				this._createIntersectionObserver();
				this.containerElement!.style.opacity = '1';
			});
		}
	}

	private _closePopover() {
		if (this.intersectionObserver) {
			this.intersectionObserver.disconnect();
			delete this.intersectionObserver;
		}
		document.removeEventListener('mousedown', this._onDocumentClick);
		document.removeEventListener('keyup', this._onKeyUp);
		this._currentPlacement = null;
	}

	// Use this when changing the open state from within this component.
	private _triggerPopoverClose() {
		const event = new UUIPopoverEvent(UUIPopoverEvent.CLOSE, {
			cancelable: true,
		});
		this.dispatchEvent(event);

		if (event.defaultPrevented) return;

		this.open = false;
	}

	private _getScrollParents(): any {
		// Clear previous scroll parents to avoid duplicates
		this._scrollParents = [];

		const hostElement = this.shadowRoot!.host;
		let style = getComputedStyle(hostElement);
		if (style.position === 'fixed') {
			return;
		}

		const includeHidden = false;
		const excludeStaticParent = style.position === 'absolute';
		const overflowRegex = includeHidden ? /(auto|scroll|hidden)/ : /(auto|scroll)/;

		let el: Element | null = hostElement;
		while (el) {
			style = getComputedStyle(el);

			if (excludeStaticParent && style.position === 'static') {
				el = this._getAncestorElement(el);
				continue;
			}
			if (overflowRegex.test(style.overflow + style.overflowY + style.overflowX)) {
				this._scrollParents.push(el);
			}
			if (style.position === 'fixed') {
				return;
			}

			el = this._getAncestorElement(el);
		}
		this._scrollParents.push(document.body);
	}

	private _getAncestorElement(el: Element | null): Element | null {
		if (el?.parentElement) {
			return el.parentElement;
		} else {
			// If we had no parentElement, then check for shadow roots:
			return (el?.getRootNode() as any)?.host;
		}
	}

	private _createIntersectionObserver() {
		if (this.intersectionObserver) {
			// break out, as we already have it
			return;
		}

		const options = {
			root: null,
			rootMargin: '0px',
			threshold: 1,
		};

		this.intersectionObserver = new IntersectionObserver(this._intersectionCallback, options);
		this.intersectionObserver.observe(this.containerElement as Element);
	}

	private _intersectionCallback = (entries: IntersectionObserverEntry[]) => {
		entries.forEach((entry) => {
			if (entry.isIntersecting === false) {
				this._startScrollListener();
				this._updatePlacement();
			}
		});
	};

	private _startScrollListener() {
		this._scrollParents.forEach((el) => {
			el.addEventListener('scroll', this.scrollEventHandler);
		});
		document.addEventListener('scroll', this.scrollEventHandler);
	}
	private _stopScrollListener() {
		this._scrollParents.forEach((el) => {
			el.removeEventListener('scroll', this.scrollEventHandler);
		});
		document.removeEventListener('scroll', this.scrollEventHandler);
	}

	private _onKeyUp = (event: KeyboardEvent) => {
		if (event.key === 'Escape') {
			this._triggerPopoverClose();
			return;
		}
	};

	private _onDocumentClick = (event: Event) => {
		if (!event.composedPath().includes(this)) {
			this._triggerPopoverClose();
		}
	};

	private _updatePlacement() {
		if (!this.shadowRoot) {
			return;
		}

		const containerElement = this.containerElement;
		if (!containerElement) {
			return;
		}

		const popoverRect = this.containerElement?.getBoundingClientRect();
		const triggerRect = this._trigger?.getBoundingClientRect();

		if (triggerRect != null && popoverRect != null) {
			const scrollParentRects = this._scrollParents.map((el) => el.getBoundingClientRect());

			this._currentPlacement = this._currentPlacement || this._placement;

			if (this._placement !== 'auto') {
				this._currentPlacement = this._managePlacementFlip(this._currentPlacement, popoverRect, scrollParentRects);
			}

			let isTopPlacement = this._currentPlacement.indexOf('top') !== -1;
			let isBottomPlacement = this._currentPlacement.indexOf('bottom') !== -1;
			let isLeftPlacement = this._currentPlacement.indexOf('left') !== -1;
			let isRightPlacement = this._currentPlacement.indexOf('right') !== -1;

			const isStart = this._currentPlacement.indexOf('-start') !== -1;
			const isEnd = this._currentPlacement.indexOf('-end') !== -1;

			// -------- | INITIATE MATH | --------
			let originX = 0.5;
			let originY = 0.5;
			let alignX = 0.5;
			let alignY = 0.5;
			let marginX = 0;
			let marginY = 0;

			if (this.placement === 'auto') {
				const firstScrollParent = this._scrollParents[0];
				const scrollParentWidth = firstScrollParent.clientWidth;
				const scrollParentHeight = firstScrollParent.clientHeight;

				const spaceLeft = triggerRect.x - popoverRect.width;
				const spaceRight = scrollParentWidth - (triggerRect.x + triggerRect.width) - popoverRect.width;
				const spaceTop = triggerRect.y - popoverRect.height;
				const spaceBottom = scrollParentHeight - (triggerRect.y + triggerRect.height) - popoverRect.height;

				let dirX = 0.5;
				let dirY = 0.5;

				const hMaxSpace = Math.max(spaceLeft, spaceRight);
				let vMaxSpace = Math.max(spaceTop, spaceBottom);

				// if we have more space below than above, and there is enough room, then we will pick below.
				if (spaceBottom > spaceTop && spaceBottom > this.margin) {
					vMaxSpace += 9999;
				}

				if (hMaxSpace > vMaxSpace) {
					if (spaceLeft > spaceRight) {
						dirX = 0;
						isLeftPlacement = true;
					} else {
						dirX = 1;
						isRightPlacement = true;
					}
					marginX = this.margin;
				} else {
					if (spaceTop > spaceBottom) {
						dirY = 0;
						isTopPlacement = true;
					} else {
						dirY = 1;
						isBottomPlacement = true;
					}
					marginY = this.margin;
				}

				originX = dirX;
				originY = dirY;
				alignX = 1 - dirX;
				alignY = 1 - dirY;
			} else {
				// -------- TOP / BOT --------
				if (isTopPlacement) {
					alignY = 1;
					originY = 0;
				}

				if (isBottomPlacement) {
					alignY = 0;
					originY = 1;
				}

				if (isTopPlacement || isBottomPlacement) {
					marginY = this.margin;

					if (isStart) {
						alignX = 0;
						originX = 0;
					}
					if (isEnd) {
						alignX = 1;
						originX = 1;
					}
				}

				// -------- LEFT / RIGHT --------
				if (isLeftPlacement) {
					alignX = 1;
					originX = 0;
				}
				if (isRightPlacement) {
					alignX = 0;
					originX = 1;
				}

				if (isLeftPlacement || isRightPlacement) {
					marginX = this.margin;

					if (isStart) {
						alignY = 0;
						originY = 0;
					}
					if (isEnd) {
						alignY = 1;
						originY = 1;
					}
				}
			}

			const calcX = -popoverRect.width * alignX + triggerRect.width * originX - marginX * (alignX * 2 - 1);
			const calcY = -popoverRect.height * alignY + triggerRect.height * originY - marginY * (alignY * 2 - 1);

			let posX = calcX;
			let posY = calcY;

			// Moves the popover to keep it fully visible on the screen as long as its still in contact with the trigger.

			// Only do this clamp if popover is on the top or bottom of the parent.
			if (isTopPlacement || isBottomPlacement) {
				this._scrollParents.forEach((el, index) => {
					const rectX = el === document.body ? 0 : scrollParentRects[index].x;
					const leftClamp = -triggerRect.x + rectX;
					const rightClamp =
						el.clientWidth -
						triggerRect.x -
						triggerRect.width +
						calcX +
						rectX -
						(popoverRect.width - triggerRect.width) * (1 - originX);
					posX = mathClamp(posX, leftClamp, rightClamp);
				});

				// keep within contact of the trigger, must be last:
				posX = mathClamp(posX, -popoverRect.width, triggerRect.width);
			} else if (isLeftPlacement || isRightPlacement) {
				// Only do this clamp if popover is on the sides of the parent.

				this._scrollParents.forEach((el, index) => {
					const rectY = el === document.body ? 0 : scrollParentRects[index].y;
					const topClamp = -triggerRect.y + rectY;
					const bottomClamp =
						el.clientHeight -
						triggerRect.y -
						triggerRect.height +
						calcY +
						rectY -
						(popoverRect.height - triggerRect.height) * (1 - originY);
					posY = mathClamp(posY, topClamp, bottomClamp);
				});

				// keep within contact of the trigger, must be last:
				posY = mathClamp(posY, -popoverRect.height, triggerRect.height);
			}

			if (this._positionX !== posX || this._positionY !== posY) {
				this._positionX = posX;
				this._positionY = posY;

				if (calcX === posX && calcY === posY) {
					// Not offset anymore, so we can stop listening for scroll events:
					this._stopScrollListener();
				}

				containerElement.style.left = `${this._positionX}px`;
				containerElement.style.top = `${this._positionY}px`;
			}
		}
	}

	private _managePlacementFlip(
		currentPlacement: PopoverPlacement,
		popoverRect: DOMRect,
		scrollParentRects: DOMRect[],
	): PopoverPlacement {
		const buffer = 2; // add this to the calculation make sure that the position checks are not off by e.g: 0.1 pixel.

		const placementSplit = currentPlacement.split('-');
		const currentSide = placementSplit[0];
		const placementAlign: string | null = placementSplit[1] || null;

		let sideFlip;
		this._scrollParents.some((el, index) => {
			const rectX = el === document.body ? 0 : scrollParentRects[index].x;
			const rectY = el === document.body ? 0 : scrollParentRects[index].y;

			if (currentSide === 'top' && popoverRect.y - buffer <= rectY) {
				sideFlip = 'bottom';
				return true;
			}
			if (currentSide === 'bottom' && popoverRect.y + popoverRect.height + buffer >= el.clientHeight + rectY) {
				sideFlip = 'top';
				return true;
			}
			if (currentSide === 'left' && popoverRect.x - buffer <= rectX) {
				sideFlip = 'right';
				return true;
			}
			if (currentSide === 'right' && popoverRect.x + popoverRect.width + buffer >= el.clientWidth + rectX) {
				sideFlip = 'left';
				return true;
			}

			return false;
		});

		if (sideFlip) {
			this._startScrollListener();
			return (sideFlip + (placementAlign ? `-${placementAlign}` : '')) as PopoverPlacement;
		}
		return currentPlacement;
	}

	override render() {
		return html`
			<slot id="trigger" name="trigger" @slotchange=${this._onTriggerSlotChanged}></slot>
			<div id="container">${this._open ? html`<slot name="popover"></slot>` : ''}</div>
		`;
	}

	static override styles = [
		css`
			:host {
				position: relative;
				display: inline-block;
				width: 100%;
			}
			#container {
				position: absolute;
				width: 100%;
				z-index: var(--uui-popover-z-index, 1);
			}
			slot[name='popover'] {
				display: block;
			}
			#trigger {
				position: relative;
				width: 100%;
			}

			slot[name='trigger']::slotted(uui-button) {
				--uui-button-border-radius: var(--uui-popover-toggle-slot-button-border-radius);
				--uui-button-merge-border-left: var(--uui-popover-toggle-slot-button-merge-border-left);
			}
		`,
	];
}
