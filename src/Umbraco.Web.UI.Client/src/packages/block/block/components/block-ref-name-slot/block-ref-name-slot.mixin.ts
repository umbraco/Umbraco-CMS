import type { UmbBlockLabelUfmValueType } from '../../types.js';
import { html, ifDefined, property, state, when } from '@umbraco-cms/backoffice/external/lit';
import { UmbDeprecation } from '@umbraco-cms/backoffice/utils';
import type { HTMLElementConstructor } from '@umbraco-cms/backoffice/extension-api';

export interface UmbBlockRefNameSlot {
	/** @deprecated Use the `name` slot to project a `<umb-ufm-render>` instead. Will be removed in Umbraco 20. */
	label: string | undefined;
}

/**
 * Type of the class returned by {@link UmbBlockRefNameSlotMixin}, exposing the protected `renderNameSlot`.
 */
export declare abstract class UmbBlockRefNameSlotElement extends HTMLElement implements UmbBlockRefNameSlot {
	/** @deprecated Use the `name` slot to project a `<umb-ufm-render>` instead. Will be removed in Umbraco 20. */
	label: string | undefined;
	protected _hasNameSlotContent: boolean;

	/**
	 * Renders the `name` slot, falling back to `label` (via `<umb-ufm-render>`) while nothing is slotted.
	 * @param {UmbBlockLabelUfmValueType} blockValue - the value object passed to the fallback `<umb-ufm-render>`.
	 * @param {string} [slotName] - a slot on a wrapped element (e.g. `uui-ref-node`) to forward `name` into.
	 * @returns {unknown} the slot markup.
	 */
	protected renderNameSlot(blockValue: UmbBlockLabelUfmValueType, slotName?: string): unknown;
}

/**
 * Mixin giving a block's ref/inline leaf component a `name` slot, with the deprecated `label` property
 * rendered as a fallback while nothing is slotted.
 * @mixin
 * @template T
 * @param {T} superClass - the class to mix the `label`/`name` slot behaviour into.
 * @returns {T} the mixed-in class.
 */
export const UmbBlockRefNameSlotMixin = <T extends HTMLElementConstructor>(superClass: T) => {
	class UmbBlockRefNameSlotMixinClass extends superClass implements UmbBlockRefNameSlot {
		static #hasWarnedLabelDeprecation = false;

		@property({ type: String, reflect: false })
		public set label(value: string | undefined) {
			if (value !== undefined && value !== this.#label) {
				if (!UmbBlockRefNameSlotMixinClass.#hasWarnedLabelDeprecation) {
					UmbBlockRefNameSlotMixinClass.#hasWarnedLabelDeprecation = true;
					new UmbDeprecation({
						deprecated: `${this.tagName.toLowerCase()}.label property`,
						solution: 'Project a `<umb-ufm-render>` into the `name` slot instead.',
						removeInVersion: '20.0.0',
					}).warn();
				}
			}
			this.#label = value;
		}
		public get label(): string | undefined {
			return this.#label;
		}
		#label?: string;

		@state()
		protected _hasNameSlotContent = false;

		#onNameSlotChange(event: Event) {
			const slot = event.target as HTMLSlotElement;
			this._hasNameSlotContent = slot.assignedNodes({ flatten: true }).length > 0;
		}

		protected renderNameSlot(blockValue: UmbBlockLabelUfmValueType, slotName?: string) {
			return html`
				<slot name="name" slot=${ifDefined(slotName)} @slotchange=${this.#onNameSlotChange}></slot>
				${when(
					!this._hasNameSlotContent && this.#label !== undefined,
					() => html`
						<umb-ufm-render
							id=${ifDefined(!slotName ? 'name' : undefined)}
							slot=${ifDefined(slotName)}
							inline
							.markdown=${this.#label}
							.value=${blockValue}>
						</umb-ufm-render>
					`,
				)}
			`;
		}
	}

	return UmbBlockRefNameSlotMixinClass as unknown as HTMLElementConstructor<UmbBlockRefNameSlotElement> & T;
};
