import { css, query, state } from '@umbraco-cms/backoffice/external/lit';
import type { LitElement } from '@umbraco-cms/backoffice/external/lit';
import type { HTMLElementConstructor } from '@umbraco-cms/backoffice/extension-api';

export declare class UmbCollectionCreatePopoverScrollMixinInterface {
	protected _popoverOpen: boolean;
	protected _triggerButton: HTMLElement | undefined;
	protected _scrollContainer: HTMLElement | undefined;
	protected _onPopoverToggle(event: ToggleEvent): void;
}

/**
 * @mixin
 * Provides popover toggle tracking and dynamic scroll container height calculation for collection create action dropdowns.
 * When the popover opens, it constrains the scroll container height to the available space below the trigger button,
 * preventing the dropdown from flipping above the button due to viewport overflow.
 * @param {object} superClass - superclass to be extended.
 * @returns {Function} - The mixin class.
 */
export const UmbCollectionCreatePopoverScrollMixin = <T extends HTMLElementConstructor<LitElement>>(
	superClass: T,
) => {
	class UmbCollectionCreatePopoverScrollMixinClass extends superClass {
		@state()
		protected _popoverOpen = false;

		@query('uui-button[popovertarget]')
		protected _triggerButton?: HTMLElement;

		@query('uui-scroll-container')
		protected _scrollContainer?: HTMLElement;

		protected _onPopoverToggle(event: ToggleEvent) {
			// TODO: This ignorer is just needed for JSON SCHEMA TO WORK, As its not updated with latest TS yet.
			// eslint-disable-next-line @typescript-eslint/ban-ts-comment
			// @ts-ignore
			this._popoverOpen = event.newState === 'open';

			if (this._popoverOpen && this._triggerButton && this._scrollContainer) {
				const rect = this._triggerButton.getBoundingClientRect();
				const availableHeight = window.innerHeight - rect.bottom - 8;
				this._scrollContainer.style.maxHeight = `${Math.max(availableHeight, 120)}px`;
			}
		}

		static styles = [
			// eslint-disable-next-line @typescript-eslint/no-explicit-any
			...((superClass as any).styles ?? []),
			css`
				uui-scroll-container {
					overflow-y: auto;
					overflow-x: hidden;
				}
			`,
		];
	}

	return UmbCollectionCreatePopoverScrollMixinClass as unknown as HTMLElementConstructor<UmbCollectionCreatePopoverScrollMixinInterface> &
		T;
};

