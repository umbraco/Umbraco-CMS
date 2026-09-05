import '@umbraco-ui/uui';
import { UUITimer } from '@umbraco-ui/uui';
import type {
	UUIActiveMixinInterface,
	UUILabelMixinInterface,
	UUIPopoverTargetMixinInterface,
	UUISelectableMixinInterface,
	UUISelectOnlyMixinInterface,
} from '@umbraco-ui/uui';
export * from '@umbraco-ui/uui';

// UUI 2.0 removed the global ambient `Option` interface (its replacement is the exported
// `UUISelectOption`). Re-declare it here so existing backoffice code — and third-party
// extensions that referenced the ambient `Option` type — continue to compile unchanged.
// TODO (V19): migrate consumers to `UUISelectOption` and remove this compatibility shim.
declare global {
	interface Option {
		name: string;
		value: string;
		group?: string;
		selected?: boolean;
		disabled?: boolean;
	}
}

/**
 * @deprecated
 * For v.17 we need to keep this function around for backwards compatibility with the legacy UUI 1.x custom elements.
 * TODO: To be removed in v.18.
 * Fire a warning if the custom element with the provided name isn't available.
 * @func demandCustomElement
 * @param {HTMLElement} requester - Reference to the element requiring this custom element..
 * @param {string} elementName - Tag name of the required custom element.
 * @param {string} message - Optional message describing the consequences of it not begin available.
 */
export const demandCustomElement = (
	requester: HTMLElement,
	elementName: string,
	message: string = `This element has to be present for ${requester.nodeName} to work appropriate.`,
) => {
	if (!customElements.get(elementName)) {
		console.warn(
			`%c ${requester.nodeName} requires ${elementName} element to be registered!`,
			'font-weight: bold;',
			message,
			requester,
		);
	}
};

/** @deprecated Use UUITimer instead */
// TODO: Remove in v.18
export const Timer = UUITimer;

/** @deprecated Use UUIActiveMixinInterface instead */
// TODO: Remove in v.18
export type ActiveMixinInterface = UUIActiveMixinInterface;
/** @deprecated Use UUILabelMixinInterface instead */
// TODO: Remove in v.18
export type LabelMixinInterface = UUILabelMixinInterface;
/** @deprecated Use UUIPopoverTargetMixinInterface instead */
// TODO: Remove in v.18
export type PopoverTargetMixinInterface = UUIPopoverTargetMixinInterface;
/** @deprecated Use UUISelectOnlyMixinInterface instead */
// TODO: Remove in v.18
export type SelectOnlyMixinInterface = UUISelectOnlyMixinInterface;
/** @deprecated Use UUISelectableMixinInterface instead */
// TODO: Remove in v.18
export type SelectableMixinInterface = UUISelectableMixinInterface;

// TODO: Remove in v.18
export * from './uui-popover.element.js';
// TODO: Remove in v.18
export * from './UUIPopoverEvent.js';
// TODO: Remove in v.18
export * from './uui-caret.element.js';
