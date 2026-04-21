import { css, query } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

/**
 * Abstract base element providing popover toggle tracking and dynamic scroll container
 * height calculation for dropdown popovers.
 * When the popover opens, it constrains the scroll container height to the available space
 * below the trigger button, preventing the dropdown from extending beyond the viewport.
 *
 * NOTE: This abstraction has a known dependency on the subclass shadow DOM structure —
 * it queries for `uui-button[popovertarget]` and `uui-scroll-container` elements.
 * A pure CSS solution (e.g. `max-height: 100dvh`) is not sufficient here because
 * `uui-popover-container` uses the Popover API, which renders in the top layer and is
 * not constrained by ancestor overflow or viewport-relative CSS on the container itself.
 * The height must therefore be set dynamically in JavaScript based on the trigger button's
 * position at the time the popover opens.
 *
 * TODO: Consider moving this to the UI Library once the pattern stabilises.
 */
export abstract class UmbPopoverScrollElement extends UmbLitElement {
	static override properties = {
		_popoverOpen: { state: true },
	};

	private __popoverOpen = false;

	protected get _popoverOpen(): boolean {
		return this.__popoverOpen;
	}

	protected set _popoverOpen(value: boolean) {
		const oldValue = this.__popoverOpen;
		this.__popoverOpen = value;
		if (value) {
			this.#updateScrollHeight();
		}
		this.requestUpdate('_popoverOpen', oldValue);
	}

	@query('uui-button[popovertarget]')
	protected _triggerButton?: HTMLElement;

	@query('uui-scroll-container')
	protected _scrollContainer?: HTMLElement;

	// Arrow function so `this` is always correctly bound when passed directly as an event listener.
	protected _onPopoverToggle = (event: ToggleEvent): void => {
		// TODO: This ignorer is just needed for JSON SCHEMA TO WORK, As its not updated with latest TS yet.
		// eslint-disable-next-line @typescript-eslint/ban-ts-comment
		// @ts-ignore
		this._popoverOpen = event.newState === 'open';
	};

	#updateScrollHeight() {
		if (!this._triggerButton || !this._scrollContainer) return;
		const rect = this._triggerButton.getBoundingClientRect();
		const availableHeight = window.innerHeight - rect.bottom - 8;
		this._scrollContainer.style.maxHeight = `${Math.max(availableHeight, 120)}px`;
	}

	static override styles = [
		css`
			uui-scroll-container {
				overflow-y: auto;
				overflow-x: hidden;
			}
		`,
	];
}

